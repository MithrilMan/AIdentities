namespace AIdentities.Shared.Features.CognitiveEngine.Components;

public partial class TabAIdentityFeatureSkills
{
   class State
   {
      public bool AreSkillsEnabled { get; set; } = false;
      public ICollection<string> EnabledSkills { get; set; } = new HashSet<string>();

      internal void SetFormFields(AIdentityFeatureSkills? skillsFeature)
      {
         AreSkillsEnabled = skillsFeature?.AreSkillsEnabled ?? false;
         EnabledSkills = skillsFeature?.EnabledSkills.ToHashSet() ?? new HashSet<string>();
      }
   }

   private readonly State _state = new State();
}
