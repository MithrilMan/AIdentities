namespace AIdentities.Shared.Features.CognitiveEngine.Thoughts;

/// <summary>
/// An introspective thought is something that is not meant to be returned to the user but it's
/// a thought that is used to introspect the current state of the cognitive engine.
/// The engine can use it to improve its reasoning or can communicate it to callers that are interested in it.
/// </summary>
public interface IIntrospectiveThought : IThought { }
