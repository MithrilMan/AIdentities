# AIdentities Project

Welcome to the AIdentities Project GitHub repository! This project aims to create an innovative and versatile application that focuses on the development and management of AI-generated identities. Your contribution to this project will help us push the boundaries of AI technology and create a product that is both useful and exciting for users.

![splash](media/README/splash.jpeg)
*temporary Image generated by Bing Image Creator*



# Goal

The goal of this project is to create a modular application that allows users, thinkers, builders, to build on top of that and extend the capability of the application to achieve whatever we want.

Imagination is the limit, AI is exponentially growing and every day lot of new papers are released, that push the boundaries further.

Proprietary and open source implementations are working recklessly and we can already build on top of them on our local pc.

I firmly believe that LLM models (and every kind of AI model can make use of our personal information) should be hosted on our own hardware, our private information should be confined next to us and not into some big corporation server. There are still cases when a remote model makes sense of course!

I'm not here to enforce anyone on my view, I'm instead trying to create something useful that people can use as they wish, this is why I wanted to invest time in designing a modular application that's more than some scripts glued together but a whole framework that developers can use to augment the software feature set that every one can use. *

###### *(Don't get me wrong, I'm a huge fan of those popular repositories that let you hack into the generative AI world and get your hand dirty, without them even this project probably would not have started, but usability and user experience aren't exactly their priority)



# Current Status

> **Note**  
> The project is currently in its very early stage, everything is subject to change, join the discord server to help shaping it!

Current implemented features:

- Core application and modular system (WIP)
- AIdentities creation
- Chat Plugin that allows you to have conversation with any AIdentity you created, by using one of the available connectors (actually OpenAI API and oobabooga TextGeneration are implemented)
- A personal AI agent to interact with in the app, work in progress, actually it allows you for example to change the theme of the app by asking to him directly what you'd like to have (prototype to investigate on Autonomous agents and task executors)



## Short Term Goals

I've a lot of ideas that I want to implement and with all the progress around this subject it's hard to keep the pace and have a priority, anyway here it is a list of some of the feature I'll release:

- TTS and STT to interact with AIdentities at another level
- Multi-AIdentities chat (involve multiple AIdentities in a discussion and each one, based on their personality, can intervene when they wish, contributing with their thoughts)
- Extend BrainButler (that's the AI companion that you can use to interact with application) capabilities like for example selecting the better AIdentities for a specific need, browse the web to look for informations, etc... (a playground to me to investigate on autonomous agents behaviors)
- Allow to link a specific model/API to an AIdentity
- Conversational Model Arena: pick an AIdentity and let it generate different replies to the same prompt, using different connectors/models and then optionally pick another AIdentity as the Judge, that can use a specific model (e.g. GPT4) to evaluate the various responses.
  Every output can be tweaked by the model parameters (temperature, top-p, etc..) so can be used even on the same model, to see how parameters impact on the output.
- Image Generation
  Allow an AIdentity to produce an image based on user input or custom input (e.g. just by the summary of the conversation).
  Each AIdentity will produce different output because the prompt will be mixed with the AIdentity traits (personality as general field, and then some specific ImageGeneration fields like the "style of drawing", etc...)

## Mid Term Goals

- Brainstorming zone: a derivation of the Multi AIdentities chat where you can set a topic/goal and let the AIdentities to discuss about it.
  You can for example create different personalities with different roles, each one with their experience and point of view and see how the discussion proceed toward a goal. I think that will be an interesting experiment.
- P2P Network for remote interaction with AIdentities 
  This is a big one and will allow people to join a P2P network where anyone can interact with public AIdentities of other people allowing them to preserve their prompt (because the AIdentity response will be generated remotely by the owner) and will allow to see AIdentity as a service, that could even develop in a market (e.g. by using 2nd layer blockchain solutions to work as an economic incentive)
- Autonomous Command Execution
  This is maybe the most intriguing part that lead to autonomous agents: having a library of functionalities (e.g. browse the web, export pdf, etc...) that can be executed autonomously, supervised by an AIdentity leader that may chose specific AIdentity run specific tasks. Blending the pure task execution to the AIdentity personality may lead to interesting results

# Try it out

This project has been developed by using Visual Studio 2022, it shouldn't be a problem run it on other IDEs, I can't just give support to it by myself.
If you are just interested to try the software and not on the development, You can still run it easily by running it on a docker instance (I suggest to do so in any case because it's always better to run unknown programs on a sandboxed environment...)

The docker file is in the src/AIdentities.UI folder but there is already a docker-compose file here in the root folder and if you are lucky and the port aren't already taken in your environment you can just run

Before running the container, you need to configure your `.env` file

Here you can find a `.env-example` file that you can use as reference, just copy it as `.env` and tweak based on your need. (that's the place where you'd want to set your OpenAI API key or any other sensitive information, other than change ports in case of conflicts with your environment)

Now you are ready to run

`docker-compose up --build`

and then access the web app to the address you specified in your .env HTTPS_PORT (by default https://localhost:5001/ ) or HTTP_PORT (by default http://localhost:5000 )
If your system doesn't have the port 5000 available you should edit the docker-compose.override.yml file and edit the port with one available (or you can go deeper and add an environment file and configure it externally, that's the preferred way to deal with this kind of apps).

If during the docker-compose build you encounter errors about self signed certificates and you don't want to mess with your system trying to find a solution, you can disable the 443 port (see .env-example) and run it on http port only.

{% note %}
**Note** You can map the volume of the container path `/app/packages/_storage_/`
{% endnote %}

At current state of development a proper guide doesn't exists yet, so be patient or help writing one 😊



# Community

Join the discord server to help shaping the software!

### Discord server: https://discord.gg/5KbTuGQseB