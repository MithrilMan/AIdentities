using AIdentities.Connector.TTS.ElevenLabs.Components;
using AIdentities.Connector.TTS.ElevenLabs.Models;
using AIdentities.Connector.TTS.ElevenLabs.Services;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;

namespace AIdentities.Connector.TTS.ElevenLabs;

public class PluginEntry : BasePluginEntry<PluginEntry>
{
   public override void RegisterServices(IServiceCollection services)
   {
      services.AddScoped<ITextToSpeechConnector, ElevenLabsTTSConnector>();

      RegisterPluginSettings<ElevenLabsSettings, Settings>("ElevenLabs TTS");
   }
}
