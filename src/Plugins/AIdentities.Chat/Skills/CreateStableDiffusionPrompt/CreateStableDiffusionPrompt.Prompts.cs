namespace AIdentities.Chat.Skills.CreateStableDiffusionPrompt;
public partial class CreateStableDiffusionPrompt
{
   const string PROMPT = """
      Here is a conversation history focused on defining a prompt to be used as input for an image generation tool called StableDiffusion.
      StableDiffusion works by taking as input a set of keywords that describe an image. Appending styles or descriptive tags can affect the overall generated image.
      For example, saying 'A portrait of' or 'A full figure of' affects how StableDiffusion will generate the image: in the first case it will be a portrait focused on the face of the subject while in the second case it will try to include the whole figure in the generated image.
      You can express yourself as you want as long as you understand the implication of what I've said. If the specification given to you about the IMAGE-CONTEXT isn't explicitly detailing a style, you can apply your own depending on what you think match wells with the IMAGE-CONTEXT and based on your PERSONALITY.
      You can mention artists that matches the style you want to apply to the image you are thinking about.

      Example of prompts:
      - A ghostly apparition drifting through a haunted mansion's grand ballroom, illuminated by flickering candlelight. Eerie, ethereal, highly detailed, digital painting, artstation, concept art, moody lighting. 
      - portait of a homer simpson archer shooting arrow at forest monster, front game card, drark, marvel comics, dark, intricate, highly detailed, smooth, digital illustration
      - pirate, concept art, deep focus, fantasy, intricate, highly detailed, digital painting, matte, sharp focus, illustration
      - red dead redemption 2, cinematic view, epic sky, detailed, concept art, low angle, high detail, warm lighting, volumetric, godrays, vivid, beautiful
      - a fantasy style portrait painting of rachel lane / alison brie hybrid in the style of francois boucher oil painting, rpg portrait
      - athena, greek goddess, claudia black, bronze greek armor, owl crown, d & d, fantasy, intricate, portrait, highly detailed, headshot, digital painting, concept art, sharp focus, illustration
      - closeup portrait shot of a large strong female biomechanic woman in a scenic scifi environment, intricate, elegant, highly detailed, centered, digital painting, concept art, smooth, sharp focus, warframe, illustration
      - ultra realistic illustration of steve urkle as the hulk, intricate, elegant, highly detailed, digital painting, concept art, smooth, sharp focus, illustration
      - portrait of beautiful happy young ana de armas, ethereal, realistic anime, detailed, clean lines, sharp lines, crisp lines, vibrant color scheme
      - A highly detailed and hyper realistic portrait of a gorgeous young ana de armas, lisa frank, butterflies, floral, sharp focus, intricate details, highly detailed

      The prompt should adhere to and include all of the following rules:
      - Prompt should always be written in English, regardless of the input language. Please provide the prompts in English.
      - Each prompt should consist of a description of the scene followed by modifiers divided by commas.
      - The modifiers should alter the mood, style, lighting, and other aspects of the scene.
      - Multiple modifiers can be used to provide more specific details.
      - After providing the prompt in English, also provide the Chinese translation for each prompt.
      - When generating prompts, reduce abstract psychological and emotional descriptions.

      This is your PERSONALITY: {{Personality}}
      This is the IMAGE-CONTEXT: {{ImageContext}}
      X: {{ X() }}
      I want you to write me a list of {{PromptsCount}} detailed prompts exactly about the IMAGE-CONTEXT:
      Prompt: 
      
      """;
}
