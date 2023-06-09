﻿using AIdentities.Shared.Features.AIdentities.Services;
using AIdentities.Shared.Features.Core.Abstracts;
using AIdentities.Shared.Features.Core.Services;
using AIdentities.Shared.Plugins.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace AIdentities.Shared.Plugins;

/// <summary>
/// A base implementation of the plugin entry point.
/// It has to be implemented by the plugin and it is used to register the plugin services and declare its capabilities.
/// Contains some helper methods to register features.
/// </summary>
public abstract class BasePluginEntry<TPluginEntry> : IPluginEntry
   where TPluginEntry : BasePluginEntry<TPluginEntry>
{
   protected PluginManifest _manifest = default!;
   private IServiceCollection _services = default!;

   void IPluginEntry.Initialize(PluginManifest manifest, IServiceCollection services)
   {
      _manifest = manifest;
      _services = services;


      // automatically register the plugin storage for the instance that extends this class.
      services.AddScoped<IPluginStorage<TPluginEntry>>(sp =>
         {
            var factory = sp.GetRequiredService<IPluginStorageFactory>();
            var pluginStorage = factory.CreatePluginStorage<TPluginEntry>(manifest);
            return pluginStorage;
         });

      RegisterServices(services);
   }

   /// <summary>
   /// Registers a feature to be exposed in the AIdentity management page.
   /// </summary>
   /// <typeparam name="TFeature">The AIdentity feature type that will be saved along the AIdentity.</typeparam>
   /// <typeparam name="TFeatureTab">The AIdentity feature component Type that will be used to edit the AIdentity feature.</typeparam>
   /// <param name="uiTitle">The UI title that will be shown on the Tab Panel.</param>
   /// <exception cref="InvalidOperationException">Thrown if the plugin is not initialized.</exception>
   protected void RegisterAIdentityFeature<TFeature, TFeatureTab>(string uiTitle)
     where TFeature : class, IAIdentityFeature
     where TFeatureTab : class, IAIdentityFeatureTab<TFeature>
   {
      if (_services == null) throw new InvalidOperationException("Cannot register a feature before the plugin is initialized.");

      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      _services.AddSingleton(new AIdentityFeatureRegistration(typeof(TFeature), typeof(TFeatureTab), uiTitle));
   }

   /// <summary>
   /// Registers a plugin settings to be exposed in the Settings management page.
   /// </summary>
   /// <typeparam name="TPluginSettings">The plugin settings Type that will be saved into the ApplicationSettings.</typeparam>
   /// <typeparam name="TPluginSettingsTab">The plugin settings component Type that will be used to edit the plugin settings.</typeparam>
   /// <param name="uiTitle">The UI title that will be shown on the Tab Panel.</param>
   /// <exception cref="InvalidOperationException">Thrown if the plugin is not initialized.</exception>
   protected void RegisterPluginSettings<TPluginSettings, TPluginSettingsTab>(string uiTitle)
     where TPluginSettings : class, IPluginSettings
     where TPluginSettingsTab : class, IPluginSettingsTab<TPluginSettings>
   {
      if (_services == null) throw new InvalidOperationException("Cannot register a feature before the plugin is initialized.");

      // Register the AIdentity feature to expose an editor in the AIdentity management page.
      _services.AddScoped(sp => new PluginSettingRegistration(sp.GetRequiredService<IPluginStorage<TPluginEntry>>(), typeof(TPluginSettings), typeof(TPluginSettingsTab), uiTitle));
   }

   /// <summary>
   /// Registers a plugin startup service.
   /// A plugin startup service is used to perform some startup operations.
   /// The registered service has to implement <see cref="IPluginStartup"/>.
   /// </summary>
   /// <typeparam name="TIPluginStartup">The plugin startup service Type.</typeparam>
   /// <exception cref="InvalidOperationException">Thrown if the plugin is not initialized.</exception>
   protected void RegisterPluginStartup<TIPluginStartup>()
      where TIPluginStartup : class, IPluginStartup
   {
      if (_services == null) throw new InvalidOperationException("Cannot register a startup service before the plugin is initialized.");
      _services.AddScoped<IPluginStartup, TIPluginStartup>();
   }

   /// <summary>
   /// Has to be implemented by the plugin and it is used to register the plugin services and declare its capabilities.
   /// </summary>
   /// <param name="services">The service collection where to register the services.</param>
   public abstract void RegisterServices(IServiceCollection services);

   /// <summary>
   /// Hooking point to register a custom AIdentity safety checker.
   /// see <see cref="IAIdentitySafetyChecker"/> for more info.
   /// Every plugin dealing with AIdentities should register its own valid AIdentity safety checker.
   /// Plugins that doesn't deal with AIdentities can do nothing.
   /// By default, no safety checker is registered.
   /// </summary>
   /// <param name="services">The service collection where to register the services.</param>
   public void RegisterAIdentitySafetyChecker<TAIdentitySafetyChecker>()
      where TAIdentitySafetyChecker : class, IAIdentitySafetyChecker
   {
      if (_services == null) throw new InvalidOperationException("Cannot register an AIdentitySafetyChecker before the plugin is initialized.");

      //by default, no safety checker is registered.
      if (typeof(TAIdentitySafetyChecker) == typeof(NoAIdentitySafetyChecker)) return;

      _services.AddScoped<TAIdentitySafetyChecker>();
      _services.AddScoped(sp => new AIdentitySafetyCheckerRegistration(_manifest.Signature, sp.GetRequiredService<TAIdentitySafetyChecker>()));
   }
}
