using System.Text;

namespace AIdentities.Chat.Skills;
public static class PromptUtils
{
   public static string BuildParticipants(AIdentity aIdentity, IEnumerable<string> participants)
   {
      participants = participants.Where(p => p != aIdentity.Name);

      if (!participants.Any()) return "other characters";

      var participantCount = participants.Count();
      if (participantCount == 1) return $"{participants.First()}";

      var sb = new StringBuilder($"{participantCount} characters: ");
      sb.Append(string.Join(", ", participants));

      return sb.ToString();
   }

   /// <summary>
   /// Build the example messages prompt chunk that would show to the LLM
   /// how the AIdentity should respond to the user.
   /// </summary>
   /// <param name="chatFeature">The chat feature of the AIdentity.</param>
   /// <param name="aidentityName">The name of the AIdentity.</param>
   /// <returns>The example messages prompt chunk, or null if no example messages are specified.</returns>
   public static string? BuildAIdentityMessageStyleExamples(AIdentityChatFeature? chatFeature, string aidentityName)
   {
      var exampleMessages = chatFeature?.ExampleMessages;
      if (exampleMessages is null or { Count: 0 }) return null;

      var sb = new StringBuilder()
         .AppendLine(PromptTokens.EXAMPLE_MESSAGES_INSTRUCTION)
         .Replace(PromptTokens.TOKEN_AIDENTITY_NAME, aidentityName);

      foreach (var exampleMessage in exampleMessages)
      {
         sb.AppendLine($"User: {exampleMessage.UserMessage}");
         sb.AppendLine($"{aidentityName}: {exampleMessage.AIdentityMessage}");
      }

      return sb.ToString();
   }
}
