﻿namespace AIdentities.Connector.OpenAI.Models.API;

public record CreateCompletionRequest
{
   /// <summary>
   /// ID of the model to use. Currently, only &#x60;gpt-3.5-turbo&#x60; and &#x60;gpt-3.5-turbo-0301&#x60; are supported.
   /// </summary>
   [Required]
   [JsonPropertyName("model")]
   public string Model { get; init; } = default!;

   /// <summary>
   /// The messages to generate chat completions for, in the [chat format](/docs/guides/chat/introduction).
   /// </summary>
   [Required]
   [JsonPropertyName("prompt")]
   public string Prompt { get; init; } = "";

   /// <summary>
   /// The suffix that comes after a completion of inserted text.
   /// </summary>
   [JsonPropertyName("suffix")]
   public string? Suffix { get; init; }

   /// <summary>
   /// What sampling temperature to use, between 0 and 2.
   /// Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
   /// We generally recommend altering this or &#x60;top_p&#x60; but not both. 
   /// </summary>
   [Range(0, 2)]
   [JsonPropertyName("temperature")]
   public float? Temperature { get; init; }

   /// <summary>
   /// An alternative to sampling with temperature, called nucleus sampling, where the model considers the results of the tokens with top_p probability mass.
   /// So 0.1 means only the tokens comprising the top 10% probability mass are considered.
   /// We generally recommend altering this or &#x60;temperature&#x60; but not both. 
   /// </summary>
   [Range(0, 1)]
   [JsonPropertyName("top_p")]
   public float? TopP { get; init; }

   /// <summary>
   /// How many chat completion choices to generate for each input message.
   /// </summary>
   [Range(1, 128)]
   [JsonPropertyName("n")]
   public int? N { get; init; }

   /// <summary>
   /// If set, partial message deltas will be sent, like in ChatGPT.
   /// Tokens will be sent as data-only [server-sent events](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events/Using_server-sent_events#Event_stream_format) as
   /// they become available, with the stream terminated by a &#x60;data: [DONE]&#x60; message. 
   /// </summary>
   [JsonPropertyName("stream")]
   public bool? Stream { get; init; }

   /// <summary>
   /// Up to 4 sequences where the API will stop generating further tokens. 
   /// </summary>
   [JsonPropertyName("stop")]
   public IEnumerable<string>? Stop { get; init; }

   /// <summary>
   /// The maximum number of tokens allowed for the generated answer.
   /// By default, the number of tokens the model can return will be (4096 - prompt tokens). 
   /// </summary>
   [JsonPropertyName("max_tokens")]
   public int? MaxTokens { get; init; }

   /// <summary>
   /// Number between -2.0 and 2.0.
   /// Positive values penalize new tokens based on whether they appear in the text so far, increasing the model&#x27;s likelihood to talk about new topics.
   /// [See more information about frequency and presence penalties.](/docs/api-reference/parameter-details) .
   /// </summary>
   [Range(-2, 2)]
   [JsonPropertyName("presence_penalty")]
   public float? PresencePenalty { get; init; }

   /// <summary>
   /// Number between -2.0 and 2.0.
   /// Positive values penalize new tokens based on their existing frequency in the text so far, decreasing the model&#x27;s likelihood to repeat the same line verbatim.
   /// [See more information about frequency and presence penalties.](/docs/api-reference/parameter-details) 
   /// </summary>
   [Range(-2, 2)]
   [JsonPropertyName("frequency_penalty")]
   public float? FrequencyPenalty { get; init; }

   /// <summary>
   /// Modify the likelihood of specified tokens appearing in the completion.
   /// Accepts a json object that maps tokens (specified by their token ID in the tokenizer) to an associated bias value from -100 to 100.
   /// Mathematically, the bias is added to the logits generated by the model prior to sampling.
   /// The exact effect will vary per model, but values between -1 and 1 should decrease or increase likelihood of selection;
   /// values like -100 or 100 should result in a ban or exclusive selection of the relevant token. 
   /// </summary>
   [JsonPropertyName("logit_bias")]
   public object? LogitBias { get; init; }

   /// <summary>
   /// A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.
   /// [Learn more](/docs/guides/safety-best-practices/end-user-ids). 
   /// </summary>
   [JsonPropertyName("user")]
   public string? User { get; init; }
}
