# View a product

View a product lets a shopper open a sample product from search results and read its name on the product page.

## Sub-features

- `product-open` opens `Pride and Prejudice` from its search result link.
- `product-identity` shows that name as the product heading and keeps the store identity visible.

## How to get to it (user POV)

- Search for `Pride`, then choose the `Pride and Prejudice` product link.
- From a category or the homepage, choose the product name link on a product tile.

## Driving it with control-nopcommerce

Preconditions:

- `control-nopcommerce doctor` prints `ok=true`.
- Sample data published `Pride and Prejudice`.

- **Find the book.** Search from the header. Run `control-nopcommerce browser open --path /`, `control-nopcommerce browser fill --role textbox --name "Search store" --value "Pride" --within-role search`, `control-nopcommerce browser click --role button --name "Search" --within-role search`, and `control-nopcommerce browser wait --text "Pride and Prejudice"`. The printed URL contains `q=Pride`.
- **Open the product.** Choose the product link. Run `control-nopcommerce browser click --role link --name "Pride and Prejudice" --selector "h2.product-title"`. The printed URL changes away from `/search`.
- **Read the heading.** Run `control-nopcommerce browser wait --text "Pride and Prejudice"`. The product heading is `Pride and Prejudice`.
- **Proof.** Run `control-nopcommerce browser snapshot --aria --path product/pride.aria.txt` and `control-nopcommerce browser screenshot --path product/pride.png`. The artifacts show `Your store name` and the heading `Pride and Prejudice`.

## Gotchas

- Search can list the same name on more than one anchor. Scope the click to `h2.product-title` so the tile title is the control that is activated.
- `Build your own computer` is a valid product page, and its heading is that name. It is a poor stand-in for a later add-to-cart proof because its required attributes block a plain add. This recipe only checks that the page shows the name.
- The product URL is the sample slug generated at install. Follow the link. Do not invent the slug.
