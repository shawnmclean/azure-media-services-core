using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.MediaServices.Core.Locators
{
  public class Locator
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime ExpirationDateTime { get; set; }
    public int Type { get; set; }
    public string Path { get; set; }
    public string BaseUri { get; set; }
    public string AccessPolicyId { get; set; }
    public string AssetId { get; set; }

  }
}
