﻿namespace AIdentities.Shared.Plugins.Connectors.Completion;

/// <summary>
/// A common conversational request.
/// Properties aren't guaranteed to be supported by all connectors.
/// The numerical values within the documentation of this interface are indicative, they depend on the model and the connector.
/// To see what values are supported by a connector, check the documentation of the connector.
/// </summary>
public interface ICompletionRequest
{
   /// <summary>
   /// The ID of the model to use.
   /// </summary>
   string? ModelId { get; init; }

   /// <summary>
   /// The messages to generate chat completions for.
   /// </summary>
   string Prompt { get; init; }

   /// <summary>
   /// The suffix that comes after a completion of inserted text.
   /// Not all connectors support this.
   /// </summary>
   string? Suffix { get; init; }

   /// <summary>
   /// How many chat completion choices to generate for each prompt.
   /// </summary>
   int? CompletionResults { get; init; }

   /// <summary>
   /// The optional sequences to instruct the model to stop generating further tokens once the sequence is encountered.
   /// </summary>
   IList<string>? StopSequences { get; init; }

   /// <summary>
   /// The optional user ID to associate with this request.
   /// </summary>
   string? UserId { get; init; }

   /// <summary>
   /// Determines how creative the model should be.
   /// A higher temperature allows the model to use words with lower probabilities, resulting in more diverse output.
   /// A temperature of 0 makes the model deterministic and limits it to the word with the highest probability.
   /// The generative model generates output based on probabilities of words that can follow a given phrase.
   /// In layman terms, the higher the temperature, the more creative the model is, the lower the temperature, the more logical the model is.
   /// Value from 0.1 to 2.0.
   /// </summary>
   float? Temperature { get; init; }

   /// <summary>
   /// Repetition penalty is used to prevent the model from repeating the same words over and over again.
   /// Adjusting this value helps to control the repetition of the same words in the generated text, try to increase this value if the model is repeating the same words too much.
   /// Increasing the value too much may result in the model not repeating the same words at all, which may result in the model not making sense and breaking the chat format.
   /// The standard value for chat is approximately 1.0 - 1.05 but may vary depending on the model.
   /// </summary>
   float? RepetitionPenality { get; init; }

   /// <summary>
   /// The range of influence of Repetition penalty in tokens.
   /// The repetition penalty is applied to the last N tokens, where N is the value of this parameter.
   /// </summary>

   float? RepetitionPenalityRange { get; init; }

   /// <summary>
   /// Top-p sampling (aka nucleus sampling) tells the model to pick the next token from the top tokens based on the sum of their probabilities.
   /// For example, if set to 0.15, the model will only pick from the top tokens whose probabilities add up to 15%.
   /// Top-p is often used to exclude outputs with lower probabilities, e.g. if you set it to 0.75, the model will exclude the bottom 25% of probable tokens.
   /// Set to 0 to perform a Greedy Decoding.
   /// A lower value will result in more predictable and repetitive text, while setting a higher value will result in more diverse but potentially more incoherent or nonsensical text.
   /// range is 0.0 to 1.0.
   /// </summary>
   float? TopPSamplings { get; init; }

   /// <summary>
   /// Top-k sampling tells the model to pick the next token from the top 'k' tokens in its list, sorted by probability.
   /// For example, if set to 3, the model will only pick from the top 3 options.
   /// Top-k is often used to generate more consistent results.
   /// Set to 0 to disable.
   /// Set it to 1 to only use the first token (the most likely word).
   /// </summary>
   float? TopKSamplings { get; init; }

   /// <summary>
   /// Top-a sampling comes from BlinDL/RWKV-LM <see cref="https://github.com/BlinkDL/RWKV-LM"/>.
   /// It converts logits into probabilities using the softmax function and sets the logits of tokens with a 
   /// probability less than a certain value (<see cref="TopASamplings"/>) to negative infinity.
   /// Technically Top-a sampling works this way:
   /// - finds the max probability p_max after softmax
   /// - remove all entries whose probability is less than top_a * p_max^2.
   /// This sampling methods reduces randomness when the model is confident about the next token, but has little effect on creativity.
   /// </summary>
   float? TopASamplings { get; init; }

   /// <summary>
   /// Sets the probability threshold (probability mass) for the typical sampling method.
   /// The Typical Sampling technique limits the sampling distribution to only those words with negative log-probability within 
   /// a certain absolute range from the conditional entropy of the model at that time step.
   /// In peak distributions this can result in a very small number of tokens being sampled, which can lead to repetitive text.
   /// In smoother distributions this can result in a broader range of tokens being sampled, which can lead to more creative text.
   /// </summary>
   float? TypicalSampling { get; init; }

   /// <summary>
   /// Tail Free Sampling aims to remove low probability tokens without compromising the creativity of the generated text.
   /// It identifies a tail of undesirable tokens based on the probability distribution and removes them based on a chosen threshold.
   /// This sampling method works well on longer pieces of text and can be used in conjunction with other sampling methods.
   /// A value of 1 disabled this sampling method.
   /// </summary>
   float? TailFreeSampling { get; init; }

   /// <summary>
   /// The maximum amount of tokens that a AI will generate to respond.
   /// Based on the specific tokenization scheme used by the Model, an english word can be made of an average 
   /// of 3 tokens. In ChatGPT 1 token is approximatelly 4 characters.
   /// The larger the parameter value, the longer the generation time will be.
   /// </summary>
   int? MaxGeneratedTokens { get; init; }

   /// <summary>
   /// How much will the AI remember.
   /// Context size also affects the speed of generation.
   /// Note that the maximum available size depends on the model so this parameter
   /// shouldn't exceed the model context size in order to avoid errors.
   /// </summary>
   int? ContextSize { get; init; }

   /// <summary>
   /// Modify the likelihood of specified tokens appearing in the completion.
   /// Accepts a json object that maps tokens (specified by their token ID in the tokenizer) to an associated bias value from -100 to 100.
   /// Mathematically, the bias is added to the logits generated by the model prior to sampling.
   /// The exact effect will vary per model, but values between -1 and 1 should decrease or increase likelihood of selection;
   /// values like -100 or 100 should result in a ban or exclusive selection of the relevant token. 
   /// </summary>
   public object? LogitBias { get; init; }
}
