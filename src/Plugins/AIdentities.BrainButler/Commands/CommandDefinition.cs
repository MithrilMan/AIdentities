using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace AIdentities.BrainButler.Commands;

public abstract partial class CommandDefinition : IBrainButlerCommand
{
   public string Name { get; }
   public string ActivationContext { get; }
   public IEnumerable<CommandArgumentDefinition> Arguments { get; protected set; } = Enumerable.Empty<CommandArgumentDefinition>();
   public string ReturnDescription { get; }
   public string Examples { get; }

   public CommandDefinition(string name,
                            string activationContext,
                            string returnDescription,
                            string examples)
   {
      Name = name;
      ActivationContext = activationContext;
      ReturnDescription = returnDescription;
      Examples = examples;
   }

   public abstract IAsyncEnumerable<CommandExecutionStreamedFragment> ExecuteAsync(string userPrompt, string? inputPrompt, CancellationToken cancellationToken = default);

   /// <summary>
   /// Extracts the typeod arguments from the text.
   /// </summary>
   /// <typeparam name="TReturnValue">The type of the arguments to extract.</typeparam>
   /// <param name="text">The text to extract the arguments from.</param>
   /// <returns>The extracted arguments.</returns>
   public virtual bool TryExtractJson<TReturnValue>(string text, [MaybeNullWhen(false)] out TReturnValue args) where TReturnValue : class
   {
      var json = ExtractJson().Match(text).Value;
      try
      {
         args = JsonSerializer.Deserialize<TReturnValue>(json)!;
         return true;
      }
      catch (Exception)
      {
         args = default;
         return false;
      }
   }

   public virtual bool TryExtractJson(string text, [MaybeNullWhen(false)] out string json)
   {
      json = ExtractJson().Match(text).Value;
      return !string.IsNullOrWhiteSpace(json);
   }

   [GeneratedRegex("\\{(.|\\s)*\\}", RegexOptions.Multiline)]
   private static partial Regex ExtractJson();
}
