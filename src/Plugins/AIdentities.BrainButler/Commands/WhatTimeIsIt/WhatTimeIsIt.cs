using System.Runtime.CompilerServices;

namespace AIdentities.BrainButler.Commands.WhatTimeIsIt;

public partial class WhatTimeIsIt : CommandDefinition
{
   public const string NAME = nameof(WhatTimeIsIt);
   const string ACTIVATION_CONTEXT = "The user wants to know the current time or a past/future time";
   const string RETURN_DESCRIPTION = "The requested time";
   const string ARGUMENT_TIME_DIFFERENCE = "TimeDifference";
   const string EXAMPLES = $$"""
      UserRequest: What time will be in 20 minutes?
      Reasoning: The time in 20 minutes is the current time plus 20 minutes. 20 minutes are 1200 seconds, that's the amount of seconds I've to add to the current time.
      JSON: { "{{ARGUMENT_TIME_DIFFERENCE}}": 1200 }

      UserRequest: What time was 2 hours and 15 seconds ago?
      Reasoning: The time 2 hours and 15 seconds ago is the current time minus 2 hours and 15 seconds. 1 hour is 3600 seconds, so 2 hours are 7200 seconds, add 15 
      JSON: { "{{ARGUMENT_TIME_DIFFERENCE}}": -7212 }
      
      UserRequest: What's the time?
      Reasoning: I've nothing to add or remove from current time, so I've just to return the current time.
      JSON: { "{{ARGUMENT_TIME_DIFFERENCE}}": 0}
      """;

   private readonly List<CommandArgumentDefinition> _arguments = new()
   {
      new CommandArgumentDefinition(ARGUMENT_TIME_DIFFERENCE, "How much the time, in seconds, should be shifted from the current time", true, typeof(int))
   };
   readonly ILogger<WhatTimeIsIt> _logger;
   readonly IConnectorsManager _connectorsManager;
   readonly IPluginStorage<PluginEntry> _pluginStorage;

   public WhatTimeIsIt(ILogger<WhatTimeIsIt> logger,
                             IConnectorsManager connectorsManager,
                             IPluginStorage<PluginEntry> pluginStorage
                             )
      : base(NAME, ACTIVATION_CONTEXT, RETURN_DESCRIPTION, EXAMPLES)
   {
      _logger = logger;
      _connectorsManager = connectorsManager;
      _pluginStorage = pluginStorage;

      Arguments = _arguments;
   }

   public override async IAsyncEnumerable<CommandExecutionStreamedFragment> ExecuteAsync(string userPrompt,
                                                                string? inputPrompt,
                                                                [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var connector = _connectorsManager.GetConversationalConnector()
         ?? throw new InvalidOperationException("No conversational connector is enabled");

      if (string.IsNullOrWhiteSpace(inputPrompt))
      {
         yield return new CommandExecutionStreamedFragment("The input prompt is empty");
         yield break;
      }

      if (!TryExtractJson<Args>(inputPrompt, out var args))
      {
         yield return new CommandExecutionStreamedFragment($"I couldn't properly execute the command because I haven't generated a valid JSON out of my thoughts: {inputPrompt}");
         yield break;
      }


      yield return new CommandExecutionStreamedFragment($"I'm going to calculate the time difference of {args.TimeDifference} seconds from now, it will take an hour... joking.");

      var askedTime = DateTime.Now.AddSeconds(args.TimeDifference);
      string result = $"the result of the time {args.TimeDifference} seconds from now is {askedTime}";

      var finalResponse = await connector.RequestChatCompletionAsync(new DefaultConversationalRequest()
      {
         Messages = new List<IConversationalMessage>() {
            new DefaultConversationalMessage(DefaultConversationalRole.System, "You are Brain Butler, a funny helpful AI agent that loves to make joke when giving back informations.",null),
            new DefaultConversationalMessage(DefaultConversationalRole.System, $"""
               The answer to the user request is: {result}
               Adjust the answer using your funny personality but ensure to include the result that's "{askedTime}".
               Detect and reply using the same user language
               """,null),
            new DefaultConversationalMessage(DefaultConversationalRole.User, userPrompt, null),
          }
      }, cancellationToken).ConfigureAwait(false);

      yield return new CommandExecutionStreamedFragment(finalResponse?.GeneratedMessage ?? "---BUG---");
   }
}
