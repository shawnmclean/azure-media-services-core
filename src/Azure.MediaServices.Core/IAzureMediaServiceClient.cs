using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.MediaServices.Core.AccessPolicies;
using Azure.MediaServices.Core.Assets;
using Azure.MediaServices.Core.Jobs;
using Azure.MediaServices.Core.Locators;
using Azure.MediaServices.Core.MediaProcessors;

namespace Azure.MediaServices.Core
{
  public interface IAzureMediaServiceClient
  {
    Task Initialization { get; }
    Task<AccessPolicy> CreateAccessPolicy(string name, int duration, int permissions);
    Task DeleteAsset(string id);
    Task<Asset> CreateAsset(string name, string storageAccountName);
    Task<AssetFile> CreateAssetFile(string name, string parentAssetId);
    Task<JobResponse> CreateJob(Job job);
    Task DeleteJob(string id);
    Task<Locator> CreateLocator(string accessPolicyId, string assetId, DateTime startTime, int type);
    Task<List<AssetFile>> GetAssetFiles(string id);
    Task<List<Asset>> GetAssets();
    Task<JobResponse> GetJob(string id);
    Task<List<Asset>> GetJobOutputAsset(string id);
    Task<List<MediaProcessor>> GetMediaProcessors();
    Task<AssetFile> UpdateAssetFile(AssetFile file);
  }
}