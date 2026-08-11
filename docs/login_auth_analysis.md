# Login Page & Auth System — Gap Analysis & Design Proposal

## 1. Current State Audit

### 1.1 What the Login Page Looks Like (screenshot)
The current login page (`Login.cshtml`) is a minimal, centred white card on a grey background (`var(--canvas)`).
It contains:
- An avatar image (static `admin.png` — not user-aware)
- A hardcoded **"Admin"** label beneath the avatar
- Username and password fields with inline JS validation
- A green **"Login"** button
- A blue biometrics button (recently added, inline style, not themed from tokens)
- A plain-text "Forgot password? Contact **God Admin**" link to a hardcoded `mailto:`

### 1.2 What Already Exists (do not re-implement)

| Feature | Location | Status |
|---|---|---|
| Password hash (BCrypt Enhanced) | `UserPassword.cs` + `AuthenticationService` | ✅ Fully implemented |
| Login via username | `AuthenticationService.LoginAsync` | ✅ |
| Login via email | `AuthenticationService.LoginWithEmailAsync` | ✅ (backend only, no UI) |
| Login via phone | `AuthenticationService.LoginWithPhoneAsync` | ✅ (backend only, no UI) |
| Biometric login (FIDO2/WebAuthn) | `WebAuthnController` + `webauthn.js` | ✅ |
| JWT access + refresh token flow | `AuthenticationService` + `UserToken` | ✅ |
| Logout (revoke token) | `AuthenticationService.LogoutAsync` | ✅ |
| Self-service password change (from Profile) | `AuthenticationService.ResetPasswordAsync` | ✅ (requires old password) |
| OTP entity + `OtpPurpose.PasswordReset` enum | `Otp.cs` + `StoreEnums.cs` | ✅ Schema ready, no service |
| `PasswordResetToken` DB migration | `20260810181907_AddPasswordRecovery` | ✅ Migration exists |
| Toast notification system | `site.js` + `components.css` | ✅ |
| AppDialog (modal confirmations) | `site.js` + `components.css` | ✅ |
| User email/phone contacts on the user model | `UserEmail.cs`, `UserPhone.cs` | ✅ Schema ready |

> [!IMPORTANT]
> A migration for `PasswordResetToken` already exists (`AddPasswordRecovery`), but **the C# entity class is missing** from `Store.Models/Entities/`. This means the migration was generated then the entity was deleted/moved, or it was done from a temporary branch. This is a gap that must be addressed.

---

## 2. Login Page — Gaps & Issues

### 2.1 Design / UX Issues

| # | Issue | Severity |
|---|---|---|
| 1 | **Avatar is always `admin.png`** — hardcoded, not dynamic. Every user sees "Admin" and the admin avatar regardless of who they are. | High |
| 2 | **Biometrics button uses inline styles** (`style="margin-top:10px; background-color:..."`) instead of design token CSS classes, breaking consistency | Medium |
| 3 | **"Forgot password?" is a dead `mailto:` link** pointing to `admin@clexan.local`. No actual recovery flow exists. | High |
| 4 | **No password visibility toggle** (`👁` show/hide). This is standard in modern login UIs. | Medium |
| 5 | **No loading state** on the Login button after form submit (user doesn't know if the request is in-flight) | Low–Medium |
| 6 | **Error message is a raw `<p>` tag** from server-rendered TempData. It clears on refresh, which is correct, but its styling resets a border-box. Consider using the existing `.auth-error` component properly. | Low |
| 7 | **Username is pre-filled with "Admin"** — the avatar label and `avatarText` `<span>` are hardcoded. They should be blank until the user types, or dynamically fetched. | Medium |
| 8 | **No branding consistency** — The login card is `Layout = null` with plain `<body>` — no brand colour, gradient, or any reference to the app's dark premium visual identity. The landing page (`landing.css`) and dashboard (`_AppLayout.cshtml`) are worlds apart from the login page | High |
| 9 | **Login page has stray `</div></div>` tags at the end** (lines 111–112 of `Login.cshtml`) outside the `</body></html>` — invalid HTML | Medium |
| 10 | **The biometrics button SVG icon is wrong** — it shows a clock hand, not a biometric/fingerprint icon | Low |

### 2.2 Missing Features

| # | Feature | Exists Elsewhere? |
|---|---|---|
| A | **Forgot Password flow** (two-method: email/SMS vs admin-issued temp password) | Schema partially there |
| B | **Forced password reset page** (for temp/expired passwords) | Not implemented |
| C | **Account lockout** after N failed attempts | Not implemented |
| D | **Username-aware avatar** — fetch user avatar after username is typed (on blur) | Possible via existing API |
| E | **"Remember me"** (extend refresh token) | Not implemented |
| F | **Session expiry notification** — toast or modal when JWT expires mid-session | Not implemented |

---

## 3. Password Recovery — Detailed Design

### 3.1 The Two Recovery Methods

Admins should be able to configure which method to allow via a system-settings table or `appsettings.json`. Here is what each method means:

#### **Method A — Self-Service via OTP (Email / Phone / WhatsApp)**
User initiates from the login page → system generates a time-limited OTP → sends via configured channel → user enters OTP on a "Verify OTP" page → redirected to "Set New Password" page.

| Step | Detail |
|---|---|
| Initiation | User clicks "Forgot Password" → enters username or email |
| OTP generation | System creates a 6-digit OTP with 15-minute expiry, stored in the `Otp` table with `Purpose = OtpPurpose.PasswordReset` |
| Delivery | Send via Email (SMTP/SendGrid), SMS (Twilio), or WhatsApp (Twilio/Meta) |
| Verification | User enters OTP on `/ForgotPassword/Verify` page |
| Reset | On valid OTP, redirect to `/ResetPassword?token=<secure_token>` where they set a new password |
| Security | OTP is single-use (`IsUsed = true` after verification), rate-limited (max 3 per hour), and invalidated if a new one is generated |

#### **Method B — Admin-Issued Temporary Password**
User requests a reset → admin sees a pending request in the admin panel → admin issues a one-time temporary password → user logs in with it → they are **forced** to set a new password before proceeding.

| Step | Detail |
|---|---|
| Initiation | User clicks "Forgot Password" → submits a reset request (stored in DB) |
| Admin action | Admin visits Users page → sees a "Reset Requested" badge → clicks "Issue Temp Password" |
| Generation | System generates a cryptographically random temporary password (e.g. `Xk3!mR9p`) |
| Delivery | Admin copies and communicates it to user via any out-of-band channel |
| Login | User logs in with the temp password — system detects `IsTempPassword = true` |
| Forced reset | System redirects user to `/ForceResetPassword` — they cannot access the dashboard until they set a new password |
| Expiry | Temp passwords expire after a configurable duration (e.g., 24h or 72h). **If expired, login still redirects to `/ForceResetPassword`** but shows "Your temporary password has expired" — they cannot proceed and must request another reset. |

### 3.2 System Settings — Method Toggle

A new `SystemSetting` entity (or configuration entry) will control this:

```
PasswordRecoveryMethod = "OTP" | "TempPassword" | "Both"
```

`"Both"` enables both methods simultaneously — the login page shows both options and the admin panel shows both flows.

### 3.3 Data Model Changes Needed

#### Entities to add or fix
| Entity | Action | Notes |
|---|---|---|
| `PasswordResetToken` | **[CREATE]** entity class is missing from `Store.Models`. Migration exists but entity was lost. | Needs: `TokenId`, `UserId`, `TokenHash`, `ExpiresAt`, `IsUsed`, `Purpose` (OTP/TempPassword) |
| `UserPassword` | **[MODIFY]** add `IsTempPassword bool`, `TempPasswordExpiresAt DateTime?` | Enables forced-reset detection |
| `SystemSetting` | **[CREATE]** or use `appsettings.json` | `Key` + `Value` pattern |
| `PasswordResetRequest` | **[CREATE]** (Method B only) | Tracks pending requests visible to admin |

#### New `UserStatus` enum value
Add `ForcePasswordReset` to `UserStatus` or use a boolean flag on `UserPassword`. The flag approach is simpler and doesn't affect role-based auth flows.

### 3.4 Flow Pages Required

| Page | Route | Purpose |
|---|---|---|
| Forgot Password | `/ForgotPassword` | Entry — user types username; system selects method |
| OTP Verify | `/ForgotPassword/Verify` | Method A: user enters 6-digit OTP |
| Set New Password | `/ResetPassword` | Common: user picks and confirms their new password |
| Force Reset | `/ForceResetPassword` | Redirect target when temp password is used or expired |

### 3.5 Security Constraints

- All reset token/OTP hashes stored as SHA-256 in DB (never plain text)
- OTPs: rate-limited 3/hour per user, 15-minute expiry, single-use
- Temp passwords: expire in 24–72h (configurable), single-use — expire immediately after login
- Reset links/tokens: signed, time-limited, single-use
- All reset endpoints enforce anti-forgery tokens
- Login must check temp-password expiry on every attempt; redirect even on expired temp passwords
- Audit log entry created for every reset event (requestd, issued, used, expired)

---

## 4. Visual / Design Consistency Plan

The current login card is disconnected from the rest of the app.
The upgraded login page should:

### 4.1 Design Tokens to Apply
Use the existing `tokens.css` variables throughout:
- Background: `--canvas` (`#f0f3f1`) with a subtle brand-tinted radial glow (like the landing page)
- Card: `--surface` + `--shadow-md`
- Primary button: `--brand` green, `--brand-dark` on hover
- Secondary button (Biometrics): `--surface` with `--border` + brand text (not hardcoded blue)
- Text: `--text-primary`, `--text-secondary`
- Inputs: existing `input:not(.browser-default)` styles already apply
- Error: `.auth-error` class already styled

### 4.2 Specific UI Improvements
- Replace static avatar with a dynamic user avatar that loads after username blur
- Add a proper fingerprint SVG icon to the biometrics button
- Add a password show/hide toggle (eye icon)
- Add a subtle loading spinner/animation on the Login button during form submission
- Make the "Forgot password?" link open a styled `/ForgotPassword` page instead of `mailto:`
- Add micro-animations (card fade-in, subtle input focus glow using `--focus-ring`)
- Clean up the duplicate closing tags in the HTML

---

## 5. Summary of Work Required

### Phase 1 — Login Page Polish (UI only, no backend)
- [ ] Fix duplicate closing HTML tags (lines 111–112)
- [ ] Replace hardcoded avatar/username with dynamic fetch on username blur
- [ ] Restyle biometrics button using design tokens (remove inline styles)
- [ ] Replace biometrics clock icon with a fingerprint SVG
- [ ] Add password show/hide toggle
- [ ] Add login button loading state
- [ ] Add card fade-in animation and background glow for brand consistency
- [ ] Replace "Contact God Admin" mailto with a link to `/ForgotPassword`

### Phase 2 — Forgot Password Backend & Pages
- [ ] Create missing `PasswordResetToken` entity in `Store.Models`
- [ ] Add `IsTempPassword` + `TempPasswordExpiresAt` to `UserPassword`
- [ ] Add `PasswordRecoveryMethod` to `SystemSetting` or `appsettings`
- [ ] Implement `IPasswordRecoveryService` with:
  - `RequestResetAsync()` — Method A: generates OTP, sends notification
  - `VerifyOtpAsync()` — validates OTP, returns signed reset token
  - `IssueTempPasswordAsync()` — Method B: admin issues temp password
  - `SetNewPasswordAsync()` — validates reset token, updates password hash, clears temp flags
- [ ] Add `ForceResetPassword` detection to `AuthenticateUser` in `AuthenticationService`
- [ ] Add auth middleware intercept for temp-password users (redirect to `/ForceResetPassword`)
- [ ] Create Razor pages: `/ForgotPassword`, `/ForgotPassword/Verify`, `/ResetPassword`, `/ForceResetPassword`
- [ ] Add "Reset Requested" UI to the Users admin page
- [ ] Implement "Issue Temp Password" action on Users page

### Phase 3 — Account Lockout (Future)
- [ ] Add `FailedLoginAttempts int` + `LockoutUntil DateTime?` to `User`
- [ ] Lock account after 5 failed attempts for 15 minutes
- [ ] Display lockout message and remaining time on login page

---

> [!NOTE]
> All new pages should use `Layout = null` with the same card-container pattern as Login so they feel like a cohesive auth flow — not pages pulled from inside the dashboard.
