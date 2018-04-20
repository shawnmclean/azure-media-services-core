using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Azure.MediaServices.Core
{
  public class AdalTokenClient
  {
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly AuthenticationContext _authenticationContext;

    public AdalTokenClient(string clientId, string clientSecret, string tenantId)
    {
      _clientId = clientId;
      _clientSecret = clientSecret;
      _authenticationContext = new AuthenticationContext($"https://login.microsoftonline.com/{tenantId}");
    }
    public async Task<string> GetTokenAsync()
    {
      AuthenticationResult result;
      try
      {
        result = await _authenticationContext.AcquireTokenSilentAsync(Constants.Resource, _clientId).ConfigureAwait(false);
        return result.AccessToken;
      }
      catch (AdalException e)
      {
        if (e.ErrorCode != AdalError.FailedToAcquireTokenSilently) throw;
        result = await _authenticationContext.AcquireTokenAsync(Constants.Resource, new ClientCredential(_clientId, _clientSecret)).ConfigureAwait(false);
        return result.AccessToken;
      }
    }
  }
}