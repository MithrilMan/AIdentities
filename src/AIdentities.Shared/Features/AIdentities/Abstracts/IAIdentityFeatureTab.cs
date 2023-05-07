namespace AIdentities.Shared.Features.AIdentities.Abstracts;

public interface IAIdentityFeatureTab
{
   /// <summary>
   /// Requests the component to save any changes made to the feature and return an updated version of the feature.
   /// </summary>
   /// <returns>
   /// The updated feature or null if the saving failed.
   /// There is no need to inform the user about the failure, the component will do it.
   /// </returns>
   Task<object?> SaveAsync();

   /// <summary>
   /// Requests the component to undo any changes made to the feature.
   /// </summary>
   /// <returns></returns>
   Task UndoChangesAsync();
}

public interface IAIdentityFeatureTab<TFeature> : IAIdentityFeatureTab
   where TFeature : IAIdentityFeature
{
   /// <summary>
   /// Requests the component to save any changes made to the feature and return an updated version of the feature.
   /// </summary>
   /// <returns>
   /// The updated feature or null if the saving failed.
   /// There is no need to inform the user about the failure, the component will do it.
   /// </returns>
   new Task<TFeature?> SaveAsync();

   /// <summary>
   /// Requests the component to undo any changes made to the feature.
   /// </summary>
   /// <returns></returns>
   new Task UndoChangesAsync();
}
