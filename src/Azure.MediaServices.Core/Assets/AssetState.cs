using System;
using System.Collections.Generic;
using System.Text;

namespace Azure.MediaServices.Core.Assets
{
  /// <summary>
  /// Specifies the allowed states of an asset.
  /// </summary>
  public enum AssetState
  {
    /// <summary>
    /// Specifies that the asset is in initialized state.
    /// </summary>
    /// <remarks>This is the default. Assets in this state may not be used for Jobs or Tasks. Assets are allowed to have locators with full control while in this state.</remarks>
    Initialized,

    /// <summary>
    /// Specifies that the asset is published.
    /// </summary>
    /// <remarks>Published Assets can be used in Job and Tasks, but are immutable. Assets are only allowed to have read and list locators in this state.</remarks>
    Published,

    /// <summary>
    /// Specifies that the asset has been deleted.
    /// </summary>
    /// <remarks>Deleted Assets cannot be used in Job or Tasks, and do not actually exist and are only exposed for tracking purposes.</remarks>
    Deleted,
  }
}
