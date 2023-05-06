using AIdentities.Shared.Common;
using Microsoft.AspNetCore.Http.Features;

namespace AIdentities.Shared.Features.AIdentities.Models;

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
   /// The description of the AIdentity.
   /// It's not used by the LLM, but it's useful for the user to know what the AIdentity is about.
   /// </summary>
   public string? Description { get; set; }

   /// <summary>
   /// The AIdentity's personality.
   /// The usage depends on the feature using the AIdentity.
   /// </summary>
   public string? Personality { get; set; }

   /// <summary>
   /// The features of the AIdentity.
   /// Plugin developers can use this to add and manage custom features to their AIdentities or access other plugins' features.
   /// </summary>
   public FeatureCollection Features { get; set; } = new();
}
