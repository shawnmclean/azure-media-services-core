using System;
using System.Collections.Generic;
using System.Text;
using Azure.MediaServices.Core.Assets;

namespace Azure.MediaServices.Core.Jobs
{
  public class Job
  {
    public Job(string name, Asset[] inputMediaAssets, JobTask[] tasks)
    {
      Name = name;
      InputMediaAssets = inputMediaAssets;
      Tasks = tasks;
    }

    public string Name { get; }
    public Asset[] InputMediaAssets { get; }
    public JobTask[] Tasks { get; }
  }

  public class JobTask
  {
    public string Name { get; set; }
    public string Configuration { get; set; }
    public string MediaProcessorId { get; set; }
    public string TaskBody { get; set; }
  }
  
}
