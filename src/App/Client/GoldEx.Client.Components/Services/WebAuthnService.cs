using Microsoft.JSInterop;

namespace GoldEx.Client.Components.Services;

public class WebAuthnService(IJSRuntime jsRuntime)
{
    public async Task<string> CreatePasskeyAsync(string optionsJson)
    {
        try
        {
            var credentialJson = await jsRuntime.InvokeAsync<string>(
                "webAuthn.createCredential",
                optionsJson);

            return credentialJson;
        }
        catch (JSException ex)
        {
            Console.WriteLine(@$"JavaScript Exception during createPasskeyAsync: {ex.Message}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine(@$"General Exception during createPasskeyAsync: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<string> SignInWithPasskeyAsync(string optionsJson)
    {
        try
        {
            var credentialJson = await jsRuntime.InvokeAsync<string>(
                "webAuthn.requestCredential",
                optionsJson);

            return credentialJson;
        }
        catch (JSException ex)
        {
            Console.WriteLine(@$"JavaScript Exception during signInWithPasskeyAsync: {ex.Message}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine(@$"General Exception during signInWithPasskeyAsync: {ex.Message}");
            return string.Empty;
        }
    }
}