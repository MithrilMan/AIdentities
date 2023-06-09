﻿namespace AIdentities.Shared.Features.Core.Components;

public enum PluginResourceType
{
   /// <summary>
   /// CSS style generated by combining the blazor scoped CSS files.
   /// Use it to include styles defined within a component scoped css/scss file.
   /// </summary>
   EmbeddedStyle,

   /// <summary>
   /// CSS style defined in a specific file stored within the wwwroot plugin folder.
   /// RelativePath has to be specified.
   /// </summary>
   CustomStyle,
}
