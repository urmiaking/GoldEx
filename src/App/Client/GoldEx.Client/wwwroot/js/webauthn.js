window.webAuthn = {
    // Function to create a new passkey
    createCredential: async function (optionsJson) {
        try {
            const options = JSON.parse(optionsJson);
            // Ensure options are properly formatted for the browser API
            options.user.id = base64UrlDecode(options.user.id);
            options.challenge = base64UrlDecode(options.challenge);

            // decode excludeCredentials ids
            if (options.excludeCredentials) {
                for (let cred of options.excludeCredentials) {
                    cred.id = base64UrlDecode(cred.id);
                }
            }

            const credential = await navigator.credentials.create({
                publicKey: options
            });

            // Serialize the credential and encode relevant parts to Base64Url
            return this.serializeCredential(credential);

        } catch (error) {
            console.error("Error during passkey creation:", error);
            // Rethrow or return a specific error structure if needed by C#
            // For Blazor, throwing JSException is often handled well.
            throw new Error(`Passkey creation failed: ${error.message}`);
        }
    },

    // Function to sign in with an existing passkey
    requestCredential: async function (optionsJson) {
        console.log("Attempting to sign in with passkey using options:", optionsJson);
        try {
            const options = JSON.parse(optionsJson);
            // The challenge in request options is often sent as a string, might need encoding
            options.challenge = base64UrlDecode(options.challenge); // Ensure challenge is decoded if needed

            // Ensure allowCredentials are properly formatted if provided
            if (options.allowCredentials && options.allowCredentials.length > 0) {
                options.allowCredentials = options.allowCredentials.map(cred => ({
                    ...cred,
                    id: base64UrlDecode(cred.id) // Decode credential IDs
                }));
            }

            const credential = await navigator.credentials.get({
                publicKey: options
            });

            // Serialize the credential and encode relevant parts to Base64Url
            return this.serializeCredential(credential, true); // true indicates assertion

        } catch (error) {
            console.error("Error during passkey sign-in:", error);
            throw new Error(`Passkey sign-in failed: ${error.message}`);
        }
    },

    // Helper to serialize credential and encode relevant fields to Base64Url
    serializeCredential: function (credential, isAssertion = false) {
        const response = credential.response;
        const serializedResponse = {
            clientDataJSON: response.clientDataJSON ? bufferToBase64Url(response.clientDataJSON) : null,
            // For registration:
            attestationObject: !isAssertion && response.attestationObject ? bufferToBase64Url(response.attestationObject) : null,
            // For assertion:
            authenticatorData: isAssertion && response.authenticatorData ? bufferToBase64Url(response.authenticatorData) : null,
            signature: isAssertion && response.signature ? bufferToBase64Url(response.signature) : null,
            userHandle: isAssertion && response.userHandle ? bufferToBase64Url(response.userHandle) : null,
        };

        const serializedCredential = {
            id: credential.id,
            rawId: bufferToBase64Url(credential.rawId),
            response: serializedResponse,
            type: credential.type,
            clientExtensionResults: credential.getClientExtensionResults?.() || {}
        };

        return JSON.stringify(serializedCredential);
    },

    // Simple check if WebAuthn API is available
    isSupported: function() {
        return !!(window.PublicKeyCredential);
    }
};

// --- Utility Functions for Base64Url Encoding/Decoding ---

// Converts ArrayBuffer to Base64Url string
function bufferToBase64Url(buffer) {
    if (buffer instanceof ArrayBuffer) {
        return base64Encode(buffer);
    } else if (typeof buffer === 'string') {
        // If it's already a string (e.g., clientDataJSON might be), it might need encoding
        // This part might need adjustment based on how data is passed. Let's assume it's ArrayBuffer.
         console.warn("Received string instead of ArrayBuffer for base64Url encoding, attempting to encode as UTF-8 string.");
         return base64Encode(stringToArrayBuffer(buffer));
    } else if (buffer instanceof Uint8Array) {
        return base64Encode(buffer.buffer);
    }
    return buffer; // Fallback if not an ArrayBuffer or Uint8Array
}

function base64Encode(arrayBuffer) {
    const bytes = new Uint8Array(arrayBuffer);
    let binary = '';
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    // Use btoa for standard base64, then replace characters for base64url
    return window.btoa(binary)
        .replace(/\+/g, '-')
        .replace(/\//g, '_')
        .replace(/=/g, '');
}

// Converts Base64Url string to ArrayBuffer
function base64UrlDecode(str) {
    str = str.replace(/-/g, '+').replace(/_/g, '/');
    // Add padding if necessary
    const padding = '='.repeat((4 - (str.length % 4)) % 4);
    str += padding;
    const base64 = window.atob(str);
    const bytes = new Uint8Array(base64.length);
    for (let i = 0; i < base64.length; i++) {
        bytes[i] = base64.charCodeAt(i);
    }
    return bytes.buffer; // Return as ArrayBuffer
}

// Helper to convert string to ArrayBuffer (for potential string inputs)
function stringToArrayBuffer(str) {
    const buf = new ArrayBuffer(str.length);
    const bufView = new Uint8Array(buf);
    for (let i = 0, strLen = str.length; i < strLen; i++) {
        bufView[i] = str.charCodeAt(i);
    }
    return buf;
}


// Add a check for the WebAuthn API availability
window.webAuthn.isSupported = window.webAuthn.isSupported;
