namespace AIdentities.Connector.TextGeneration.Models;

public class TextGenerationSettings : IPluginSettings
{
   public const bool DEFAULT_ENABLED = true;
   public const string DEFAULT_MODEL = "";
   public const string DEFAULT_CHAT_ENDPOINT = "http://localhost:5000/api/v1/generate";
   public const string DEFAULT_CHAT_STREAMED_ENDPOINT = "ws://localhost:5005/api/v1/stream";
   public const int DEFAULT_TIMEOUT = 30000;

   /// <summary>
   /// Enable or disable the TextGeneration API.
   /// </summary>
   public bool Enabled { get; set; } = DEFAULT_ENABLED;

   /// <summary>
   /// TextGeneration API Endpoint.
   /// </summary>
   public Uri ChatEndPoint { get; set; } = new Uri(DEFAULT_CHAT_ENDPOINT);
   
   /// <summary>
   /// TextGeneration API Endpoint.
   /// </summary>
   public Uri StreamedChatEndPoint { get; set; } = new Uri(DEFAULT_CHAT_STREAMED_ENDPOINT);
   
   /// <summary>
   /// TextGeneration API Key.
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

   public TextGenerationParameters DefaultParameters { get; set; } = new();
}
