﻿namespace AIdentities.BrainButler.Models;

/// <summary>
/// A command execution can be streamed, and this class is used to send the fragments of the execution to the user.
/// Some commands may take a long time to execute, and this allows to send the results as they are generated.
/// </summary>
/// <param name="Message"></param>
/// <param name="IsMarkupMessage"></param>
public record CommandExecutionStreamedFragment(string Message, bool IsMarkupMessage = false)
{
   /// <summary>
   /// The message to send to the user.
   /// </summary>
   public string? Message { get; } = Message;

   /// <summary>
   /// True when the message is a markup message that has to be renderer as raw and may contain HTML tags.
   /// </summary>
   public bool IsMarkupMessage { get; } = IsMarkupMessage;
}

/// <summary>
/// A special type of <see cref="CommandExecutionStreamedFragment"/> that contain an Artifact at any time, even if usually it is sent at the end of a successfull execution.
/// </summary>
/// <param name="Message"></param>
/// <param name="IsMarkupMessage"></param>
public record CommandExecutionStreamedFragmentWithArtifact(object Artifact, string Message, bool IsMarkupMessage = false)
   : CommandExecutionStreamedFragment(Message, IsMarkupMessage)
{
   /// <summary>
   /// The artifact generated by the command execution.
   /// </summary>
   public object Artifact { get; set; } = Artifact;

   /// <summary>
   /// Specifies whether the command execution has generated an artifact.
   /// </summary>
   public bool HasArtifact => Artifact != null;
}
