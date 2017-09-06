using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.MediaServices.Core.Assets;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace Azure.MediaServices.Core
{
  public static class AzureMediaServiceClientExtensions
  {

    public static async Task<Asset> CreateFromBlobAsync(this AzureMediaServiceClient client, CloudBlockBlob sourceBlob, StorageCredentials storageCredentials, CancellationToken cancellationToken) {
      if (sourceBlob == null) {
        throw new ArgumentNullException("sourceBlob", "The source blob cannot be null.");
      }

      if (storageCredentials == null) {
        throw new ArgumentNullException("storageCredentials", "The destination storage credentials cannot be null.");
      }

      if (storageCredentials.IsAnonymous || storageCredentials.IsSAS) {
        throw new ArgumentException("The destination storage credentials must contain the account key credentials.", "destinationStorageCredentials");
      }

      var asset = await client.CreateAsync(sourceBlob.Name, storageCredentials.AccountName, cancellationToken).ConfigureAwait(false);
      cancellationToken.ThrowIfCancellationRequested();

      IRetryPolicy retryPolicy = context.MediaServicesClassFactory.GetBlobStorageClientRetryPolicy().AsAzureStorageClientRetryPolicy();
      BlobRequestOptions blobOptions = new BlobRequestOptions { RetryPolicy = retryPolicy };
      CloudBlobContainer container = new CloudBlobContainer(asset.Uri, storageCredentials);
      CloudBlockBlob blob = container.GetBlockBlobReference(sourceBlob.Name);

      await CopyBlobHelpers.CopyBlobAsync(sourceBlob, blob, blobOptions, cancellationToken).ConfigureAwait(false);
      cancellationToken.ThrowIfCancellationRequested();

      var assetFile = await client.CreateAssetFile(sourceBlob.Name, cancellationToken).ConfigureAwait(false);

      assetFile.IsPrimary = true;
      if (sourceBlob.Properties != null) {
        assetFile.ContentFileSize = sourceBlob.Properties.Length;
        assetFile.MimeType = sourceBlob.Properties.ContentType;
      }

      await assetFile.UpdateAsync().ConfigureAwait(false);

      return asset;
    }
  }
}
