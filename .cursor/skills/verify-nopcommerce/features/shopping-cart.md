# Shopping cart

Shopping cart lets a shopper add one simple sample product and read that product back from the cart page and the header count.

## Sub-features

- `cart-add` adds `Pride and Prejudice` from its product page.
- `cart-header-count` shows the header cart count as `(1)`.
- `cart-page` lists the same product on `/cart/`.

## How to get to it (user POV)

- Open `Pride and Prejudice`, leave the quantity at `1`, and choose `Add to cart`.
- Open the header link whose name contains `Shopping cart`, or go to `/cart/`.

## Driving it with control-nopcommerce

Preconditions:

- `control-nopcommerce doctor` prints `ok=true`.
- The cart for this browser profile has no lines. Launch a new run if a previous recipe added a product.
- The product is `Pride and Prejudice`, which has no required attributes in the sample catalog.

- **Open the book.** Run `control-nopcommerce browser open --path /`, `control-nopcommerce browser fill --role textbox --name "Search store" --value "Pride" --within-role search`, `control-nopcommerce browser click --role button --name "Search" --within-role search`, `control-nopcommerce browser wait --text "Pride and Prejudice"`, and `control-nopcommerce browser click --role link --name "Pride and Prejudice" --selector "h2.product-title"`. The product heading is visible.
- **Add one copy.** Leave the quantity textbox and choose `Add to cart`. Run `control-nopcommerce browser wait --text "Enter a quantity"` and `control-nopcommerce browser click --role button --name "Add to cart" --within-role main`. The page shows `The product has been added to your shopping cart`. Confirm with `control-nopcommerce browser wait --text "The product has been added to your shopping cart"`.
- **Header count.** Run `control-nopcommerce browser wait --text "(1)"`. The header cart link includes that count.
- **Cart page.** Open the cart. Run `control-nopcommerce browser open --path /cart/` and `control-nopcommerce browser wait --text "Pride and Prejudice"`. The heading is `Shopping cart` and a line link is named `Pride and Prejudice`.
- **Proof.** Run `control-nopcommerce browser snapshot --aria --path cart/pride.aria.txt` and `control-nopcommerce browser screenshot --path cart/pride.png`. The artifacts show `Your store name`, `Shopping cart`, and `Pride and Prejudice`.

## Gotchas

- `Build your own computer` requires Processor, RAM, and HDD before add to cart succeeds. A notification that the product was not added is a failed add, not a cart proof.
- The header `Shopping cart` link and the product button are different controls. Scope `Add to cart` with `--within-role main`.
- The success notification is not the proof. The cart page must list the product after a fresh open of `/cart/`.
- Adding a second time changes the quantity. Use a fresh run when the expected count is `(1)`.
- An empty cart shows `Your Shopping Cart is empty!`. That text means this recipe failed.
