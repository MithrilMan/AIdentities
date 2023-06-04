using AIdentities.Shared.Collections;

namespace AIdentities.Shared.Features.CognitiveEngine.Components;

public partial class SkillGallery
{
   class State
   {
      public string? SearchText { get; set; }
      public FilteredObservableCollection<SkillDefinition> Skills { get; private set; } = default!;
      public HashSet<string> AvailableTags { get; set; } = new HashSet<string>();
      public IEnumerable<string> Tags { get; set; } = new HashSet<string>();

      public ICollection<string> SelectedSkills { get; set; } = new HashSet<string>();

      public void Initialize(Func<IEnumerable<SkillDefinition>, ValueTask<IEnumerable<SkillDefinition>>> filter)
      {
         SearchText = null;
         Skills = new(filter);
      }
   }

   private readonly State _state = new State();
}
