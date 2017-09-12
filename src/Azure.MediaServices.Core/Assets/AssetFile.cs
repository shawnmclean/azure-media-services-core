using System;
using System.Collections.Generic;
using System.Text;
using Azure.MediaServices.Core.Locators;

namespace Azure.MediaServices.Core.Assets
{
  public class AssetFile
  {
    public string Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public string Name { get; set; }
    public string ParentAssetId { get; set; }
    public long ContentFileSize { get; set; }
    public string MimeType { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsPrimary { get; set; }

    public Uri BlobUri(Locator locator)
    {
      var uriBuilder = new UriBuilder(locator.BaseUri);
      uriBuilder.Path += String.Concat("/", Name);
      return uriBuilder.Uri;
    }
  }
}
