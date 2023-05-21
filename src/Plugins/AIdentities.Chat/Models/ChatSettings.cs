﻿namespace AIdentities.Chat.Models;

public class ChatSettings : IPluginSettings
{
   public string? DefaultConnector { get; set; }

   /// <summary>
   /// A list of skills that are enabled for the chat plugin.
   /// This set of skills will be used by the chat plugin to manage conversations through the Chat Keeper AIdentity.
   /// </summary>
   public List<string> EnabledSkills { get; set; } = new();

   /// <summary>
   /// When skills are disabled, conversation will make less use of internal prompts to LLM and will be faster and cheaper (if paid services are used) but
   /// will have less advanced features.
   /// Set this to true to enable skills.
   /// Having this flag set to true but not enabling a single skill will have the same effect as setting this flag to false.
   /// </summary>
   public bool EnableSkills { get; set; }
}
