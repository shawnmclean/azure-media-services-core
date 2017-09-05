using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.MediaServices.Core.Models
{
  internal class ODataResponse<T>
  {
    public T D { get; set; }
  }
}
