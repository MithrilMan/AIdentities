using Microsoft.AspNetCore.Components;

namespace AIdentities.Shared.Features.CognitiveEngine.Components;

public partial class SkillGallery
{
   [Inject] public ISkillManager SkillManager { get; set; } = default!;

   [Parameter] public bool NeedToReload { get; set; }
   [Parameter] public EventCallback<bool> NeedToReloadChanged { get; set; }
   [Parameter] public bool CanBeSelected { get; set; }
   [Parameter] public ICollection<string> SelectedSkills { get; set; } = new HashSet<string>();
   [Parameter] public EventCallback<ICollection<string>> SelectedSkillsChanged { get; set; }

   protected override void OnInitialized()
   {
      base.OnInitialized();
      _state.Initialize(Filter);
   }

   protected override async Task OnInitializedAsync()
   {
      await FetchData().ConfigureAwait(false);
   }

   private async Task FetchData()
   {
      await _state.Skills.LoadItemsAsync(SkillManager.All()).ConfigureAwait(false);
      _state.AvailableTags = _state.Skills.UnfilteredItems.SelectMany(i => i.Tags).ToHashSet();
      await ApplyFilterAsync().ConfigureAwait(false);
      _state.SelectedSkills = SelectedSkills;
   }

   protected override async Task OnParametersSetAsync()
   {
      await base.OnParametersSetAsync().ConfigureAwait(false);
      if (NeedToReload)
      {
         await FetchData().ConfigureAwait(false);
         await NeedToReloadChanged.InvokeAsync(false).ConfigureAwait(false);
      }
   }

   public ValueTask<IEnumerable<ISkill>> Filter(IEnumerable<ISkill> unfilteredItems)
   {
      if (!string.IsNullOrWhiteSpace(_state.SearchText))
      {
         unfilteredItems = unfilteredItems
         .Where(c => c.Name?.Contains(_state.SearchText, StringComparison.OrdinalIgnoreCase) ?? false);

         var tags = _state.Tags.Select(t => t.ToString()).ToList();

         if (_state.Tags.Any())
         {
            unfilteredItems = unfilteredItems.Where(i => i.Tags.Any(x => tags.Contains(x)));
         }
      }

      unfilteredItems = unfilteredItems.OrderBy(c => c.Name, StringComparer.InvariantCultureIgnoreCase);

      return ValueTask.FromResult(unfilteredItems);
   }

   private async Task ApplyFilterAsync() => await _state.Skills.ApplyFilterAsync().ConfigureAwait(false);

   void ToggleTagSelection(string value)
   {
      if (_state.Tags.Contains(value))
      {
         ((HashSet<string>)_state.Tags).Remove(value);
      }
      else
      {
         ((HashSet<string>)_state.Tags).Add(value);
      }
   }

   void OnSelectedSkillChanged(ISkill skill, bool isChecked)
   {
      if (isChecked)
      {
         _state.SelectedSkills.Add(skill.Name);
      }
      else
      {
         _state.SelectedSkills.Remove(skill.Name);
      }

      SelectedSkillsChanged.InvokeAsync(_state.SelectedSkills);
   }
}
