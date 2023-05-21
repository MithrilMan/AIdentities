Embedded AIdentities

Each plugin can define an embedded AIdentity that the user can't fully control, and which is used by the plugin itself.

For example, the Chat plugin defines its own "Chat Keeper" AIdentity as the owner of any chat conversation. Whenever the user interacts with the chat, the Chat Keeper controls whether it needs to execute a skill or if the prompt is just a conversational prompt that will then be taken into consideration by the participants of the conversation.

Note that the chat itself is configured as a Mission whose goal is to have a conversation with one or more AIdentities.