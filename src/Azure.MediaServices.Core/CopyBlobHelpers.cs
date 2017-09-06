using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Azure.MediaServices.Core
{
  /// <summary>
  /// Contains helper methods for copying blobs.
  /// </summary>
  public static class CopyBlobHelpers
  {
    /// <summary>
    /// Represents the maximum number of concurrent Copy Blob operations when copying content from a source container to a destination container.
    /// </summary>
    public const int MaxNumberOfConcurrentCopyFromBlobOperations = 750;

    /// <summary>
    /// Returns a <see cref="System.Threading.Tasks.Task"/> instance for the copy blobs operation from <paramref name="sourceContainer"/> to <paramref name="destinationContainer"/>.
    /// </summary>
    /// <param name="sourceContainer">The <see cref="Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer"/> instance that contains the blobs to be copied into <paramref name="destinationContainer"/>.</param>
    /// <param name="destinationContainer">The <see cref="Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer"/> instance where the blobs from <paramref name="sourceContainer"/> will be copied.</param>
    /// <param name="options">The <see cref="Microsoft.WindowsAzure.Storage.Blob.BlobRequestOptions"/>.</param>
    /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> instance for the copy blobs operation from <paramref name="sourceContainer"/> to <paramref name="destinationContainer"/>.</returns>
    public static async Task CopyBlobsAsync(CloudBlobContainer sourceContainer, CloudBlobContainer destinationContainer, BlobRequestOptions options, CancellationToken cancellationToken) {
      BlobContinuationToken continuationToken = null;

      do {
        BlobResultSegment resultSegment = await sourceContainer.ListBlobsSegmentedAsync(null, true, BlobListingDetails.None, MaxNumberOfConcurrentCopyFromBlobOperations, continuationToken, options, null, cancellationToken).ConfigureAwait(false);

        IEnumerable<Task> copyTasks = resultSegment
            .Results
            .Cast<CloudBlockBlob>()
            .Select(
                sourceBlob => {
                  CloudBlockBlob destinationBlob = destinationContainer.GetBlockBlobReference(sourceBlob.Name);

                  return CopyBlobAsync(sourceBlob, destinationBlob, options, cancellationToken);
                });

        await Task.WhenAll(copyTasks).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        continuationToken = resultSegment.ContinuationToken;
      }
      while (continuationToken != null);
    }

    /// <summary>
    /// Returns a <see cref="System.Threading.Tasks.Task"/> instance for the copy blob operation from <paramref name="sourceBlob"/> to <paramref name="destinationBlob"/>.
    /// </summary>
    /// <param name="sourceBlob">The <see cref="Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob"/> instance to be copied to <paramref name="destinationBlob"/>.</param>
    /// <param name="destinationBlob">The <see cref="Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob"/> instance where <paramref name="sourceBlob"/> will be copied.</param>
    /// <param name="options">The <see cref="Microsoft.WindowsAzure.Storage.Blob.BlobRequestOptions"/>.</param>
    /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken"/> instance used for cancellation.</param>
    /// <returns>A <see cref="System.Threading.Tasks.Task"/> instance for the copy blob operation from <paramref name="sourceBlob"/> to <paramref name="destinationBlob"/>.</returns>
    public static async Task CopyBlobAsync(CloudBlockBlob sourceBlob, CloudBlockBlob destinationBlob, BlobRequestOptions options, CancellationToken cancellationToken) {
      await destinationBlob.StartCopyAsync(sourceBlob, null, null, options, null, cancellationToken).ConfigureAwait(false);

      CopyState copyState = destinationBlob.CopyState;
      while (copyState == null || copyState.Status == CopyStatus.Pending) {
        cancellationToken.ThrowIfCancellationRequested();

        await destinationBlob.FetchAttributesAsync(null, options, null, cancellationToken).ConfigureAwait(false);

        copyState = destinationBlob.CopyState;
        if (copyState != null && copyState.Status != CopyStatus.Pending && copyState.Status != CopyStatus.Success) {
          throw new StorageException(copyState.StatusDescription);
        }
      }
    }
  }
}
