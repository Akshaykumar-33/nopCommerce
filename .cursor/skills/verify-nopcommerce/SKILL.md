---
name: verify-nopcommerce
description: Drive a disposable nopCommerce storefront in Chrome to prove catalog search, category browsing, product details, cart, and customer login. Use when a change touches the public store, installation, or sample catalog and needs end-to-end evidence.
---

# Verify nopCommerce

The primary surface is the public storefront, a server-rendered web UI. The admin area at `/Admin` and the Docker Compose stack (`docker-compose.yml`, SQL Server, port 80) are separate surfaces. This skill does not drive those. Compose binds one shared database and port 80, so it cannot host two isolated stores.

The harness is `control-nopcommerce`. Put it on `PATH` for the session:

```bash
export PATH="$PWD/.cursor/skills/verify-nopcommerce/scripts:$PATH"
```

Run it from the nopCommerce checkout. It refuses to open any origin other than the instance it started.

## Launch

Prerequisites the script checks for, and does not install:

- .NET 10 SDK (`dotnet` on `PATH`, or `DOTNET_ROOT`)
- PostgreSQL listening on `127.0.0.1:5432`, with `citext` available (`postgresql-contrib`) and password auth for host connections
- Node.js, npm, and Google Chrome
- `sudo -n -u postgres psql` for creating and dropping disposable databases

If PostgreSQL is installed but stopped, launch tries `sudo pg_ctlcluster 16 main start` and then `sudo service postgresql start`.

Launch publishes the solution once into `.cursor/skills/verify-nopcommerce/.publish`, then copies that output into an isolated content root. A later launch reuses the publish when the git commit, `Nop.Web` product version, and `Nop.Web.dll` hash still match. Set `NOP_VERIFY_REBUILD=1` to publish again.

```bash
control-nopcommerce launch
```

What launch does, in order:

1. Publish with `dotnet build src/NopCommerce.sln -c Release -m:2` and `dotnet publish src/Presentation/Nop.Web/Nop.Web.csproj -c Release` into the cache above.
2. Create database `nopverify<runId>`, owned by the local superuser role `nop_verify`, and create the `citext` and `pgcrypto` extensions. The role password is random and stored only in `/tmp/nopcommerce-verify/pg-role.json` mode `600`.
3. Copy the publish to `/tmp/nopcommerce-verify/runs/<runId>/app` and delete any copied `App_Data/appsettings.json`.
4. Start `dotnet Nop.Web.dll` bound to `http://127.0.0.1:<free port>` beginning at `18080`. The pid is recorded in that run's `state.json`.
5. Wait until `GET /install` returns the form `installation-form`.
6. Fill that form and submit it: admin email `verify@example.com`, admin password `Verify-store-1`, database host `127.0.0.1`, the new database name, user `nop_verify`, provider `PostgreSQL`. It checks **Create sample data**, clears **Subscribe to nopCommerce newsletter**, leaves **Create database if it doesn't exist** unchecked, and selects the empty country option when that list is shown. The empty country keeps the install on `en-US`, so nopCommerce does not download a language pack.
7. Before submit, launch fails if sample data is unchecked or the newsletter box is still checked. After submit, it fails if the POST body is missing `InstallSampleData=true` or contains `SubscribeNewsletters=true`. Those checks are the observation that this install does not call nopCommerce's newsletter endpoint. The install page may still request Google Fonts; that request is not a newsletter subscription.
8. Wait until `App_Data/appsettings.json` contains a PostgreSQL connection for this database. Installation then stops the process on purpose (`RestartAppDomain`). Launch starts that same content root again.
9. Ready means `GET /` is the store: the body contains `Your store name`, `Computers`, and `Powered by`, and it does not contain `installation-form`. A listening port while the install page is still showing is not ready.

Launch prints `ok=true`, `runId`, `url`, `pid`, `database`, `contentRoot`, `evidenceDir`, `dllSha256`, `productVersion`, `commit`, and `adminEmail`. The admin password is the fixture value above and is also printed by `control-nopcommerce credentials`.

Two launches can run side by side. Each has its own port, content root, browser profile, and database. They may share one PostgreSQL server and the `nop_verify` role. Do not point a run at an existing store's `App_Data` or database. Do not drive an instance this harness did not start.

`control-nopcommerce doctor` and `browser` use the run id in `/tmp/nopcommerce-verify/current` unless `--run <id>` is set.

## Doctor

Doctor is read-only. Run it before a drive whenever the store looks wrong.

```bash
control-nopcommerce doctor
```

It exits `0` only when all of these are true:

- The recorded pid is alive, its command line contains `Nop.Web.dll`, and `/proc/<pid>/cwd` is this run's content root.
- That pid owns the listening socket for the recorded port.
- `Nop.Web.dll` in the content root hashes to the launch `dllSha256`, the csproj product version matches, and `git rev-parse HEAD` is the launch commit. Set `NOP_VERIFY_ALLOW_STALE=1` only when a commit landed after launch and you intentionally keep the old process.
- `App_Data/appsettings.json` has `ConnectionStrings.DataProvider` `PostgreSQL` and a connection string whose database is this run's database.
- `GET /` returns 200 with `Your store name`, `Computers`, and `Powered by`, and without `installation-form`.
- `GET /login/` returns 200 and contains `Returning Customer`. Doctor does not sign in.

Stdout includes `ok=true` or `ok=false`, plus `url`, `pid`, `port`, `database`, `contentRoot`, `dllSha256`, `productVersion`, `commit`, `adminEmail`, and `evidenceDir`. Failures are lines starting with `failure=`.

## Drive

Read `features/README.md`, then follow one feature file exactly. Browser state (cookies and the last URL) is the run's Chrome profile under the content run directory, so separate command invocations continue one shopper session. Each command reopens that URL in a fresh page, then replays any `fill` values recorded for it, so a `fill` followed by a later `click` still submits the typed text. An `open` clears those pending fills.

Stable handles used by the map:

- Header search landmark: `role="search"` in `Views/Shared/_Header.cshtml`. The textbox accessible name is `Search store` (`#small-searchterms`). The button name is `Search`.
- Search results page: heading `Search`, keyword textbox `Search keyword`, button `Search` inside `<main>`.
- Product tiles: link whose accessible name is the product name, from `h2.product-title a` in `Views/Shared/_ProductBox.cshtml`.
- Product page: heading is the product name. Quantity textbox accessible name is `Enter a quantity`. Button name is `Add to cart` (`#add-to-cart-button-<id>`).
- Header cart link accessible name contains `Shopping cart`.
- Header account links: `Log in`, `My account`, `Log out`, and for the seeded admin `Administration`.
- Login form: textboxes `Email` and `Password`, submit button `Log in`. The inputs start hidden and the page script shows email mode; wait until the Email textbox is visible.
- Homepage categories: link `Computers` inside `section.home-page-category-grid`.
- Header menu: button `Computers` inside `nav.menu-container`.
- Sidebar categories: link `Computers` inside `section.block-category-navigation`.
- Cookie bar, only for European install regions: button `#eu-cookie-ok`. The harness clicks it when it is visible. This `en-US` install does not show it.

Commands:

```bash
control-nopcommerce browser open --path /
control-nopcommerce browser fill --role textbox --name "Search store" --value "computer" --within-role search
control-nopcommerce browser click --role button --name "Search" --within-role search
control-nopcommerce browser click --role link --name "Computers" --selector "section.home-page-category-grid"
control-nopcommerce browser wait --text "Build your own computer"
control-nopcommerce browser absent --text "Pride and Prejudice"
control-nopcommerce browser url
control-nopcommerce browser snapshot --aria --path search/results.aria.txt
control-nopcommerce browser screenshot --path search/results.png
```

`--selector` scopes a `--role` search. `--within-role search` limits the action to the header search landmark. `--within-role main` limits it to the page body, which is how the search-page button is distinguished from the header button.

Every browser command reopens the last URL from this run, except `open`, which navigates first. `click` waits for the document load that the click starts. `wait` then waits until the named text is visible, which is required for catalog results because `Views/Catalog/_CatalogProducts.cshtml` loads products through `CatalogProducts` after the document loads.

## Evidence

Write proof under `.cursor/skills/verify-nopcommerce/evidence/<runId>/`. `snapshot` and `screenshot` place relative `--path` values there and print the absolute file. Cleanup never deletes that directory.

A proof covers the shopper path, not an internal setter or a test-only endpoint:

- Capture the action and the resulting state. A status message alone is not enough.
- UI proof is an ARIA snapshot plus a full-page screenshot. The snapshot file begins with the URL and document title. The store identity on the page is the logo alt text `Your store name` and the footer text `Powered by nopCommerce`.
- For search, the snapshot must show the query and the matching product, and it must not be the only artifact. The following `browser url` output is part of the proof.
- For a mutation such as add to cart or login, open a second page that reads the new state back: `/cart/` for the cart line, and `My account` for the signed-in email.
- Sample data is the real install path (`Create sample data`), not a fixture injected into the database.

## Cleanup

```bash
control-nopcommerce cleanup
control-nopcommerce cleanup --run <runId>
```

Cleanup sends `SIGTERM`, then `SIGKILL` if needed, to the recorded pid's process group. It does not kill by process name. It terminates connections to that run's `nopverify...` database and drops it. It deletes `/tmp/nopcommerce-verify/runs/<runId>/` (content root, log, browser profile) and clears `current` when it points at that run. It leaves `evidence/<runId>/` in place.

Stdout reports `cleaned=true` and `evidenceRemains=true` when the evidence directory is still there. Run cleanup after a failed launch too. Launch already does that for the run it was creating.

## Helpers

| Command | Effect |
|---|---|
| `control-nopcommerce launch` | Publish if needed, install a disposable store with sample data, print the run id and URL |
| `control-nopcommerce doctor` | Read-only health check. Exit 0 only when the instance is this run's store |
| `control-nopcommerce credentials` | Print `adminEmail` and `adminPassword` for the current run |
| `control-nopcommerce browser ...` | One Chrome action against the current run, then exit |
| `control-nopcommerce cleanup` | Stop the process and drop the database. Keep evidence |

The script is `.cursor/skills/verify-nopcommerce/scripts/control-nopcommerce`. Playwright is installed into the skill directory on first browser use with `PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1`, and Chrome is launched with `--no-sandbox` because the harness runs headless. `NOP_VERIFY_HEADED=1` shows the window.
