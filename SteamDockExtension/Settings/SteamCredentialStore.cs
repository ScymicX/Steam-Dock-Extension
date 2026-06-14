using Windows.Security.Credentials;

namespace SteamDockExtension.Settings;

internal sealed class SteamCredentialStore
{
    private const string Resource = "DennieZorg.SteamDockExtension";
    private const string UserName = "SteamWebApiKey";
    private readonly PasswordVault _vault = new();

    public string? ReadApiKey()
    {
        try
        {
            var credential = _vault.Retrieve(Resource, UserName);
            credential.RetrievePassword();
            return string.IsNullOrWhiteSpace(credential.Password) ? null : credential.Password.Trim();
        }
        catch
        {
            return null;
        }
    }

    public bool TryWriteApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return false;
        }

        try
        {
            TryRemove();
            _vault.Add(new PasswordCredential(Resource, UserName, apiKey.Trim()));
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void DeleteApiKey()
    {
        try
        {
            TryRemove();
        }
        catch
        {
        }
    }

    private void TryRemove()
    {
        try
        {
            _vault.Remove(_vault.Retrieve(Resource, UserName));
        }
        catch
        {
            // The credential does not exist.
        }
    }
}
