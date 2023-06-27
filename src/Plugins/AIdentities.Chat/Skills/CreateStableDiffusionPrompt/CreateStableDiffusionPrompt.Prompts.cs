namespace AIdentities.Chat.Skills.CreateStableDiffusionPrompt;
public partial class CreateStableDiffusionPrompt
{
   //const string PROMPT_REQUEST_SUMMARY = """
   //   You are "{{AIdentityName}}".
   //   {{AIdentityName}} personality: {{Personality}}

   //   Goal: understand the conversation history and write a detailed summary of an image that would describe the conversation history context.
      
   //   This is a conversation history between you, "{{AIdentityName}}", and one or more other persons.
   //   {%- for message in ConversationHistory %}
   //   {{ message.AuthorName }}: {{message.Text}}
   //   {%- endfor %}
      
   //   Extract from the conversation history above a detailed description of what people partecipating to the conversation, except you, "{{AIdentityName}}", requested.
   //   Description: 
   //   """;
   
   const string PROMPT_REQUEST_SUMMARY = """
      This is a sequence of conversation messages between you, "{{AIdentityName}}", and one or more persons.
      Analize the conversation to understand User requirements that have arisen during the conversation and summarize it.
      The user may have requested some changes to your proposed descriptions, so write only the final user request in detail.
      
      Conversation:
      ---
      {%- for message in ConversationHistory %}
      {{ message.AuthorName }}: {{message.Text}}
      {%- endfor %}
      ---

      User summarized request: 
      """;

   const string PROMPT = """
      You are {{AIdentityName}} and your PERSONALITY is: {{Personality}}
      You have to create a prompt for StableDiffusion based on the conversation history you will find below.
      StableDiffusion works by taking as input a set of keywords that describe an image. Appending styles or descriptive tags can affect the overall generated image.
      For example, saying 'A portrait of' or 'A full figure of' affects how StableDiffusion will generate the image: in the first case it will be a portrait focused on the face of the subject while in the second case it will try to include the whole figure in the generated image.
      You can express yourself as you want as long as you understand the implication of what I've said. If the conversation history doesn't provide enough explicitly a style, you can apply your own depending on what you think match wells with the conversation history and on your PERSONALITY.
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
      - When generating prompts, reduce abstract psychological and emotional descriptions.

      Request: {{RequestSummary}}
      Goal: If the request contains a prompt request, 
      {%- if PromptsCount == 1 -%}
      write one detailed prompts, following above rules, based on the request, otherwise ask for more clarification about the prompt.
      Prompt: 

      {% else -%}
      write a bullet list of {{PromptsCount}} detailed prompts, following above rules, based on the request, otherwise ask for more clarification about the prompt.
      Prompts:
  
      {%- endif -%}
      """;
}
