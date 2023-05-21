namespace AIdentities.Chat.Components;
public partial class Settings
{
   public class State : BaseState
   {
      public Dictionary<string, IConversationalConnector> AvailableConnectors { get; internal set; } = new();
      public List<string> AllKnownSkillNames { get; internal set; } = new();

      public string? DefaultConnector { get; set; } = default!;
      public ICollection<string> EnabledSkills { get; set; } = new HashSet<string>();
      public bool EnableSkills { get; set; }

      public override void SetFormFields(ChatSettings pluginSettings)
      {
         pluginSettings ??= new();
         DefaultConnector = pluginSettings.DefaultConnector;
         EnableSkills = pluginSettings.EnableSkills;
         EnabledSkills = pluginSettings.EnabledSkills.ToHashSet();
      }
   }
}
