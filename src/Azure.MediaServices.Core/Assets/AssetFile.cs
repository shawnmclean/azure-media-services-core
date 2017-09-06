using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.MediaServices.Core.Assets
{
  public class AssetFile
  {
    public string Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public string Name { get; set; }
    public string ParentAssetId { get; set; }
    public int ContentFileSize { get; set; }
    public string MimeType { get; set; }
    public bool IsEncrypted { get; set; }
    public bool IsPrimary { get; set; }
  }
}
