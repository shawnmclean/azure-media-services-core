using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Azure.MediaServices.Core.Models
{
  internal class ODataResult<T>
  {
    public D<T> D { get; set; }
  }

  internal class D<T>
  {
    public List<T> Results { get; set; }
  }
  
}
