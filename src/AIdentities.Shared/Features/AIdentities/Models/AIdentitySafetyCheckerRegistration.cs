using AIdentities.Shared.Features.AIdentities.Services;
using AIdentities.Shared.Plugins;

namespace AIdentities.Shared.Features.AIdentities.Models;

/// <summary>
/// This class is used to register a AIdentity safety checker.
/// For details about what's a AIdentity safety checker, see <see cref="IAIdentitySafetyChecker"/>.
/// </summary>
/// <param name="PluginSignature">The plugin signature.</param>
/// <param name="SafetyCheckerType">The type of the AIdentity safety checker.</param>
public record AIdentitySafetyCheckerRegistration(IPluginSignature PluginSignature, IAIdentitySafetyChecker SafetyChecker);
