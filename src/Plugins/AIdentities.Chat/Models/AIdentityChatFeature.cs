﻿namespace AIdentities.Chat.Models;
public record AIdentityChatFeature: IAIdentityFeature
{
   /// <summary>
   /// AIdentity's background.
   /// This is injected in the LLM prompt to make the AIdentity adhere to the background.
   /// You can for example specify where the AIdentity is from, or what it does for a living.
   /// </summary>
   public string? Background { get; set; }

   /// <summary>
   /// The full prompt passed to the LLM to start the conversation.
   /// This is optional and can be left empty.
   /// When specified, the LLM will use this prompt to start the conversation and will ignore the other fields.
   /// </summary>
   public string? FullPrompt { get; set; }

   /// <summary>
   /// The first message sent by the AIdentity when a new conversation starts.
   /// </summary>
   public string? FirstMessage { get; set; }

   /// <summary>
   /// Whether the AIdentity should use the full prompt or not.
   /// When using the full prompt, the LLM will ignore the other fields and use the FullPrompt to start the conversation.
   /// </summary>
   public bool UseFullPrompt { get; set; }
}