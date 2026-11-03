# nopCommerce storefront verification map

This directory is the maintained source for verifying the public nopCommerce storefront. Read this index before driving the app, then use the matching feature file as the recipe.

## Baseline preconditions

- Run `control-nopcommerce launch` and keep the printed `url`, `runId`, and `database`.
- The store is the sample-data catalog. `Your store name` is the logo alt text. `Computers` and `Build your own computer` come from that install.
- Admin shopper credentials are email `verify@example.com` and password `Verify-store-1`.
- Run `control-nopcommerce doctor` and require `ok=true`, the same url, and the same database.
- Put `control-nopcommerce` on `PATH`.
- Never drive an instance that was not started by this verification run. Pass `--run <runId>` when more than one run exists.

## Driving conventions

- Start every recipe from the baseline state unless its preconditions say otherwise.
- Prefer ARIA roles and accessible names. Use `--selector` only to scope a role to a landmark that the feature names.
- Treat every command as literal. Keep quoted names and flags unchanged.
- Run browser actions through `control-nopcommerce browser`.
- A command prints the URL it landed on. The next command continues from that URL and the same browser profile.
- Restore nothing by editing the database. Start a new launch when a recipe needs a clean catalog.
- Do not remove proof artifacts during cleanup.

## Proof and skip reporting

- Capture the shopper action and the resulting state, not only the final screen.
- UI proof includes an ARIA snapshot and a full-page screenshot with `Your store name` visible.
- Mutation proof includes a second page that shows the stored result.
- Record the feature id, the entry point, and the run id with every artifact.
- Report an unreachable path with the attempted command and the unmet precondition.
- Do not report a skipped entry point as verified through a different path.

## Feature entry contract

Each feature file starts with an H1 title and one paragraph describing the shopper-visible behavior. It then uses exactly four H2 sections in this order.

1. `Sub-features` lists short IDs with one line for each behavior.
2. `How to get to it (user POV)` lists every shopper entry point.
3. `Driving it with control-nopcommerce` starts with `Preconditions:` and uses labeled bullets that pair each shopper action with an exact command and observable result.
4. `Gotchas` lists traps that can waste or invalidate a verification run.

Keep implementation details out of the map. Name only shopper paths, stable handles, required state, commands, and observable proof.

## Features

- [Search the catalog](./search-catalog.md) covers header search, a matching product, an empty query, and the search page form.
- [Browse a category](./browse-category.md) covers the homepage category grid, the header menu, and the sidebar category list.
- [View a product](./product-details.md) covers opening a sample product and reading its name back from the product page.
- [Shopping cart](./shopping-cart.md) covers adding a simple product and reading it back from the cart page.
- [Customer login](./customer-login.md) covers signing in as the seeded store admin and reading the account page.
