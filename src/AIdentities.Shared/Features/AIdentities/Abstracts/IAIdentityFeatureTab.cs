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

   /// <summary>
   /// This is the parameter a tab has to implement to be able to be used in the 
   /// <see cref="AIdentityFeatureTab"/> component.
   /// It is used to signal that a tab has been changed and needs to be saved.
   /// </summary>
   bool IsChanged { get; set; }
   /// <summary>
   /// This is the parameter a tab has to implement to be able to be used in the
   /// <see cref="AIdentityFeatureTab"/> component.
   /// It is used to specify the AIdentity the feature belongs to.
   /// </summary>
   AIdentity AIdentity { get; set; }
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

   /// <summary>
   /// This is the parameter a tab has to implement to be able to be used in the
   /// <see cref="AIdentityFeatureTab"/> component.
   /// It is used to specify the feature to be edited.
   /// </summary>
   TFeature Feature { get; set; }
}
