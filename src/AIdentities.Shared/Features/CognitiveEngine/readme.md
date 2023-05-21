## Cognitive Engine

This is where all the services and models that build up the cognitive engine (the brain!) of an AIdentity lies.
Everything that runs autonomously in this project is led by an AIdentity.

The Cognitive Engine represents the brain of a single AIdentity and is responsible to perform actions driven by previous events that happened under the supervision of such AIdentity.
Any skill could be implemented in such a way that the outcome depends not only from eventual parameters a skill action requires in order to be executed but also on AIdentity personality itself! (more on that later)

If we think about a job we want our AIdentity execute for us, we can imagine it as a finite (or indefinite in some cases) number of actions executed sequentially by a single AIdentity. This doesn't mean that a job has to be executed sequentially: if it can be splitted among different subtask, the execution could be branched and finally the results can be packed together and the flow can continue.

Also we aren't limiting the AIdentity to a single task: any action executed is independent from any other action the same AIdentity may be executing at the same time.

To hold the state of an executing task, a CognitiveContext is used. That's the place where temporary data created by each AIdentity skill are held and any skill can modify the context in any of its part: each state variable within the CognitiveContext isn't owned by a specific skill but can instead be modified by anyone that has access to the specific CognitiveContext. If you think like a human does, every action the human does share the state with any other action he's doing, it's kind of his volatile memory.

## AIdentity into a mission

When the user wants to have his AIdentities to help him to accomplish a goal, the user has to create a Mission.
A mission is composed by two main aspects: a goal and a set of constraints.

The goal is self explanatory: what the user wants to achieve. It can be anything and it's only limited by the tools capabilities.

The constraint is a more generic terms that contains several aspect:

1. Resource
2. Constraints
3. AIdentities

### 1 - Resource Constraints

There are two types of resource constraints: the ones generically available like the maximum time available to complete a mission and the constraints configurable within a specific Skill (see Skill Constraints section).

Some constraints may cause the mission to abort, others may just influence how it gets executed.

### 2 - Skill Constraints

The available skills depends on which skills the user has installed/implemented in the app. 
Once a skill is available in the system, the user can decide to automatically set it available to any AIdentity or decide to assign specifically a skill to a specific subset of AIdentities. A skill can even be implemented to have a custom setting page in the AIdentity that allow to configure differently each AIdentity with different parameters.

Think for example to an hypothetical "Paint Skill" that allows an AIdentity to generate an Image based on a user prompt... this would sounds familiar to you if you think about generative image models like DALL-E , Stable Diffusion, etc.
Now imagine setting a custom "drawing style" to a specific AIdentity (and/or a negative prompt for a Stable Diffusion skill), everytime you ask to that specific AIdentity to do something, the result will be affected by the AIdentity specific settings!

In the context of a mission, the user can then decide to use only a subset of his available skills (maybe because some skills uses paid service and he wants to control the cost, or maybe because some skills are redundant and some may work better than others in some specific scenario).

Also a skill can have resource constraints bound to the service they are using. For example a skill that generates text through OpenAI API may have a maximum number of spendable tokens or a maximum numbers of retries if its action fails.

### 3 - AIdentities Constraints

All available AIdentities can be potentially used to accomplish a mission but the user may decide to select just a subset for several reasons, some of which are already described in the Skill Constraints section above: a specific skill may use some parameters specified in the AIdentities itself, so based on the mission not all AIdentities may be suitable to perform an action. Since during the mission is the main AIdentity that chose which AIdentity to use to execute an action, it's important that the available pool of AIdentities fits well the mission requirements.

Is also possible, for each skill, to set a preferred AIdentity in charge of use such skill

>*Note*  
>An AIdentity could be bound to a specific service (e.g. a specific LLM connector like OpenAI API) so the result of the skill execution may differ and it's important to be aware of these details when setting constraints.



## MissionContext

In order to share some common data about the mission evolution, each AIdentity has access to a common context that works similar to CognitiveContext but it's used to share partial data during the execution of a mission.

MissionContext has to be thought as a common memory among different AIdentities working on the mission, no one has exclusive access to any state variable of the Mission Context.
