using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Azure.MediaServices.Core.AccessPolicies;
using Azure.MediaServices.Core.Assets;
using Azure.MediaServices.Core.EncodingReservedUnitTypes;
using Azure.MediaServices.Core.Jobs;
using Azure.MediaServices.Core.Locators;
using Azure.MediaServices.Core.MediaProcessors;
using Azure.MediaServices.Core.Models;
using Newtonsoft.Json;

namespace Azure.MediaServices.Core
{

  public class AzureMediaServiceClient : IAzureMediaServiceClient
  {
    private readonly string _restApiUrl;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerSettings _jsonSerializerSettings;
    private readonly AdalTokenClient _adalTokenClient;

    public AzureMediaServiceClient(string tenantId, string clientId, string clientSecret, string restApiUrl)
    {
      _restApiUrl = restApiUrl;
      _adalTokenClient = new AdalTokenClient(clientId, clientSecret, tenantId);
      _jsonSerializerSettings = new JsonSerializerSettings
      {
        ContractResolver = new PrivateSetterContractResolver()
      };

      if (_httpClient == null)
      {
        _httpClient = new HttpClient { BaseAddress = new Uri(restApiUrl) };
        _httpClient.DefaultRequestHeaders.Add("x-ms-version", "2.15");
        _httpClient.DefaultRequestHeaders.Add("DataServiceVersion", "3.0");
        _httpClient.DefaultRequestHeaders.Add("MaxDataServiceVersion", "3.0");
        _httpClient.DefaultRequestHeaders.Add("accept", "application/json;odata=verbose");
      }
    }

    public async Task<JobResponse> CreateJob(Job job)
    {
      var body = new
      {
        job.Name,
        InputMediaAssets = job.InputMediaAssets.Select(m => new
        {
          __metadata = new { uri = m.Metadata?.Uri ?? Path.Combine(_restApiUrl, $"Assets('{Uri.EscapeDataString(m.Id)}')") }
        }),
        Tasks = job.Tasks.Select(t => new
        {
          t.Configuration,
          t.MediaProcessorId,
          t.TaskBody
        })
      };
      var jobResponse = await Post<JobResponse>("Jobs", body, verboseOdata: true);
      return jobResponse;
    }

    public Task<List<JobResponse>> GetJobs(string filter)
    {
      return Get<JobResponse>($"Jobs?$filter={filter}");
    }
    public Task<JobResponse> GetJob(string id)
    {
      return GetOne<JobResponse>($"Jobs('{Uri.EscapeDataString(id)}')");
    }
    public Task DeleteJob(string id)
    {
      return Delete($"Jobs('{Uri.EscapeDataString(id)}')");
    }

    public Task<List<Asset>> GetJobOutputAsset(string id)
    {
      return Get<Asset>($"Jobs('{Uri.EscapeDataString(id)}')/OutputMediaAssets");
    }

    public Task<List<Asset>> GetJobInputAsset(string id)
    {
      return Get<Asset>($"Jobs('{Uri.EscapeDataString(id)}')/InputMediaAssets");
    }

    public Task<List<Asset>> GetAssets()
    {
      return Get<Asset>("Assets");
    }

    public Task<List<Locator>> GetLocators(string id)
    {
      return Get<Locator>($"Assets('{Uri.EscapeDataString(id)}')/Locators");
    }

    public Task DeleteLocator(string id)
    {
      return Delete($"Locators('{Uri.EscapeDataString(id)}')");
    }

    public Task<List<AssetFile>> GetAssetFiles(string id)
    {
      return Get<AssetFile>($"Assets('{Uri.EscapeDataString(id)}')/Files");
    }

    public Task<Asset> GetAsset(string id)
    {
      return GetOne<Asset>($"Assets('{Uri.EscapeDataString(id)}')");
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

    public Task DeleteAsset(string id)
    {

      return Delete($"Assets('{Uri.EscapeDataString(id)}')");
    }

    public Task CreateFileInfos(string id)
    {
      return Get($"CreateFileInfos?assetid='{Uri.EscapeDataString(id)}'");
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

    public Task DeleteAccessPolicy(string id)
    {
      return Delete($"AccessPolicies('{Uri.EscapeDataString(id)}')");
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

    public async Task<EncodingReservedUnitType> GetEncodingReservedUnit()
    {
      var result = await Get<EncodingReservedUnitType>("EncodingReservedUnitTypes");
      return result.FirstOrDefault();
    }

    public Task UpdateEncodingReservedUnits(string accountId, int reservedUnits)
    {
      var body = new
      {
        CurrentReservedUnits = reservedUnits
      };
      return Post<EncodingReservedUnitType>($"EncodingReservedUnitTypes(guid'{accountId}')", body, HttpMethod.Put);
    }

    public Task<List<MediaProcessor>> GetMediaProcessors()
    {
      return Get<MediaProcessor>("MediaProcessors");
    }

    internal async Task<List<TResponse>> Get<TResponse>(string path)
    {
      var message = new HttpRequestMessage(HttpMethod.Get, path);
      var response = await SendAsync(message);
      var stringContent = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"GET Failed: {stringContent}, HttpStatus: {response.StatusCode}");
      }
      var responseObject =
        JsonConvert.DeserializeObject<ODataResult<TResponse>>(stringContent, _jsonSerializerSettings);
      return responseObject.D.Results;
    }

    internal async Task<TResponse> GetOne<TResponse>(string path) where TResponse : class
    {
      var message = new HttpRequestMessage(HttpMethod.Get, path);
      var response = await SendAsync(message);
      var stringContent = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"GET Failed: {stringContent}, HttpStatus: {response.StatusCode}");
      }
      var responseObject =
        JsonConvert.DeserializeObject<ODataSingleResult<TResponse>>(stringContent, _jsonSerializerSettings);
      return responseObject.D;
    }

    internal async Task Get(string path)
    {
      var message = new HttpRequestMessage(HttpMethod.Get, path);
      var response = await SendAsync(message);
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"GET Failed: {path}, HttpStatus: {response.StatusCode}");
      }
    }

    internal async Task Delete(string path)
    {
      var message = new HttpRequestMessage(HttpMethod.Delete, path);
      var response = await SendAsync(message);
      var stringContent = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"{message.Method.Method} Failed: {stringContent}, HttpStatus: {response.StatusCode}");
      }
    }

    internal async Task<TResponse> Post<TResponse>(string path, object body, HttpMethod method = null, bool verboseOdata = false)
      where TResponse : class
    {
      var message = new HttpRequestMessage(HttpMethod.Post, Uri.EscapeDataString(path));
      if (method != null)
        message.Method = method;
      var bodyContent = JsonConvert.SerializeObject(body, _jsonSerializerSettings);
      message.Content = new StringContent(bodyContent, Encoding.UTF8, "application/json");
      if (verboseOdata)
        message.Content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("odata", "verbose"));
      var response = await SendAsync(message);
      var stringContent = await response.Content.ReadAsStringAsync();
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"{message.Method.Method} Failed: {stringContent}, HttpStatus: {response.StatusCode}");
      }
      if (response.StatusCode == HttpStatusCode.NoContent)
        return await Task.FromResult((TResponse)null);

      var responseObject = JsonConvert.DeserializeObject<ODataResponse<TResponse>>(stringContent, _jsonSerializerSettings);
      return responseObject.D;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
    {
      message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _adalTokenClient.GetTokenAsync().ConfigureAwait(false));
      return await _httpClient.SendAsync(message).ConfigureAwait(false);
    }
  }
}