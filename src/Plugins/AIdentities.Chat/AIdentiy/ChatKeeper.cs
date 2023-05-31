namespace AIdentities.Chat.AIdentiy;

/// <summary>
/// Chat Keeper AIdentity manages any conversation happening in the chat.
/// The user can't edit the main settings but can customize any plugin related settings and also
/// specify which skills it can use, this way the user can customize the Chat Keeper to their liking.
/// If skills are disabled, conversation will make less use of internal prompts to LLM and will be faster and cheaper (if paid services are used) but
/// will have less advanced features.
/// </summary>
public record ChatKeeper : AIdentity
{
   public override bool IsManaged => true;

   public ChatKeeper()
   {
      Id = new Guid("c1111111-1111-1111-1111-111111111111");
      CreatedAt = new DateTimeOffset(2023, 05, 21, 11, 35, 0, TimeSpan.Zero);
      Name = "Chat Keeper";
      Description = "The Chat Keeper is an AIdentity who manages any conversation happening in the chat.";
      Personality = "The Chat Keeper is a very polite, quiet and friendly AIdentity. It will always try to manage conversations behind the scenes, and will only intervene when it is necessary.";
      Tags.Add("system");
   }
}
