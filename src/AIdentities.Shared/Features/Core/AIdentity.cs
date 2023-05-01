using AIdentities.Shared.Common;
using Microsoft.AspNetCore.Http.Features;

namespace AIdentities.Shared.Features.Core;

/// <summary>
/// Represents an AIdentity, which is a conversional AI that can be used to chat with a user.
/// Each AIdentity has a unique name and description.
/// Example of AIdentity:
/// - AIdentity Name: Alice
/// - AIdentity Description: A nice and friendly AIdentity that loves to chat with people with a bit of sarcasm. She's a bit shy and doesn't like to talk about herself.
/// - AIdentity Personality: Friendly and nice with a bit of sarcasm. She's a bit shy and doesn't like to talk about herself.
/// </summary>
public record AIdentity : Entity
{
   /// <summary>
   /// The date and time when the AIdentity was created.
   /// </summary>
   public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

   /// <summary>
   /// The date and time when the AIdentity was last updated.
   /// </summary>
   public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

   /// <summary>
   /// The name of the AIdentity.
   /// This is how the LLM will refer to the AIdentity.
   /// </summary>
   public string? Name { get; set; }

   /// <summary>
   /// A base64 data URI of the AIdentity's image.
   /// </summary>
   public string? Image { get; set; } = default!;

   /// <summary>
   /// AIdentity's background.
   /// This is injected in the LLM prompt to make the AIdentity adhere to the background.
   /// You can for example specify where the AIdentity is from, or what it does for a living.
   /// </summary>
   public string? Background { get; set; }

   /// <summary>
   /// The description of the AIdentity.
   /// It's not used by the LLM, but it's useful for the user to know what the AIdentity is about.
   /// </summary>
   public string? Description { get; set; }

   /// <summary>
   /// The full prompt passed to the LLM to start the conversation.
   /// This is optional and can be left empty.
   /// When specified, the LLM will use this prompt to start the conversation and will ignore the other fields.
   /// </summary>
   public string? FullPrompt { get; set; }

   /// <summary>
   /// The AIdentity's personality.
   /// This is injected in the LLM prompt to make the AIdentity behave following a specific personality.
   /// Ignored if FullPrompt is specified.
   /// </summary>
   public string? Personality { get; set; }

   /// <summary>
   /// The first message sent by the AIdentity when a new conversation starts.
   /// </summary>
   public string? FirstMessage { get; set; }

   /// <summary>
   /// Whether the AIdentity should use the full prompt or not.
   /// When using the full prompt, the LLM will ignore the other fields and use the FullPrompt to start the conversation.
   /// </summary>
   public bool UseFullPrompt { get; set; }

   /// <summary>
   /// The features of the AIdentity.
   /// Plugin developers can use this to add and manage custom features to their AIdentities or access other plugins' features.
   /// </summary>
   public FeatureCollection Features { get; set; } = new();
}
