namespace AIdentities.Connector.TextGeneration.Models.API;

public class ChatCompletionHistoryMessage
{
   [JsonPropertyName("internal")]
   public List<string> Internal { get; set; } = new List<string>();

   [JsonPropertyName("visible")]
   public List<string> Visible { get; set; } = new List<string>();
}
