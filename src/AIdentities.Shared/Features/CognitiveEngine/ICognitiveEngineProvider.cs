namespace AIdentities.Shared.Features.CognitiveEngine;

/// <summary>
/// Creates a specific cognitive engine for a specific AIdentity.
/// This interface has to be used by whoever wants to create a cognitive engine for a specific AIdentity.
/// </summary>
public interface ICognitiveEngineProvider
{
   /// <summary>
   /// The list of known cognitive engine types.
   /// </summary>
   IEnumerable<Type> KnownCognitiveEngineTypes { get; }

   /// <summary>
   /// Creates a specific cognitive engine for a specific AIdentity.
   /// </summary>
   /// <param name="CognitiveEngineType">The type of the cognitive engine to create.</param>
   /// <param name="aIdentity">The AIdentity for which the cognitive engine is created.</param>
   /// <param name="configure">An action to configure the cognitive engine.</param>
   /// <returns>The created cognitive engine.</returns>
   ICognitiveEngine CreateCognitiveEngine(Type CognitiveEngineType, AIdentity aIdentity, Action<ICognitiveEngine> configure);

   /// <summary>
   /// Creates a specific cognitive engine for a specific AIdentity.
   /// </summary>
   /// <param name="CognitiveEngineTypeName">The name of the type of the cognitive engine to create.</param>
   /// <param name="aIdentity">The AIdentity for which the cognitive engine is created.</param>
   /// <param name="configure">An action to configure the cognitive engine.</param>
   /// <returns>The created cognitive engine.</returns>
   ICognitiveEngine CreateCognitiveEngine(string CognitiveEngineTypeName, AIdentity aIdentity, Action<ICognitiveEngine> configure);

   /// <summary>
   /// Creates a cognitive engine for a specific AIdentity based on <see cref="AIdentity.DefaultCognitiveEngine"/>.
   /// </summary>
   /// <param name="CognitiveEngineTypeName">The name of the type of the cognitive engine to create.</param>
   /// <param name="aIdentity">The AIdentity for which the cognitive engine is created.</param>
   /// <param name="configure">An action to configure the cognitive engine.</param>
   /// <returns>The created cognitive engine.</returns>
   ICognitiveEngine CreateCognitiveEngine(AIdentity aIdentity, Action<ICognitiveEngine> configure);

   /// <summary>
   /// Creates the specified cognitive engine for a specific AIdentity.
   /// </summary>
   /// <typeparam name="TCognitiveEngine">The type of the cognitive engine to create.</typeparam>
   /// <param name="aIdentity">The AIdentity for which the cognitive engine is created.</param>
   /// <param name="configure">An action to configure the cognitive engine.</param>
   /// <returns>The created cognitive engine.</returns>
   ICognitiveEngine CreateCognitiveEngine<TCognitiveEngine>(AIdentity aIdentity, Action<ICognitiveEngine> configure);
}
