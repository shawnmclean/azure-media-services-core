using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.MediaServices.Core.AccessPolicies
{
  public class AccessPolicy
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public double DurationInMinutes { get; set; }
  }
}
