# Browse a category

Browse a category lets a shopper open `Computers` from the homepage, the header menu, or the sidebar and see that category's heading.

## Sub-features

- `category-home` opens `Computers` from the homepage category grid.
- `category-menu` opens `Computers` from the header menu.
- `category-sidebar` opens `Computers` from the sidebar list on a two-column page.

## How to get to it (user POV)

- On the homepage, choose the `Computers` link in the category grid.
- In the header menu, choose the `Computers` item.
- On a two-column page such as search, choose `Computers` under the `Categories` heading.

## Driving it with control-nopcommerce

Preconditions:

- `control-nopcommerce doctor` prints `ok=true`.
- Sample data installed the `Computers` category.

- **Homepage grid.** Open the homepage and choose `Computers` in the category grid. Run `control-nopcommerce browser open --path /` and `control-nopcommerce browser click --role link --name "Computers" --selector "section.home-page-category-grid"`. The printed URL contains `/computers` and the heading `Computers` is visible. Confirm with `control-nopcommerce browser wait --text "Computers"`.
- **Header menu.** Return home and choose the menu item. Run `control-nopcommerce browser open --path /` and `control-nopcommerce browser click --role button --name "Computers" --selector "nav.menu-container"`. The printed URL contains `/computers`.
- **Sidebar.** Open search, then choose the sidebar link. Run `control-nopcommerce browser open --path /search/` and `control-nopcommerce browser click --role link --name "Computers" --selector "section.block-category-navigation"`. The heading `Computers` is visible.
- **Proof.** Capture the category page after one of the entries above. Run `control-nopcommerce browser snapshot --aria --path category/computers.aria.txt` and `control-nopcommerce browser screenshot --path category/computers.png`. The artifacts show `Your store name` and the heading `Computers`. Record which entry point produced them. The other entry points stay unverified until their own commands are run.

## Gotchas

- The homepage grid is a link. The header menu item is a button with an href, because the menu markup sets `role="button"` on the anchor. Do not click a generic `Computers` target without the selector in the command.
- A menu click that only expands the submenu and leaves the URL at `/` has not opened the category. Report that command as failed.
- The homepage uses a one-column layout, so the sidebar `Categories` block is absent there. Open `/search/` before the sidebar command.
- Subcategories such as `Desktops` are a different page. This recipe stops at the `Computers` heading.
