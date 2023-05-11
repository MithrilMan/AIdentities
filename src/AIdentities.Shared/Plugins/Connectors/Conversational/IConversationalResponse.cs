﻿namespace AIdentities.Shared.Plugins.Connectors.Conversational;

/// <summary>
/// A response from a conversational endpoint.
/// The only expected result from all the required property is <see cref="GeneratedMessage"/>.
/// Other properties are optional and depend on the <see cref="IConversationalConnector"/> capabilities.
/// </summary>
public interface IConversationalResponse
{
   /// <summary>
   /// The generated response.
   /// This is the only expected property that should be filled by any Conversational Connector.
   /// </summary>
   string? GeneratedMessage { get; init; }

   /// <summary>
   /// The number of tokens used by the prompt.
   /// </summary>
   public int? PromptTokens { get; set; }

   /// <summary>
   /// The number of tokens generated by the response.
   /// </summary>
   int? CompletionTokens { get; init; }

   /// <summary>
   /// The total number of used tokens, given by the prompt and the response.
   /// </summary>
   int? TotalTokens { get; init; }

   /// <summary>
   /// The time it took to generate the response.
   /// </summary>
   TimeSpan ResponseTime { get; init; }

   /// <summary>
   /// The finish reason of the response.
   /// </summary>
   public string? FinishReadon { get; init; }
}
