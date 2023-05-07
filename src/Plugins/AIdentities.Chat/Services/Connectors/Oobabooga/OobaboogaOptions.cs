namespace AIdentities.Chat.Services.Connectors.Oobabooga;

public class OobaboogaOptions
{
   public const string SECTION_NAME = "Oobabooga";
   const bool DEFAULT_ENABLED = false;

   /// <summary>
   /// Enable or disable the OpenAI API.
   /// </summary>
   public bool Enabled { get; set; } = DEFAULT_ENABLED;

   /// <summary>
   /// OpenAI API Endpoint.
   /// </summary>
   public string? EndPoint { get; set; } = default!;


   /// <summary>
   /// OpenAI API Key.
   /// </summary>
   public string ApiKey { get; set; } = default!;
}
