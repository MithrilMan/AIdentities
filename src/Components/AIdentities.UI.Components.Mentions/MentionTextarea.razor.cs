﻿using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace AIdentities.UI.Components.Mentions;

public partial class MentionTextarea : ComponentBase, IAsyncDisposable
{
   [Inject] IJSRuntime JS { get; set; } = null!;

   [Parameter] public string Class { get; set; } = default!;
   [Parameter] public EventCallback<string> TextChanged { get; set; }

   [Parameter] public string? Placeholder { get; set; } = "Type some text";

   [Parameter] public string Delimiters { get; set; } = @"([.,;\s])";
   [Parameter] public string Markers { get; set; } = "@#";

   [Parameter] public int DebounceTimer { get; set; } = 500;
   [Parameter] public int MaxSuggestions { get; set; } = 5;

   [Parameter] public Func<char, string, Task<IEnumerable<IMention>>>? SearchFunc { get; set; }
   [Parameter] public RenderFragment<IMention>? SuggestionContentItem { get; set; }
   [Parameter] public RenderFragment<IMention>? TooltipContent { get; set; }

   private readonly string _mentionContainerId = $"mention-{Guid.NewGuid()}";

   private bool _showMentionPopover = false;
   private bool _showMentionTooltip = false;

   private string? CurrentWord { get; set; }

   private IEnumerable<IMention> _suggestions = Enumerable.Empty<IMention>();
   private object SelectedSuggestionIndex { get; set; } = 0;

   private IJSObjectReference? _jsEditor;
   private IJSObjectReference? _jsPopoverPlacer;

   protected override async Task OnAfterRenderAsync(bool firstRender)
   {
      if (firstRender)
      {
         _jsEditor = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/AIdentities.UI.Components.Mentions/editor.js").ConfigureAwait(false);
         _jsPopoverPlacer = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/AIdentities.UI.Components.Mentions/PopoverPlacer.js").ConfigureAwait(false);

         var reference = DotNetObjectReference.Create(this);

         await _jsEditor.InvokeVoidAsync("editor.initialize", reference).ConfigureAwait(false);
         await _jsPopoverPlacer.InvokeVoidAsync("popover.initialize", _mentionContainerId, 0).ConfigureAwait(false);
         await _jsPopoverPlacer.InvokeVoidAsync("tooltip.initialize", _mentionContainerId, 1).ConfigureAwait(false);
      }
      await base.OnAfterRenderAsync(firstRender).ConfigureAwait(false);
   }

   public async Task<string> GetContent()
   {
      var content = await _jsEditor!.InvokeAsync<string>("editor.getContent").ConfigureAwait(false);
      return content;
   }

   private System.Timers.Timer? _timer;

   private void StartTimer()
   {
      _timer = new System.Timers.Timer(DebounceTimer);
      _timer.Elapsed += OnSearchAsync;
      _timer.Enabled = true;
      _timer.Start();
   }

   private void DisposeTimer()
   {
      if (_timer is not null)
      {
         _timer.Dispose();
         _timer = null;
         _suggestions = Enumerable.Empty<IMention>();
      }
   }

   public async void OnSearchAsync(object? sender, EventArgs e)
   {
      DisposeTimer();
      if (!string.IsNullOrEmpty(CurrentWord) && SearchFunc is not null)
      {
         var marker = CurrentWord[0];
         var query = CurrentWord[1..];

         _suggestions = await SearchFunc(marker, query).ConfigureAwait(false);
         await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      }
   }

   public async Task OnItemSelected(IMention item)
   {
      await _jsEditor!.InvokeVoidAsync("editor.insertMentionAtHighlighted", item.Value).ConfigureAwait(false);
      await InvokeAsync(ResetMentions).ConfigureAwait(false);
   }

   private async Task ResetMentions()
   {
      if (_showMentionPopover)
      {
         DisposeTimer();
         _showMentionPopover = false;
         _suggestions = Enumerable.Empty<IMention>();
         SelectedSuggestionIndex = 0;
         CurrentWord = null;
         await _jsEditor!.InvokeVoidAsync("editor.isPopoverVisible", false).ConfigureAwait(false);
         await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      }
   }

   private async Task OpenMentionBox()
   {
      if (!_showMentionPopover)
      {
         _showMentionPopover = true;
         _suggestions = Enumerable.Empty<IMention>();
         await _jsEditor!.InvokeVoidAsync("editor.isPopoverVisible", true).ConfigureAwait(false);
         await InvokeAsync(StateHasChanged).ConfigureAwait(false);
      }
   }

   private async Task CheckKey(KeyboardEventArgs ev)
   {
      if (_showMentionPopover && _suggestions.Any())
      {
         // handle keys if mention box is opened
         switch (ev.Key)
         {
            case "ArrowUp":
               // handle next suggestion
               SelectedSuggestionIndex = (int)SelectedSuggestionIndex - 1;
               if ((int)SelectedSuggestionIndex < 0)
                  SelectedSuggestionIndex = _suggestions.Count() - 1;
               return;

            case "ArrowDown":
               // handle next suggestion
               SelectedSuggestionIndex = (int)SelectedSuggestionIndex + 1;
               if ((int)SelectedSuggestionIndex >= _suggestions.Count())
                  SelectedSuggestionIndex = 0;
               return;

            case "Enter":
               if ((int)SelectedSuggestionIndex < _suggestions!.Count())
                  await OnItemSelected(_suggestions.ElementAt((int)SelectedSuggestionIndex)).ConfigureAwait(false);
               return;

            case "Escape":
            case " ":
               await InvokeAsync(ResetMentions).ConfigureAwait(false);
               return;
         }
         if (ev.Key.Length > 1)
         {
            await InvokeAsync(ResetMentions).ConfigureAwait(false);
            return;
         }
      }
   }

   [JSInvokable]
   public Task<List<Token>> Tokenizer(string? text)
   {
      List<Token> tokens = new();
      if (text is not null)
      {
         // FIXME: for some reason if I have an empty space followed by a new line
         // I'll have an additional string with length 0. It should not happen, I think
         // the problem is with the regex I'm using to split the text.
         IEnumerable<string> parts = Regex.Split(text, Delimiters).Where(x => x.Length > 0);

         int offset = 0;
         foreach (var (part, index) in parts.Select((v, i) => (v, i)))
         {
            var end = part.Length;
            Token token = new()
            {
               Value = part,
               Attributes = new()
                    {
                        { "data-word", "" },
                        { "data-wordindex", index.ToString() },
                        { "data-wordstart", offset.ToString() },
                        { "data-wordend", (offset + end).ToString() },
                    }
            };
            if (Markers.Contains(token.Value[0]))
               token.Attributes.Add("data-mention", $"{token.Value[0]}");

            tokens.Add(token);
            offset += end;
         }
      }
      return Task.FromResult(tokens);
   }

   private IMention? _currentTooltipMention;

   [JSInvokable]
   public async Task MentionTooltipOpen(MentionPopover p)
   {
      if (!_showMentionPopover && SearchFunc is not null)
      {
         var mentions = await SearchFunc(p.Marker, p.Query!).ConfigureAwait(false);
         if (mentions.Any())
         {
            _currentTooltipMention = mentions.First();
            _showMentionTooltip = true;
            await _jsPopoverPlacer!.InvokeVoidAsync("tooltip.updateOffsets", p.Top, p.Left).ConfigureAwait(false);
            await InvokeAsync(StateHasChanged).ConfigureAwait(false);
         }
      }
   }

   [JSInvokable]
   public async Task MentionTooltipClose()
   {
      _showMentionTooltip = false;
      await InvokeAsync(StateHasChanged).ConfigureAwait(false);
   }

   [JSInvokable]
   public async Task OnMention(string word, double top, double left)
   {
      CurrentWord = word;
      var isMentionBoxOpened = _showMentionPopover;
      StartTimer();
      await _jsPopoverPlacer!.InvokeVoidAsync("popover.updateOffsets", top, left).ConfigureAwait(false);
      await InvokeAsync(OpenMentionBox).ConfigureAwait(false);
   }

   [JSInvokable]
   public async Task OnCloseMentionPopover()
   {
      await InvokeAsync(ResetMentions).ConfigureAwait(false);
   }

   public async ValueTask DisposeAsync()
   {
      try
      {
         DisposeTimer();
         if (_jsEditor is not null)
            await _jsEditor.DisposeAsync().ConfigureAwait(false);
         GC.SuppressFinalize(this);
      }
      catch (JSDisconnectedException) { }
      catch (TaskCanceledException) { }
   }
}
