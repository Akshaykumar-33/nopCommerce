# Search the catalog

Search lets a shopper find a sample product from the header box, open the search page, and see a clear empty state when nothing matches.

## Sub-features

- `search-header` submits the header search box.
- `search-match` shows `Build your own computer` for the query `computer` and does not show `Pride and Prejudice`.
- `search-empty` shows the empty catalog message for a query with no matches.
- `search-page` repeats a query from the search page form.

## How to get to it (user POV)

- Type in the header box labeled `Search store` and choose the `Search` button inside that search landmark.
- Open `/search/` and submit the form whose keyword field is labeled `Search keyword`.

## Driving it with control-nopcommerce

Preconditions:

- `control-nopcommerce doctor` prints `ok=true`.
- The sample catalog is installed. `Build your own computer` and `Pride and Prejudice` are published products.
- The browser profile has no typed search yet.

- **Open the store.** Go to the homepage. Run `control-nopcommerce browser open --path /`. The logo alt text is `Your store name`.
- **Header query.** Type `computer` into the header search box and choose `Search`. Run `control-nopcommerce browser fill --role textbox --name "Search store" --value "computer" --within-role search` and `control-nopcommerce browser click --role button --name "Search" --within-role search`. The printed URL contains `/search` and `q=computer`.
- **Matching product.** Wait until the result is visible. Run `control-nopcommerce browser wait --text "Build your own computer"`. A product link named `Build your own computer` is on the page.
- **Unrelated product stays out.** Run `control-nopcommerce browser absent --text "Pride and Prejudice"`. That title is not visible.
- **Proof.** Capture the result state. Run `control-nopcommerce browser snapshot --aria --path search/results.aria.txt` and `control-nopcommerce browser screenshot --path search/results.png`. Both artifacts show `Your store name`, the query `computer`, and `Build your own computer`. The snapshot file's `url:` line contains `/search` and `q=computer`.
- **Empty query.** On the search page, replace the keyword with `volcano` and choose the page `Search` button. Run `control-nopcommerce browser fill --role textbox --name "Search keyword" --value "volcano" --within-role main` and `control-nopcommerce browser click --role button --name "Search" --within-role main`. The printed URL contains `q=volcano`.
- **Empty state.** Wait for the catalog message. Run `control-nopcommerce browser wait --text "No products were found that matched your criteria."`. The message is visible and `Build your own computer` is not required to remain.

## Gotchas

- The header button and the search-page button are both named `Search`. Scope the header button with `--within-role search` and the page button with `--within-role main`.
- Results are filled by the catalog script after the document loads. Wait for the product link or the empty message. Do not treat the first URL change as the result list.
- The header form sends `q`. Assert that parameter in the printed URL from the Search click before waiting for product text. The homepage also shows `Build your own computer` in the featured grid, so a wait on `/` is not a search proof.
- `Build your own computer` is the only sample product whose name contains `computer`. A description match for an unexpected product means the assertion `absent` for `Pride and Prejudice` is still required, and a new visible product needs its own note in this file.
- Autocomplete can open under the header box. Submit the form anyway. The proof is the search results page, not the suggestion list.
- `fill` and `click` are separate process invocations. The harness replays the typed value when it reopens the page; do not replace that sequence with a direct `/search?q=...` open.
