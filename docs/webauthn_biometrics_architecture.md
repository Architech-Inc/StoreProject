# Biometric Authentication Architecture (WebAuthn)

To achieve native biometric authentication across all operating systems directly from the web browser, we must use the **WebAuthn (FIDO2)** standard. 

WebAuthn bridges the gap between the web application and the device's native "Platform Authenticator" (Windows Hello on Windows, Touch ID / Face ID on macOS/iOS, and fingerprint/face scanners on Android and Linux).

Here is exactly what we need to implement this across the stack.

---

## 1. The Technology Stack

### Frontend (Browser API)
The frontend utilizes the native `navigator.credentials` JavaScript API. 
- **No third-party libraries needed**: It is built directly into all modern browsers (Edge, Chrome, Safari, Firefox).
- **Registration**: Calls `navigator.credentials.create()` to ask the user's OS to generate a new cryptographic key pair bound to the device.
- **Login**: Calls `navigator.credentials.get()` to ask the OS to verify the user via biometrics and sign a challenge.

### Backend (.NET Core API)
The backend must issue cryptographic challenges, verify digital signatures, and store public keys. We do not write this cryptography from scratch.
- **Recommended Library**: `Fido2NetLib` (A widely used, certified .NET library for FIDO2/WebAuthn).
- **Purpose**: Parses the attestation/assertion objects returned by the browser and verifies they are cryptographically sound.

---

## 2. Database Schema Changes

Biometric authentication uses Public Key Cryptography. Your server never sees the user's biometric data (fingerprint or face). Instead, the OS securely holds a Private Key in the hardware TPM/Secure Enclave and gives your database the Public Key.

We will need a new table (e.g., `UserCredentials`) linked to the `Users` table with the following fields:
- `CredentialId` (byte[]): Unique identifier for the key.
- `PublicKey` (byte[]): The public key used to verify signatures.
- `UserHandle` (byte[]): A unique, non-identifying user ID.
- `SignatureCounter` (int): Prevents replay attacks by ensuring the signature count always increases.
- `Aaguid` (Guid): Identifies the type of authenticator used.
- `UserId` (Guid): Foreign key to your existing `User` entity.

---

## 3. The Workflows

### Phase 1: Registration (Enrolling a Device)
1. User logs in with their password as usual.
2. User goes to **Profile > "Setup Biometrics"**.
3. **Server** generates a random cryptographic `Challenge` and sends it to the frontend.
4. **Browser** triggers the OS prompt (e.g., Windows Hello pops up). The user scans their face/fingerprint.
5. **OS** creates a new Private/Public key pair securely in the hardware TPM.
6. **Browser** sends the Public Key and signed challenge back to the Server.
7. **Server** validates the signature and saves the Public Key to the database.

### Phase 2: Login (Authenticating)
1. User clicks "Login with Biometrics" and enters their Username.
2. **Server** generates a `Challenge` and looks up the `CredentialIds` associated with that Username.
3. **Browser** passes the challenge to the OS. Windows Hello prompts the user to scan.
4. **OS** signs the challenge using the hardware Private Key.
5. **Browser** sends the signature back to the Server.
6. **Server** verifies the signature using the stored Public Key. If it matches, the server issues the JWT!

---

## 4. Strict Requirements & Gotchas

> [!WARNING]
> **HTTPS is Strictly Enforced**
> WebAuthn will absolutely refuse to run on standard HTTP connections, even in development, except for `localhost`. If you test this on a local network IP (e.g., `http://192.168.1.5`), the browser will block the API.

> [!NOTE]
> **Platform vs. Roaming Authenticators**
> - **Platform Authenticators**: Tied to the specific device (Windows Hello, Touch ID). If a user buys a new laptop, they must register that new laptop.
> - **Roaming Authenticators**: YubiKeys or security keys. The WebAuthn API supports these seamlessly using the exact same code!

> [!TIP]
> **Account Recovery**
> Because platform keys are tied to a specific device, users will lose access if they lose the device. You must always maintain a fallback login method (like Password or Email Magic Link) to allow them to register a new device.
