using AIdentities.Shared.Features.CognitiveEngine.Skills;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;

namespace AIdentities.Chat.Components;

public partial class Settings : BasePluginSettingsTab<ChatSettings, Settings.State>
{
   [Inject] IEnumerable<IConversationalConnector> ConversationalConnectors { get; set; } = default!;
   [Inject] IEnumerable<ITextToSpeechConnector> TextToSpeechConnectors { get; set; } = default!;
   [Inject] ISkillManager SkillManager { get; set; } = default!;

   MudForm? _form;

   protected override void OnInitialized()
   {
      _validator = new Validator(ConversationalConnectors, TextToSpeechConnectors);
      _state.AvailableConnectors = ConversationalConnectors.ToDictionary(c => c.Name, c => c);
      _state.AvailableTTSConnectors = TextToSpeechConnectors.ToDictionary(c => c.Name, c => c);
      _state.AllKnownSkillNames = SkillManager.GetSkillDefinitions().Select(s => s.Name).ToList();

      base.OnInitialized();
   }

   protected override async ValueTask<bool> AreSettingsValid()
   {
      await _form!.Validate().ConfigureAwait(false);
      return _form.IsValid;
   }

   public override Task<ChatSettings> PerformSavingAsync()
   {
      return Task.FromResult(new ChatSettings()
      {
         DefaultConnector = _state.DefaultConnector,
         EnableSkills = _state.EnableSkills,
         EnabledSkills = _state.EnabledSkills.ToList(),
         EnableTextToSpeech = _state.EnableTextToSpeech,
         DefaultTextToSpeechConnector = _state.DefaultTextToSpeechConnector,
         TextToSpeechMode = _state.TextToSpeechMode
      });
   }

   public static bool IsConnectorDisabled(string? connectorName, IEnumerable<IConnector> connectors)
   {
      if (connectorName == null) return true;

      var connector = connectors.FirstOrDefault(x => x.Name == connectorName);
      return connector == null ? true : !connector.Enabled;
   }
}
