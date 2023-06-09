﻿using System.ComponentModel.DataAnnotations;

namespace AIdentities.Shared.Plugins.Connectors.Conversational;
/// <summary>
/// A message to be sent to the conversational connector
/// </summary>
public record DefaultConversationalMessage(DefaultConversationalRole Role, string Content, string? Name) : IConversationalMessage
{
   /// <summary>
   /// The role of the author of this message.
   /// </summary>
   /// <value>The role of the author of this message.</value>
   [Required]
   public DefaultConversationalRole Role { get; init; } = Role;

   /// <summary>
   /// The contents of the message
   /// </summary>
   /// <value>The contents of the message</value>
   [Required]
   public string Content { get; init; } = Content;

   /// <summary>
   /// The name of the user in a multi-user chat
   /// </summary>
   /// <value>The name of the user in a multi-user chat</value>
   public string? Name { get; init; } = Name;
}
