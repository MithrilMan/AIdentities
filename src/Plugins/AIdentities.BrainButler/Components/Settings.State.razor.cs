namespace AIdentities.BrainButler.Components;
public partial class Settings
{
   public class State : BaseState
   {
      private IConnectorsManager _connectorsManager = default!;

      public List<ICompletionConnector> AvailableCompletionConnectors { get; internal set; } = default!;
      public List<IConversationalConnector> AvailableConversationalConnectors { get; internal set; } = default!;

      public ICompletionConnector? DefaultCompletionConnector { get; set; }
      public IConversationalConnector? DefaultConversationalConnector { get; set; }

      internal void Initialize(IConnectorsManager connectorsManager)
      {
         _connectorsManager = connectorsManager;
         AvailableCompletionConnectors = connectorsManager.GetAllCompletionConnectors().ToList();
         AvailableConversationalConnectors = connectorsManager.GetAllConversationalConnectors().ToList();
      }

      public override void SetFormFields(BrainButlerSettings pluginSettings)
      {
         pluginSettings ??= new();
         DefaultCompletionConnector = _connectorsManager.GetCompletionConnector();
         DefaultConversationalConnector = _connectorsManager.GetConversationalConnector();
      }
   }
}
