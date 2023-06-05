using System.Runtime.Serialization;

namespace AIdentities.Connector.TextGeneration.Models.API;

public enum ChatCompletionMode
{
   [EnumMember(Value = "chat")]
   Chat,
   [EnumMember(Value = "chat-instruct")]
   ChatInstruct,
   [EnumMember(Value = "instruct")]
   Instruct
}
