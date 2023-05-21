using AIdentities.Shared.Features.CognitiveEngine.Skills;

namespace AIdentities.Chat.Components;

public partial class Settings : BasePluginSettingsTab<ChatSettings, Settings.State>
{
   [Inject] IEnumerable<IConversationalConnector> ConversationalConnectors { get; set; } = default!;
   [Inject] ISkillManager SkillActionsManager { get; set; } = default!;

   MudForm? _form;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _validator = new Validator(ConversationalConnectors);
      _state.AvailableConnectors = ConversationalConnectors.ToDictionary(c => c.Name, c => c);
      _state.AllKnownSkillNames = SkillActionsManager.All().Select(s => s.Name).ToList();
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
      });
   }

   public bool IsConnectorDisabled(string? connectorName)
   {
      if (connectorName == null) return true;

      var connector = ConversationalConnectors.FirstOrDefault(x => x.Name == connectorName);
      return connector == null ? true : !connector.Enabled;
   }
}
