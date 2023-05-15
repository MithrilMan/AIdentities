namespace AIdentities.Connector.OpenAI.Models;

public class OpenAISettings : IPluginSettings
{
   public const bool DEFAULT_ENABLED = true;
   public const string DEFAULT_MODEL = "gpt-3.5-turbo";
   public const string DEFAULT_CHAT_ENDPOINT = "https://api.openai.com/v1/chat/completions";
   public const int DEFAULT_TIMEOUT = 30000;

   /// <summary>
   /// Enable or disable the OpenAI API.
   /// </summary>
   public bool Enabled { get; set; } = DEFAULT_ENABLED;

   /// <summary>
   /// OpenAI API Endpoint.
   /// </summary>
   public Uri ChatEndPoint { get; set; } = new Uri(DEFAULT_CHAT_ENDPOINT);

   /// <summary>
   /// OpenAI API Key.
   /// </summary>
   public string? ApiKey { get; set; } = default!;

   /// <summary>
   /// The default model to use if no model has been specified in the request.
   /// </summary>
   public string DefaultModel { get; set; } = DEFAULT_MODEL;

   /// <summary>
   /// The default timeout for the API.
   /// </summary>
   public int Timeout { get; set; } = DEFAULT_TIMEOUT;
}
