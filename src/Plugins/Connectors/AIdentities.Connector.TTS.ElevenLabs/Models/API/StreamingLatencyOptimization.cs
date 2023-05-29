namespace AIdentities.Connector.TTS.ElevenLabs.Models.API;

/// <summary>
/// Streaming latency optimization options.
/// </summary>
public enum StreamingLatencyOptimization
{
   /// <summary>
   /// No latency optimization
   /// </summary>
   Default = 0,

   /// <summary>
   /// Normal latency optimization (about 50% of possible latency improvement of option 3)
   /// </summary>
   Normal = 1,

   /// <summary>
   /// Strong latency optimization (about 75% of possible latency improvement of option 3)
   /// </summary>
   Strong = 2,

   /// <summary>
   /// Max latency optimizations, but also with text normalizer turned off for even more 
   /// latency savings (best latency, but can mispronounce eg numbers and dates).
   /// </summary>
   Max = 3,

   /// <summary>
   /// Max latency optimizations, but also with text normalizer turned off for even more 
   /// latency savings (best latency, but can mispronounce eg numbers and dates).
   /// </summary>
   MaxPlus = 4
}
