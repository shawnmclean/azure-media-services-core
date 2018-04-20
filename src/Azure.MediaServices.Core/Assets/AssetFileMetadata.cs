using System;
using System.Xml.Linq;
using Azure.MediaServices.Core.Xml;

namespace Azure.MediaServices.Core.Assets
{
  public class AssetFileMetadata
  {
    /// <summary>
    /// Gets the name of the media file.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Gets the size of the media file in bytes.
    /// </summary>
    public long Size { get; internal set; }

    /// <summary>
    /// Gets the play back duration of the media file.
    /// </summary>
    public TimeSpan Duration { get; internal set; }

    internal static AssetFileMetadata Load(XElement assetFileElement)
    {
      var assetFileMetadata = new AssetFileMetadata
      {
        Name = assetFileElement.GetAttributeOrDefault(AssetMetadataParser.NameAttributeName),
        Size = assetFileElement.GetAttributeAsLongOrDefault(AssetMetadataParser.SizeAttributeName),
        Duration = assetFileElement.GetAttributeAsTimeSpanOrDefault(AssetMetadataParser.DurationAttributeName)
      };

      return assetFileMetadata;
    }
  }
}