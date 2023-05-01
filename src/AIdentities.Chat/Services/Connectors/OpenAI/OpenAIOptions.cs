namespace AIdentities.Chat.Services.Connectors.OpenAI;

public class OpenAIOptions
{
   public const string SECTION_NAME = "OpenAI";
   const bool DEFAULT_ENABLED = true;

   /// <summary>
   /// Enable or disable the OpenAI API.
   /// </summary>
   public bool Enabled { get; set; } = DEFAULT_ENABLED;

   /// <summary>
   /// OpenAI API Endpoint.
   /// </summary>
   public string EndPoint { get; set; } = default!;

   /// <summary>
   /// OpenAI API Key.
   /// </summary>
   public string ApiKey { get; set; } = default!;
}
