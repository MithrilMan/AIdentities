using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AIdentities.Chat.Services.Connectors.OpenAI.API;

/// <summary>
/// The role of the author of this message.
/// </summary>
/// <value>The role of the author of this message.</value>
[JsonConverter(typeof(JsonStringEnumConverterEx<ChatCompletionRoleEnum>))]
public enum ChatCompletionRoleEnum
{
   /// <summary>
   /// Enum SystemEnum for system
   /// </summary>
   [EnumMember(Value = "system")]
   System = 0,
   /// <summary>
   /// Enum UserEnum for user
   /// </summary>
   [EnumMember(Value = "user")]
   User = 1,
   /// <summary>
   /// Enum AssistantEnum for assistant
   /// </summary>
   [EnumMember(Value = "assistant")]
   Assistant = 2
}
