using AIdentities.Shared.Features.CognitiveEngine.Mission;
using AIdentities.Shared.Features.CognitiveEngine.Prompts;
using AIdentities.Shared.Features.CognitiveEngine.Thoughts;
using AIdentities.Shared.Plugins.Connectors.Completion;
using AIdentities.Shared.Plugins.Connectors.Conversational;
using Polly.Retry;
using Polly;
using System.Collections.Concurrent;
using AIdentities.Shared.Plugins.Connectors.TextToSpeech;
using AIdentities.Shared.Plugins.Connectors;

namespace AIdentities.Shared.Features.CognitiveEngine;

/// <summary>
/// The abstract cognitive engine class.
/// There can be different implementations of this class that differs on how
/// each one handles the prompts.
/// </summary>
public abstract class CognitiveEngine<TCognitiveContext> : ICognitiveEngine
   where TCognitiveContext : CognitiveContext
{
   protected readonly ILogger _logger;
   protected readonly ISkillManager _skillManager;

   /// <summary>
   /// Dictionary that holds the default connectors for each type of connector the cognitive engine supports.
   /// </summary>
   private readonly ConcurrentDictionary<Type, IConnector> _defaultConnectors = new();

   public AIdentity AIdentity { get; }

   public TCognitiveContext Context { get; }

   CognitiveContext ICognitiveEngine.Context => Context;

   protected DefaultConversationalMessage PersonalityInstruction { get; private set; } = default!;

   protected AsyncRetryPolicy RetryPolicy { get; private set; }

   protected IConversationalConnector DefaultConversationalConnector => GetDefaultConnector<IConversationalConnector>();

   /// <summary>
   /// Holds a list of skills that are enabled for the AIdentity.
   /// The user can enable or disable skills for an AIdentity in the AIdentity settings.
   /// </summary>
   protected HashSet<SkillDefinition> EnabledSkills = default!;

   public CognitiveEngine(ILogger logger,
                          AIdentity aIdentity,
                          IConversationalConnector defaultConversationalConnector,
                          ICompletionConnector defaultCompletionConnector,
                          ISkillManager skillManager)
   {
      _logger = logger;
      AIdentity = aIdentity;

      SetDefaultConversationalConnector(defaultConversationalConnector);
      SetDefaultCompletionConnector(defaultCompletionConnector);

      _skillManager = skillManager;

      EnsureAIdentityIsValid();

      Context = CreateCognitiveContext();
      RetryPolicy = CreateRetryPolicy();

      SetupAIdentityPersonality();
      ConfigureAvailableSkills();
   }


   /// <summary>
   /// Setup the available skills for the AIdentity.
   /// Each AIdentity can have different skills available based on the user
   /// preferences so we take that into account here.
   /// Also, before executing a skill, there should be a check to
   /// ensure that a mission doesn't have skill constraints that should
   /// prevent the skill from being executed, but we can't store that here
   /// because a cognitive engine can interact with multiple missions or
   /// without a mission at all.
   /// </summary>
   protected virtual void ConfigureAvailableSkills()
   {
      var settings = AIdentity.Features.Get<AIdentityFeatureSkills>();
      if (settings is null || !settings.AreSkillsEnabled)
      {
         _logger.LogDebug("The AIdentity {AIdentityName} have skills disabled.", AIdentity.Name);
         EnabledSkills = new();
         return;
      }

      EnabledSkills = settings.EnabledSkills
         .Select(s => _skillManager.GetSkillDefinition(s)!)
         .Where(s => s is not null)
         .ToHashSet();
   }

   public abstract TCognitiveContext CreateCognitiveContext();

   /// <summary>
   /// Ensure that the AIdentity is valid and meet the minimum requirements.
   /// </summary>
   /// <exception cref="ArgumentNullException"></exception>
   protected virtual void EnsureAIdentityIsValid()
   {
      if (AIdentity is null) throw new ArgumentNullException(nameof(AIdentity));
      if (AIdentity.Name is null)
      {
         _logger.LogError("The AIdentity {AIdentityId} doesn't have a name.", AIdentity.Id);
         throw new ArgumentNullException(nameof(AIdentity.Name));
      }
      if (string.IsNullOrEmpty(AIdentity.Personality))
      {
         _logger.LogWarning("The AIdentity {AIdentity} ({AIdentityId}) doesn't have a personality.", AIdentity.Name, AIdentity.Id);
      }
   }

   /// <summary>
   /// Setup the personality of the AIdentity.
   /// </summary>
   protected virtual void SetupAIdentityPersonality()
   {
      PersonalityInstruction = new DefaultConversationalMessage(
                 Role: DefaultConversationalRole.System,
                 Content: AIdentity.Personality!,
                 Name: AIdentity.Name);
   }

   /// <inheritdoc />
   public abstract IAsyncEnumerable<Thought> HandlePromptAsync(Prompt prompt, MissionContext? missionContext, CancellationToken cancellationToken);

   public IAsyncEnumerable<Thought> ExecuteSkill(ISkill skill, Prompt prompt, SkillExecutionContext? skillExecutionContext, CancellationToken cancellationToken)
   {
      var context = skillExecutionContext ?? new SkillExecutionContext(
         skill,
         cognitiveContext: Context,
         missionContext: null
         );

      var jsonArgs = SkillRegexUtils.ExtractJson().Match(prompt.Text).Value;
      //jsonArgs may contain the arguments to pass to the skill
      //try to apply them to the skillContext using the SkillDefinition of the skill
      if (!string.IsNullOrEmpty(jsonArgs))
      {
         try
         {
            var args = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonArgs);
            if (args != null)
            {
               foreach (var arg in args)
               {
                  context.SetInput(arg.Key, arg.Value);
               }
            }
         }
         catch (Exception)
         {
            ActionThought($"I couldn't parse the arguments for the skill {skill.Definition.Name}.");
         }
      }

      return skill.ExecuteAsync(prompt, context, cancellationToken);
   }

   /// <summary>
   /// Generates an <see cref="InvalidPromptResponseThought"/>.
   /// This is a special type of <see cref="IntrospectiveThought"/> that is used when the
   /// cognitive engine is not able to handle the conversion the AIdentity response to a prompt
   /// into a valid value for the skill action.
   /// </summary>
   /// <param name="howToFix">The explaination about how to fix the invalid response, or the error description.</param>
   /// <returns>The invalid prompt thought.</returns>
   public InvalidArgumentsThought InvalidArgumentsThought(string howToFix)
      => new(null, AIdentity, howToFix);

   /// <summary>
   /// Generates an <see cref="InvalidPromptResponseThought"/>.
   /// This is a special type of <see cref="IntrospectiveThought"/> that is used when the 
   /// cognitive engine is not able to understand the user's response to a prompt.
   /// In this specific case, the caller should try to provide missing arguments before invoking again the action.
   /// </summary>
   /// <param name="missingArguments">The list of missing arguments.</param>
   /// <returns>The invalid prompt response thought.</returns>
   public MissingArgumentsThought MissingArgumentsThought(params string[] missingArguments)
      => new MissingArgumentsThought(null, AIdentity, missingArguments);

   public IntrospectiveThought IntrospectiveThought(string thought)
      => new IntrospectiveThought(null, AIdentity, thought);

   /// <summary>
   /// Produces an <see cref="ActionThought"/>.
   /// Consider this as a way to log what the cognitive engine is doing.
   /// </summary>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The action thought.</returns>
   public ActionThought ActionThought(string thought)
      => new ActionThought(null, AIdentity, thought);

   /// <summary>
   /// Generates a <see cref="FinalThought"/>.
   /// A final thought is something that the cognitive engine can return to the user.
   /// </summary>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The final thought.</returns>
   public FinalThought FinalThought(string thought)
      => new FinalThought(null, AIdentity, thought);

   /// <summary>
   /// Generates a <see cref="StreamedThought"/>.
   /// A streamed thought is supposed to be updated until it generates the last chunk of information.
   /// At that point StreamedThought.IsStreamComplete should be set to true.
   /// </summary>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The first streamed thought.</returns>
   public StreamedFinalThought StreamFinalThought(string thought)
      => new StreamedFinalThought(null, AIdentity, thought);

   /// <summary>
   /// Generates a <see cref="StreamedThought"/>.
   /// A streamed thought is supposed to be updated until it generates the last chunk of information.
   /// At that point StreamedThought.IsStreamComplete should be set to true.
   /// </summary>
   /// <param name="thought">The thought to produce.</param>
   /// <returns>The first streamed thought.</returns>
   public StreamedIntrospectiveThought StreamIntrospectiveThought(string thought)
      => new StreamedIntrospectiveThought(null, AIdentity, thought);


   /// <summary>
   /// Creates the retry policy for the cognitive engine.
   /// This is applied everytime a call to a generation API fails with an
   /// exception specified in the retry policy.
   /// The default implementation is a simple exponential backoff that tries 3 times
   /// and catch all exceptions.
   /// </summary>
   /// <returns></returns>
   private AsyncRetryPolicy CreateRetryPolicy()
   {
      // Define the retry policy
      var retryPolicy = Policy
         .Handle<Exception>()
         .WaitAndRetryAsync(
         3,
         retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
         (exception, timeSpan, retryCount, context) =>
         {
            _logger.LogWarning("Retry {RetryCount} due to {ExceptionType} with message {Message}",
                               retryCount,
                               exception.GetType().Name,
                               exception.Message);
         });

      return retryPolicy;
   }

   public void SetDefaultConversationalConnector<TConnector>(TConnector connector) where TConnector : IConversationalConnector
      => _defaultConnectors.AddOrUpdate(typeof(TConnector), connector, (_, _) => connector);

   public void SetDefaultCompletionConnector<TConnector>(TConnector connector) where TConnector : ICompletionConnector
      => _defaultConnectors.AddOrUpdate(typeof(TConnector), connector, (_, _) => connector);

   public void SetDefaultTextToSpeechConnector<TConnector>(TConnector connector) where TConnector : ITextToSpeechConnector
      => _defaultConnectors.AddOrUpdate(typeof(TConnector), connector, (_, _) => connector);

   public TConnector GetDefaultConnector<TConnector>() where TConnector : IConnector
   {
      var connector = _defaultConnectors.GetOrAdd(typeof(TConnector), _
         => throw new InvalidOperationException($"No default connector of type {typeof(TConnector).Name} has been set."));

      return (TConnector)connector;
   }
}
