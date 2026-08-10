// Helper to convert base64url to ArrayBuffer
function coerceToArrayBuffer(str) {
    if (typeof str === 'string') {
        str = str.replace(/-/g, '+').replace(/_/g, '/');
        const padLen = (4 - (str.length % 4)) % 4;
        str += '='.repeat(padLen);
        const decoded = atob(str);
        const buf = new Uint8Array(decoded.length);
        for (let i = 0; i < decoded.length; i++) {
            buf[i] = decoded.charCodeAt(i);
        }
        return buf.buffer;
    }
    return str;
}

// Helper to convert ArrayBuffer to base64url
function coerceToBase64Url(thing) {
    let buf;
    if (thing instanceof ArrayBuffer) {
        buf = new Uint8Array(thing);
    } else if (thing instanceof Uint8Array) {
        buf = thing;
    } else if (thing !== null && typeof thing === 'object' && thing.buffer) {
        buf = new Uint8Array(thing.buffer, thing.byteOffset, thing.byteLength);
    } else {
        throw new Error('Could not coerce to Uint8Array');
    }
    let str = '';
    for (let i = 0; i < buf.byteLength; i++) {
        str += String.fromCharCode(buf[i]);
    }
    return btoa(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
}

async function registerBiometrics() {
    try {
        const fetchOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': `Bearer ${window.localStorage.getItem('token')}` }
        };
        
        // 1. Get MakeCredentialOptions from Server
        const response = await fetch('/api/webauthn/makeCredentialOptions', fetchOptions);
        if (!response.ok) throw new Error('Failed to get credential options');
        const makeCredentialOptions = await response.json();

        // 2. Coerce string byte arrays back into ArrayBuffers
        makeCredentialOptions.challenge = coerceToArrayBuffer(makeCredentialOptions.challenge);
        makeCredentialOptions.user.id = coerceToArrayBuffer(makeCredentialOptions.user.id);
        if (makeCredentialOptions.excludeCredentials) {
            for (let cred of makeCredentialOptions.excludeCredentials) {
                cred.id = coerceToArrayBuffer(cred.id);
            }
        }

        // 3. Prompt OS Biometrics
        const newCredential = await navigator.credentials.create({
            publicKey: makeCredentialOptions
        });

        // 4. Send response to Server
        const makeCredentialResponse = {
            id: newCredential.id,
            rawId: coerceToBase64Url(newCredential.rawId),
            type: newCredential.type,
            clientExtensionResults: newCredential.getClientExtensionResults(),
            response: {
                attestationObject: coerceToBase64Url(newCredential.response.attestationObject),
                clientDataJSON: coerceToBase64Url(newCredential.response.clientDataJSON),
                transports: newCredential.response.getTransports ? newCredential.response.getTransports() : []
            }
        };

        const regRes = await fetch('/api/webauthn/makeCredential', {
            ...fetchOptions,
            body: JSON.stringify(makeCredentialResponse)
        });

        if (regRes.ok) {
            window.showToast?.('success', 'Biometrics successfully registered!');
        } else {
            const errData = await regRes.json().catch(() => ({}));
            console.error('Registration server response error:', errData);
            let errMsg = errData.message || 'Server rejected credential';
            if (errData.errors && Array.isArray(errData.errors)) {
                errMsg += '\nDetails: ' + errData.errors.join(', ');
            }
            window.showToast?.('error', 'Failed to register biometrics: ' + errMsg);
        }
    } catch (e) {
        console.error('WebAuthn Error:', e);
        window.showToast?.('error', 'Biometric registration failed or was cancelled: ' + e.message);
    }
}

async function loginBiometrics() {
    try {
        const usernameInput = document.getElementById('inputUserName');
        if (!usernameInput || !usernameInput.value.trim()) {
            window.showToast?.('error', 'Please enter your username first.');
            return;
        }

        const fetchOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ username: usernameInput.value.trim() })
        };
        
        // 1. Get AssertionOptions from Server
        const response = await fetch('/api/webauthn/assertionOptions', fetchOptions);
        if (!response.ok) {
            window.showToast?.('error', 'This user does not have biometrics enabled.');
            return;
        }
        const assertionOptions = await response.json();

        // 2. Coerce challenge and credential ids
        assertionOptions.challenge = coerceToArrayBuffer(assertionOptions.challenge);
        if (assertionOptions.allowCredentials) {
            for (let cred of assertionOptions.allowCredentials) {
                cred.id = coerceToArrayBuffer(cred.id);
            }
        }

        // 3. Prompt OS Biometrics
        const credential = await navigator.credentials.get({
            publicKey: assertionOptions
        });

        // 4. Send assertion to server
        const makeAssertionResponse = {
            id: credential.id,
            rawId: coerceToBase64Url(credential.rawId),
            type: credential.type,
            clientExtensionResults: credential.getClientExtensionResults(),
            response: {
                authenticatorData: coerceToBase64Url(credential.response.authenticatorData),
                clientDataJSON: coerceToBase64Url(credential.response.clientDataJSON),
                signature: coerceToBase64Url(credential.response.signature),
                userHandle: credential.response.userHandle ? coerceToBase64Url(credential.response.userHandle) : null
            }
        };

        const assertRes = await fetch('/api/webauthn/makeAssertion', {
            ...fetchOptions,
            body: JSON.stringify(makeAssertionResponse)
        });

        if (assertRes.ok) {
            const data = await assertRes.json();
            if (data.token) {
                window.localStorage.setItem('token', data.token);
            }
            window.location.href = '/Dashboard';
        } else {
            const errData = await assertRes.json().catch(() => ({}));
            console.error('Assertion server response error:', errData);
            let errMsg = errData.message || 'Verification failed';
            if (errData.errors && Array.isArray(errData.errors)) {
                errMsg += '\nDetails: ' + errData.errors.join(', ');
            }
            window.showToast?.('error', 'Failed to authenticate with biometrics: ' + errMsg);
        }
    } catch (e) {
        console.error('WebAuthn Error:', e);
        window.showToast?.('error', 'Biometric login failed or was cancelled: ' + e.message);
    }
}
