# Customer login

Customer login lets a shopper sign in with the seeded store admin and see that account on the customer info page.

## Sub-features

- `login-form` submits the returning-customer form.
- `login-session` replaces `Log in` with `My account` and shows `Administration` for this seeded admin.
- `login-account-page` shows the same email on the customer info page.

## How to get to it (user POV)

- Choose the header `Log in` link, or open `/login/`.
- Enter the email and password, then choose the `Log in` button.

## Driving it with control-nopcommerce

Preconditions:

- `control-nopcommerce doctor` prints `ok=true`.
- The browser profile is signed out. The header shows `Log in`.
- Credentials are email `verify@example.com` and password `Verify-store-1`.

- **Open the form.** Run `control-nopcommerce browser open --path /login/` and `control-nopcommerce browser wait --text "Returning Customer"`. The Email textbox is visible.
- **Enter the seeded admin.** Run `control-nopcommerce browser fill --role textbox --name "Email" --value "verify@example.com" --within-role main` and `control-nopcommerce browser fill --role textbox --name "Password" --value "Verify-store-1" --within-role main`.
- **Submit.** Choose the form button, not the header link. Run `control-nopcommerce browser click --role button --name "Log in" --within-role main`. The printed URL is no longer only the login form after a successful post.
- **Signed-in header.** Run `control-nopcommerce browser wait --text "My account"` and `control-nopcommerce browser wait --text "Administration"`. The header shows both. `Log in` is no longer the account action.
- **Account page.** Open the account link and read the email back. Run `control-nopcommerce browser click --role link --name "My account"` and `control-nopcommerce browser wait --text "verify@example.com"`. The heading contains `My account - Customer info`.
- **Proof.** Run `control-nopcommerce browser snapshot --aria --path login/account.aria.txt` and `control-nopcommerce browser screenshot --path login/account.png`. The artifacts show `Your store name`, `My account`, and `verify@example.com`.

## Gotchas

- The Email and Password inputs are hidden until the page script selects email mode. Wait for `Returning Customer` and fill only after the Email textbox is visible. The harness waits for a visible target before filling.
- The header `Log in` control is a link. The submit control is a button. `--role button` avoids the header link.
- `Administration` appears for the seeded admin. A registered shopper without that role will not show it. Do not treat a missing Administration link as a failed password unless the header still shows `Log in` and the page shows `Login was unsuccessful`.
- Registration is a different journey, reached from the `Register` link. Signing in does not verify registration.
- Doctor does not sign in. A login proof has to run this recipe.
