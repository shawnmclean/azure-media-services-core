using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.MediaServices.Core.Assets
{
  public class Asset
  {
    public string Id { get; set; }
    public AssetState State { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public string Name { get; set; }
    public string Uri { get; set; }
  }
}
