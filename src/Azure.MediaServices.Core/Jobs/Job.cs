using System;
using System.Collections.Generic;
using System.Text;
using Azure.MediaServices.Core.Assets;
using Newtonsoft.Json;

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

  public class JobResponse
  {
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime Created { get; set; }
    public DateTime LastModified { get; set; }
    public int RunningDuration { get; set; }
    public JobState State { get; set; }
    public DeferredAsset InputMediaAssets { get; set; }
    public DeferredAsset OutputMediaAssets { get; set; }
  }

  public class DeferredAsset
  {
    [JsonProperty("__deferred")]
    public Deferred Deferred { get; set; }
  }

  public class Deferred
  {
    [JsonProperty("uri")]
    public string Uri { get; set; }
  }

  public enum JobState
  {
    Queued = 0,
    Scheduled = 1,
    Processing = 2,
    Finished = 3,
    Error = 4,
    Canceled = 5,
    Canceling = 6
  }
}
