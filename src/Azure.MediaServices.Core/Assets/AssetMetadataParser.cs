using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Azure.MediaServices.Core.Assets
{
  public static class AssetMetadataParser
  {
    internal static readonly XName NameAttributeName = XName.Get("Name");

    internal static readonly XName SizeAttributeName = XName.Get("Size");

    internal static readonly XName DurationAttributeName = XName.Get("Duration");

    public static async Task<IEnumerable<AssetFileMetadata>> ParseAssetFileMetadataAsync(Uri assetFileMetadataUri)
    {
      IList<AssetFileMetadata> assetFileMetadataList = new List<AssetFileMetadata>();
      try
      {
        using (var assetFileMetadataStream = new MemoryStream())
        {
          var blob = new CloudBlockBlob(assetFileMetadataUri);
          await blob.DownloadToStreamAsync(assetFileMetadataStream, null, null, null).ConfigureAwait(false);

          assetFileMetadataStream.Seek(0, SeekOrigin.Begin);

          var root = XElement.Load(assetFileMetadataStream);
          foreach (var assetFileElement in root.Elements())
          {
            assetFileMetadataList.Add(AssetFileMetadata.Load(assetFileElement));
          }
        }
      }
      catch
      {
      }

      return assetFileMetadataList;
    }
  }
}