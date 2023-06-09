﻿using AIdentities.Shared.Plugins.Connectors.Completion;

namespace AIdentities.Connector.TextGeneration;

public class PluginEntry : BasePluginEntry<PluginEntry>
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<IConversationalConnector, TextGenerationChatConnector>();
      services.AddScoped<ICompletionConnector, TextGenerationCompletionConnector>();

      RegisterPluginSettings<TextGenerationSettings, Settings>("Text-Generation");
   }
}
