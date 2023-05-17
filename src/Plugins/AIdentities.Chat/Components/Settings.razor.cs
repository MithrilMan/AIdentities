namespace AIdentities.Chat.Components;

public partial class Settings : BasePluginSettingsTab<ChatSettings, Settings.State>
{
   [Inject] IEnumerable<IConversationalConnector> ConversationalConnectors { get; set; } = default!;

   MudForm? _form;

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _validator = new Validator(ConversationalConnectors);
      _state.AvailableConnectors = ConversationalConnectors.ToDictionary(c => c.Name, c => c);
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
      });
   }

   public bool IsConnectorDisabled(string? connectorName)
   {
      if (connectorName == null) return true;

      var connector = ConversationalConnectors.FirstOrDefault(x => x.Name == connectorName);
      return connector == null ? true : !connector.Enabled;
   }
}
