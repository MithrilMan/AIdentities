namespace AIdentities.Chat.Skills.InviteFriend.Events;

/// <summary>
/// Event raised by the InviteFriend skill to invite a friend to a conversation.
/// </summary>
/// <param name="Aidentity"></param>
public record InviteToConversation(AIdentity AIdentity);
