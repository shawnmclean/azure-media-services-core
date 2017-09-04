using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.MediaServices.Core.MediaProcessors
{
  public class MediaProcessor
  {
    public string Id { get; private set; }
    public string Description { get; private set; }
    public string Name { get; private set; }
    public string Sku { get; private set; }
    public string Vendor { get; private set; }
    public string Version { get; private set; }
  }
}
