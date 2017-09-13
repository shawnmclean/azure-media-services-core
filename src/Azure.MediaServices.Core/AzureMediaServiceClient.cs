using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Azure.MediaServices.Core.AccessPolicies;
using Azure.MediaServices.Core.Assets;
using Azure.MediaServices.Core.Jobs;
using Azure.MediaServices.Core.Locators;
using Azure.MediaServices.Core.MediaProcessors;
using Azure.MediaServices.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Azure.MediaServices.Core
{
  public class AzureMediaServiceClient : IAzureMediaServiceClient
  {
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly string _tenantDomain;

    private AzureMediaServiceClient(HttpClient httpClient, string clientId, string clientSecret, string restApiUrl,
      string tenantDomain) : this(clientId, clientSecret, restApiUrl, tenantDomain)
    {
      _httpClient = httpClient;
    }

    private AzureMediaServiceClient(string clientId, string clientSecret, string restApiUrl, string tenantDomain)
    {
      _clientId = clientId;
      _clientSecret = clientSecret;
      _tenantDomain = tenantDomain;

      _jsonSerializerSettings = new JsonSerializerSettings
      {
        ContractResolver = new PrivateSetterContractResolver()
      };

      if (_httpClient == null)
      {
        _httpClient = new HttpClient {BaseAddress = new Uri(restApiUrl)};
        _httpClient.DefaultRequestHeaders.Add("x-ms-version", "2.7");
        _httpClient.DefaultRequestHeaders.Add("DataServiceVersion", "3.0");
        _httpClient.DefaultRequestHeaders.Add("MaxDataServiceVersion", "3.0");
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json;odata=verbose");
      }
    }

    public static async Task<AzureMediaServiceClient> CreateAsync(HttpClient httpClient, string clientId,
      string clientSecret, string restApiUrl, string tenantDomain)
    {
      var client = new AzureMediaServiceClient(httpClient, clientId, clientSecret, restApiUrl, tenantDomain);
      await client.Initialize();
      return client;
    }

    public static async Task<AzureMediaServiceClient> CreateAsync(string clientId, string clientSecret,
      string restApiUrl, string tenantDomain)
    {
      var client = new AzureMediaServiceClient(clientId, clientSecret, restApiUrl, tenantDomain);
      await client.Initialize();
      return client;
    }

    private async Task Initialize()
    {
      await SetupJWT();
    }

    public async Task<JobResponse> CreateJob(Job job)
    {
      var body = new
      {
        job.Name,
        InputMediaAssets = job.InputMediaAssets.Select(m => new
        {
          __metadata = new {uri = m.Metadata.Uri}
        }),
        Tasks = job.Tasks.Select(t => new
        {
          t.Configuration,
          t.MediaProcessorId,
          t.TaskBody
        })
      };
      _httpClient.DefaultRequestHeaders.Remove("DataServiceVersion");
      _httpClient.DefaultRequestHeaders.Add("DataServiceVersion", "2.0");
      var jobResponse = await Post<JobResponse>("Jobs", body);
      _httpClient.DefaultRequestHeaders.Remove("DataServiceVersion");
      _httpClient.DefaultRequestHeaders.Add("DataServiceVersion", "3.0");

      return jobResponse;
    }

    public Task<JobResponse> GetJob(string id)
    {
      return GetOne<JobResponse>($"Jobs('{Uri.EscapeDataString(id)}')");
    }

    public Task<List<Asset>> GetJobOutputAsset(string id)
    {
      return Get<Asset>($"Jobs('{Uri.EscapeDataString(id)}')/OutputMediaAssets");
    }

    public Task<List<Asset>> GetAssets()
    {
      return Get<Asset>("Assets");
    }

    public Task<List<AssetFile>> GetAssetFiles(string id)
    {
      return Get<AssetFile>($"Assets('{Uri.EscapeDataString(id)}')/Files");
    }

    public Task<Asset> CreateAsset(string name, string storageAccountName)
    {
      var body = new
      {
        Name = name,
        Options = 0,
        StorageAccountName = storageAccountName
      };

      return Post<Asset>("Assets", body);
    }

    public Task<AccessPolicy> CreateAccessPolicy(string name, int duration, int permissions)
    {
      var body = new
      {
        Name = name,
        DurationInMinutes = duration,
        Permissions = permissions
      };
      return Post<AccessPolicy>("AccessPolicies", body);
    }

    public Task<Locator> CreateLocator(string accessPolicyId, string assetId, DateTime startTime, int type)
    {
      var body = new
      {
        AccessPolicyId = accessPolicyId,
        AssetId = assetId,
        StartTime = startTime,
        Type = type
      };
      return Post<Locator>("Locators", body);
    }

    public Task<AssetFile> CreateAssetFile(string name, string parentAssetId)
    {
      var body = new
      {
        Name = name,
        ParentAssetId = parentAssetId
      };
      return Post<AssetFile>("Files", body);
    }

    public async Task<AssetFile> UpdateAssetFile(AssetFile file)
    {
      var body = new
      {
        file.Id,
        file.MimeType,
        file.Name,
        file.ParentAssetId,
        ContentFileSize = file.ContentFileSize.ToString()
      };
      await Post<AssetFile>($"Files('{file.Id}')", body, new HttpMethod("MERGE"));
      return file;
    }

    public Task<List<MediaProcessor>> GetMediaProcessors()
    {
      return Get<MediaProcessor>("MediaProcessors");
    }

    internal async Task<List<TResponse>> Get<TResponse>(string path)
    {
      var response = await _httpClient.GetAsync(path);
      var stringContent = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode) throw new WebException("Failed");
      var responseObject =
        JsonConvert.DeserializeObject<ODataResult<TResponse>>(stringContent, _jsonSerializerSettings);
      return responseObject.D.Results;
    }

    internal async Task<TResponse> GetOne<TResponse>(string path)
    {
      var response = await _httpClient.GetAsync(path);
      var stringContent = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode) throw new WebException("Failed");
      var responseObject =
        JsonConvert.DeserializeObject<ODataSingleResult<TResponse>>(stringContent, _jsonSerializerSettings);
      return responseObject.D;
    }

    internal async Task<TResponse> Post<TResponse>(string path, object body, HttpMethod method = null)
      where TResponse : class
    {
      var message = new HttpRequestMessage(HttpMethod.Post, Uri.EscapeDataString(path));
      if (method != null)
        message.Method = method;
      var bodyContent = JsonConvert.SerializeObject(body, _jsonSerializerSettings);
      message.Content = new StringContent(bodyContent, Encoding.UTF8, "application/json");
      var response = await _httpClient.SendAsync(message);
      var stringContent = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode) throw new WebException("Failed");
      if (response.StatusCode == HttpStatusCode.NoContent)
        return await Task.FromResult((TResponse) null);

      var responseObject =
        JsonConvert.DeserializeObject<ODataResponse<TResponse>>(stringContent, _jsonSerializerSettings);
      return responseObject.D;
    }

    internal async Task<List<TResponse>> Send<TResponse>(string path, HttpMethod method, object body)
    {
      var request = new HttpRequestMessage(method, Uri.EscapeDataString(path));
      var bodyContent = JsonConvert.SerializeObject(body, _jsonSerializerSettings);
      request.Content = new StringContent(bodyContent);

      var response = await _httpClient.SendAsync(request);
      var stringContent = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
        throw new WebException("Failed");
      var responseObject =
        JsonConvert.DeserializeObject<ODataResult<TResponse>>(stringContent, _jsonSerializerSettings);
      return responseObject.D.Results;
    }

    private async Task SetupJWT()
    {
      var body =
        $"resource={HttpUtility.UrlEncode(Constants.Resource)}&client_id={_clientId}&client_secret={HttpUtility.UrlEncode(_clientSecret)}&grant_type=client_credentials";

      var httpContent = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
      var response = await _httpClient.PostAsync($"https://login.microsoftonline.com/{_tenantDomain}/oauth2/token",
        httpContent);

      if (!response.IsSuccessStatusCode) throw new Exception();

      var resultBody = await response.Content.ReadAsStringAsync();

      var obj = JObject.Parse(resultBody);

      if (obj["access_token"] != null)
        _httpClient.DefaultRequestHeaders.Authorization =
          new AuthenticationHeaderValue("Bearer", obj["access_token"].ToString());
    }
  }
}