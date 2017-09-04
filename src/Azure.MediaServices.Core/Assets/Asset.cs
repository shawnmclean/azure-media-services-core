using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.MediaServices.Core.Assets
{
  public class Asset
  {
    public string Id { get; }
    public AssetState State { get; }
    public DateTime Created { get; }
    public DateTime LastModified { get; }
    public string Name { get; set; }
  }
}
