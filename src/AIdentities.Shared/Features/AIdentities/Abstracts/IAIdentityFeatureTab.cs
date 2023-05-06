namespace AIdentities.Shared.Features.AIdentities.Abstracts;

public interface IAIdentityFeatureTab<TFeature>
   where TFeature : IAIdentityFeature
{
   /// <summary>
   /// Requests the component to save any changes made to the feature and return an updated version of the feature.
   /// </summary>
   /// <returns>The updated feature or null if the saving failed.</returns>
   Task<TFeature?> SaveAsync();

   /// <summary>
   /// Requests the component to undo any changes made to the feature.
   /// </summary>
   /// <returns></returns>
   Task UndoChangesAsync();
}
