﻿namespace AIdentities.Shared.Features.CognitiveEngine;

public class CognitiveEngineProvider : ICognitiveEngineProvider
{
   readonly Dictionary<Type, ICognitiveEngineFactory> _cognitiveEngineFactories;

   public CognitiveEngineProvider(IEnumerable<ICognitiveEngineFactory> cognitiveEngineFactories)
   {
      _cognitiveEngineFactories = cognitiveEngineFactories.ToDictionary(f => f.CognitiveEngineType, f => f);
   }

   /// <inheritdoc/>
   public IEnumerable<Type> KnownCognitiveEngineTypes => _cognitiveEngineFactories.Keys;

   /// <inheritdoc/>
   public ICognitiveEngine CreateCognitiveEngine(Type CognitiveEngineType, AIdentity aIdentity, Action<ICognitiveEngine> configure)
   {
      if (!_cognitiveEngineFactories.TryGetValue(CognitiveEngineType, out var factory))
      {
         throw new ArgumentException($"The cognitive engine type {CognitiveEngineType.Name} is not registered.");
      }

      var engine = factory.CreateCognitiveEngine(aIdentity);
      configure(engine);
      return engine;
   }

   /// <inheritdoc/>
   public ICognitiveEngine CreateCognitiveEngine(string CognitiveEngineTypeName, AIdentity aIdentity, Action<ICognitiveEngine> configure)
   {
      //find the type by searching in the dictionary keys names
      var type = _cognitiveEngineFactories.Keys.FirstOrDefault(k => k.Name == CognitiveEngineTypeName)
         ?? throw new ArgumentException($"The cognitive engine type {CognitiveEngineTypeName} is not registered.");

      var engine = _cognitiveEngineFactories[type].CreateCognitiveEngine(aIdentity);
      configure(engine);
      return engine;
   }

   /// <inheritdoc/>
   public ICognitiveEngine CreateCognitiveEngine(AIdentity aIdentity, Action<ICognitiveEngine> configure)
   {
      //find the type by searching in the dictionary keys names
      var type = _cognitiveEngineFactories.Keys.FirstOrDefault(k => k.Name == aIdentity.DefaultCognitiveEngine)
         ?? throw new ArgumentException($"The configured DefaultCognitiveEngine {aIdentity.DefaultCognitiveEngine} is not registered.");

      var engine = _cognitiveEngineFactories[type].CreateCognitiveEngine(aIdentity);
      configure(engine);
      return engine;
   }

   /// <inheritdoc/>
   public ICognitiveEngine CreateCognitiveEngine<TCognitiveEngine>(AIdentity aIdentity, Action<ICognitiveEngine> configure)
   {
      if (!_cognitiveEngineFactories.TryGetValue(typeof(TCognitiveEngine), out var factory))
      {
         throw new ArgumentException($"The cognitive engine type {typeof(TCognitiveEngine).Name} is not registered.");
      }

      var engine = factory.CreateCognitiveEngine(aIdentity);
      configure(engine);

      return engine;
   }
}
