# Example of Code

> **Language:** **English** | [Русский](_Docs/README-ru.md)

## Content

- [About](#about)
- [UML Diagram](#uml-diagram)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)


## About

This project is a code sample, the main purpose of which is to demonstrate an example architecture.

This is an MVP of a typical mobile game scenario - limited-time event. It implements a store with three offers available for purchase. The event configuration is configured via a JSON file. 

Only the most required simple **UI** has been implemented: HUD resources, a button to open the store, and the store itself. Also Save-Load System has been implemented and custom tool for deleting player progress in Unity Editor

More details on the architecture are below, in the **Architecture** section.


## UML Diagram

### **The UML diagram is available at [link](https://miro.com/app/board/uXjVHBWhegI=/) on the Miro board.**

The diagram is not posted here due to technical limitations of the JPG/PNG formats, as well as because the interactive version is much easier to navigate and allows for a detailed study of the relationships between individual system components.


## Architecture
1. [Basic principles and patterns](#1-basic-principles-and-patterns)
2. [Assembly Definition Structure](#2-assembly-definition-structure)
3. [Dependency Injection](#3-dependency-injection)
4. [Entry Point](#4-entry-point)
5. [Modular Architecture](#5-modular-architecture)
6. [Game State Machine](#6-game-state-machine)
7. [MVP (Model-View-Presenter)](#7-mvp-model-view-presenter)
9. [Save-Load System](#8-save-load-system)
8. [Static Data](#9-static-data)
10. [Triple Offer Event](#1--triple-offer-event)
11. [Addressables](#11-addressables)
12. [Asynchronous Programming](#12-asynchronous-programming)
13. [Extensibility](#13-extensibility)

### 1. Basic principles and patterns

[_Back to Architecture content_](#architecture)

Before describing individual systems, it's worth stating the engineering principles that shaped every decision below — they explain why the architecture looks the way it does, not just what it contains.

**How SOLID principles are practically applied in the project:**

* **Single Responsibility Principle** — every class has exactly one reason to change. SaveLoadService only persists data; it doesn't know about purchases. `TripleOfferEvent` only orchestrates offer lifecycle; it doesn't know how a timer counts down. This is why the codebase has many small services instead of a few large ones.
* **Open/Closed Principle** — new functionality (a new event type, a new offer, a new save backend) is added by introducing new classes behind existing interfaces, not by editing working code.
* **Liskov Substitution** — any implementation of IService, `IState`, ISaveLoadService, etc. can replace another without breaking the consumer, because consumers depend only on the contract.
* **Interface Segregation** — each service interface exposes only what its consumers actually need (ISaveLoadService doesn't leak `IAPService` concerns, for example).
* **Dependency Inversion Principle** — high-level logic (state machine, presenters, events) depends on abstractions (IService-derived interfaces), and concrete implementations are injected, never constructed in place. This is the foundation the whole DI section below builds on.

**Beyond SOLID, the following practices were followed consistently:**

* **Naming as documentation**. Classes are named after their role, not their implementation (ISaveLoadService, not JsonFileManager). This keeps the name stable even if the implementation behind it changes — which matters a lot for the "extensibility" goals discussed later.
* **Composition over inheritance**. **Services** are composed via constructor injection rather than built through deep inheritance trees. The only inheritance used is shallow and interface-based (`IState`, IService), which avoids the fragile base class problem.
* **Dependency inversion at the design level, not just SOLID-on-paper**. Every cross-service dependency in the project is an interface reference, never a concrete class reference. This is what actually enables Zenject to do its job.
* **Defensive programming**. Methods validate their inputs and preconditions before acting — see OfferDataService.IsConfigValid(), IsOffersListValid(), IsEventValid() in `TripleOfferEvent`. Invalid states are detected early and handled explicitly (return empty/false), rather than allowed to propagate into null reference exceptions further down the call stack.
* **Fail-fast, with graceful degradation for content data**. Config loading fails loudly via logged errors immediately at the point of failure (Debug.LogError in OfferDataService), rather than silently swallowing an exception three layers later. At the same time, a missing or malformed event config doesn't crash the game — it degrades to "no offers available," because content data errors should never be allowed to take down a live game.
* **Interfaces first**. Every service is designed interface-first: the contract (ISaveLoadService) is decided before the implementation (SaveLoadService) is written. This is a deliberate habit, not an afterthought — it forces the question "what does this service need to expose to others?" before any implementation detail leaks into the API.
* **Low coupling, high cohesion**. Each class is internally focused (high cohesion — e.g., OfferDataService only deals with offer data) and depends on the minimum surface area of other classes (low coupling — via interfaces, not concrete types). This combination is what makes individual pieces of the project replaceable and testable in isolation.

All of these principles form the foundation of this project's architecture. They defined the assembly structure, the use of Dependency Injection, the service architecture, the Game State Machine, and the MVP. Each subsequent section demonstrates the practical application of these engineering principles in solving specific architectural problems.

### 2. Assembly Definition Structure

[_Back to Architecture content_](#architecture)

The project is split into separate Assembly Definitions (`.asmdef`): **Core**, **Services**, **Infrastructure**, **UI** **Logic**, **Events**, **Editor**.

![img_2.png](_Docs/img_2.png)

_The assembly dependency graph is also shown on a UML-diagram, where it can be examined in more detail [[link to Miro map](https://miro.com/app/board/uXjVHBWhegI=/)]_

**Why split into assemblies at all?**

In a single default assembly, Unity recompiles the entire codebase on every single script change, no matter how small. As a project grows, this becomes one of the biggest sources of wasted iteration time. Splitting code into Assembly Definitions means Unity only recompiles the assembly that changed and whatever depends on it — assemblies with no dependency on the changed code are left untouched.

Assembly splitting was chosen because it provides:

* Reduced compilation time — changing a **UI** script doesn't force a recompile of **Core** or **Services**.
* Explicit, enforced dependencies — an assembly can only reference types from assemblies it explicitly references in its `.asmdef`. This turns "please don't reach into that layer" from a convention into a compiler-enforced rule. If **Services** tries to reference something from **UI** **Logic**, it simply won't compile — the architecture boundary is real, not just documented.
* Prevention of circular dependencies — Unity's assembly system rejects circular references outright. This forces a strict, acyclic dependency graph and surfaces design problems (e.g., "why does a low-level service need something from a high-level layer?") at compile time instead of as a runtime surprise.
* Modularity — each assembly represents a coherent layer of the application (data contracts, runtime services, platform/bootstrapping concerns, presentation, event-specific logic), making it easy to reason about what depends on what.
* Scalability — as the project grows, new features land in the assembly that matches their responsibility, instead of all code accumulating in one undifferentiated blob.

Why does the **Editor** assembly stand apart?
**Editor** is intentionally isolated: it depends on nothing else in the project, and nothing else depends on it. This exists because **Editor**-only code (custom inspectors, asset validation tools, build helpers) uses UnityEditor APIs that don't exist in player builds. If editor code were not separated into its own assembly restricted to the **Editor** platform, it would either fail to compile in a build or — worse — risk being silently included in a shipped build. Isolating it guarantees editor tooling can never leak into runtime code, and that runtime code can never accidentally depend on editor-only functionality.

### 3. Dependency Injection

[_Back to Architecture content_](#architecture)

This is the foundational architectural decision the rest of the project is built on top of, so it deserves the most thorough justification.

#### 3.1 Why Dependency Injection at all?

The alternative approaches available in Unity are typically **Singleton** and **Service Locator**. Both were considered and rejected:

**Why not **Singleton**?**

A singleton (`SaveLoadService.Instance`) creates a globally accessible mutable instance that any class can reach into from anywhere. This:

* Hides dependencies — a class using `IAPService`.Instance internally doesn't declare that dependency anywhere visible; you have to read the method body to discover it.
* Makes unit testing very difficult — you cannot substitute a fake/mock instance for a test, because the dependency isn't injected, it's hard-coded to a static accessor.
* Creates implicit global state and lifecycle problems — who initializes it, when, and in what order, becomes a project-wide implicit contract instead of an explicit one.
* Tends to encourage tight coupling, since it's too easy to reach for Instance from anywhere, which erodes the boundaries the assembly split is trying to enforce.

**Why not **Service Locator**?**

A **Service Locator** (e.g. a static `ServiceLocator.Get<ISaveLoadService>()`) solves the "single global instance" problem of **Singleton** and does allow interface-based access, which is an improvement. However, it still:

* Hides dependencies inside method bodies rather than declaring them in the constructor signature — you still have to read the implementation to know what a class actually needs.
* Defers missing-dependency errors to runtime (Get<T>() throws or returns null when something isn't registered), rather than catching them at composition time.
* Makes a class's true dependency list invisible from its public API, which directly hurts testability and code review — you can't tell what a class needs without reading its internals.

#### 3.2 Why Zenject specifically?

It's important to clarify that choosing **Zenject** doesn't mean it's the best DI container for Unity.

Zenject was chosen because it's one of the most mature and widely used DI frameworks in the Unity ecosystem. It offers a rich set of features proven in numerous commercial projects: **Installers**, **Factories**, **Memory Pools**, **Signals**, and **lifecycle interfaces** such as `IInitializable`. Zenject also has extensive documentation and a large professional community.

**Why not a hand-written container?**

A custom DI container is a viable learning exercise, but for production-oriented work it means re-solving problems (lifetime scoping, circular dependency detection, factory generation, binding validation) that mature frameworks have already solved and hardened through years of real-world use. Using an established framework means more engineering time goes into actual game features rather than re-implementing infrastructure, and it means any other developer joining the project already has documentation, community knowledge, and tooling to lean on.

**Why not VContainer?**

This isn't a case of "Zenject is better" — that wouldn't be technically accurate. VContainer is a legitimate, lighter-weight, higher-performance alternative (no reflection at resolve time, smaller allocation footprint), and is a reasonable choice for performance-critical or mobile-constrained projects. 

However, the choice fell on Zenject due to its greater popularity. In fact, the difference in code implementation between VContainer and Zenject is minor.

#### 3.3 What the container actually gives the project

Centralized composition — all bindings (interface → implementation) are declared in one place (the installer), not scattered across the codebase.
Lifetime management — the container manages object lifetimes (singleton-per-container vs transient) without any class needing to manage its own lifecycle.
Effortless substitution — swapping `IAPService` for a more complex backend-driven implementation later is a one-line change in the installer binding, with zero changes to any class that consumes `IIAPService`.
Lifecycle hooks — interfaces like IInitializable give a clean, container-managed entry point for startup logic (see `GameBootstrapper` below), instead of relying on Unity's Awake/Start ordering, which is notoriously fragile across multiple objects.

#### 3.4 Impact on testability

Because every dependency is an interface received through the constructor, any service can be unit-tested in isolation by injecting hand-written fakes or mocking framework substitutes for its dependencies — no Unity runtime, no scene, no container required for the test itself. For example, `TripleOfferEvent` could be tested by injecting a fake `IOfferDataService` that returns a fixed list of offers and a fake `ISaveLoadService` with predetermined purchase data, fully exercising its filtering and validity logic without touching the filesystem or Addressables. This is simply not possible with **Singleton** or **Service Locator** approaches, where the dependency is either a hard-coded static reference or a runtime lookup that's awkward to redirect in a test context.
### 4. Entry Point

[_Back to Architecture content_](#architecture)

The project has a single, clearly controlled entry point: `GameBootstrapper`, which implements Zenject's `IInitializable` interface.

**Why a single entry point?**

A game has many systems that need to start in a specific, predictable order — services need to exist before the state machine can use them, saved progress needs to load before the first gameplay state runs, and so on. Without a single defined entry point, startup logic tends to scatter across multiple `MonoBehaviour.Awake()/Start()` calls whose relative order depends on Unity's script execution order settings — a notoriously fragile and easy-to-break mechanism, especially as a project grows. A single bootstrapper makes startup an explicit, readable, top-to-bottom sequence instead of an implicit one governed by Unity's internal ordering rules.

**Why `IInitializable` rather than `Awake/Start`?**

`IInitializable` is a Zenject lifecycle interface that guarantees Initialize() is called only after the container has finished resolving and injecting all bindings. This removes an entire category of bugs where a MonoBehaviour.Awake() tries to use an injected dependency before Zenject has actually had the chance to inject it. It ties startup timing to the DI container's lifecycle rather than to Unity's component lifecycle, which is exactly the guarantee a bootstrapper needs: "run this only once everything it depends on is ready."
Why does `GameBootstrapper` only handle startup, and nothing else?

This is Single Responsibility applied directly to the most important class in the project. `GameBootstrapper`'s only job is to kick off the very first transition (typically into the initial state of the Game State Machine, described below) — it does not contain gameplay logic, **UI** logic, or service logic itself. If startup behavior needed to change (e.g., add a splash screen, add a remote-config fetch before anything else runs), that change happens in one small, well-understood class, instead of being buried inside a MonoBehaviour that's also responsible for half a dozen other things. This keeps the entry point legible — anyone new to the project can open `GameBootstrapper`, read a handful of lines, and understand exactly what happens first.
### 5. Modular Architecture

[_Back to Architecture content_](#architecture)

The project's logic is decomposed into independent services rather than a small number of large manager classes: `SaveLoadService`, `OfferDataService`, `IAPService`, the Addressable asset provider, and others.

**Why decompose into services at all?**

Each service represents one cohesive capability of the application — persistence, offer/config data, purchases, asset loading — and nothing else. This is a direct, practical application of the Single Responsibility Principle at the system level (rather than just the class level): instead of one `GameManager` that knows about saving, purchasing, loading assets, and configuring events all at once, each of those concerns lives in its own service with its own narrow interface.

**Why do services interact through interfaces (`IAPService`, ISaveLoadService, `IOfferDataService`) rather than concrete classes?**

This is the Dependency Inversion Principle in practice: a consumer like `TripleOfferEvent` depends on the abstraction ISaveLoadService, not on the concrete SaveLoadService class. The practical payoff is exactly what's described in the IAP discussion later — the concrete implementation behind an interface can be swapped without touching any of its consumers, because consumers were never coupled to the implementation in the first place.

**SOLID principles most directly visible at this level:**

* **Single Responsibility** — one service, one capability.
* **Dependency Inversion** — services are consumed via interface, never via concrete type.
* **Open/Closed** — new services (or new implementations of existing service interfaces) extend the system without modifying the classes that consume them.
* **Interface Segregation** — each service interface exposes only the operations relevant to its own capability, so consumers are never forced to depend on methods they don't use.
### 6. Game State Machine

[_Back to Architecture content_](#architecture)

The application's lifecycle is controlled by a finite state machine (FSM) rather than a single monolithic manager.

#### 6.1 Why a state machine?

A game's lifecycle naturally moves through discrete phases — loading progress, loading a level, running the main hub — and each phase has its own distinct setup, behavior, and exit conditions. Modeling this explicitly as a state machine, rather than as a tangle of boolean flags and if-checks inside one large class, gives several concrete advantages:

* **Explicit, controlled transitions**. Moving from one phase to another is a deliberate, traceable action (Enter()/Exit() on a state), not an implicit side effect of some flag flipping somewhere in a large class.
* **Each state is self-contained**. A state knows how to set itself up and how to clean itself up, and nothing outside the state machine needs to know the internal details of how a particular phase works.
* **No "1000-line `GameManager`**." This is, deliberately, the single biggest practical win of this approach. Instead of one class accumulating loading logic, level-transition logic, and hub logic all together — becoming progressively harder to safely modify as it grows — the application's lifecycle is split into small, independent, single-purpose classes. This is exactly the kind of decomposition that becomes critical for a codebase's long-term health as a project scales past a small sample.
* **Each state has exactly one phase of responsibility**, which is the Single Responsibility Principle applied to application lifecycle management rather than to an individual class's internal methods.

#### 6.2 Specific conditions that exist in the project

`LoadProgressState`

Loads the player's saved progress from `SaveLoadService` and preloads Addressable assets needed early in the application's life, ahead of when they're actually needed. Front-loading these preloads here — rather than loading them lazily later — reduces visible loading time at points further into the experience where a delay would be more disruptive to the player (e.g., right as the hub appears).

`LoadLevelState`

Handles the actual loading of the level/scene and the visual presentation of that loading — i.e., the loading curtain or transition screen that hides scene-loading work in progress from the player.

**Why are `LoadProgressState` and `LoadLevelState` separate states, rather than one combined "loading" state?**

This is Single Responsibility again, applied at exactly the granularity where it matters most: these two states do genuinely different kinds of work. `LoadProgressState` is about data — pulling save data and pre-warming the asset cache — while **LoadLevelState** is about presentation — the scene transition and the player-facing loading visuals. Keeping them separate means:

* Each state can be modified independently — changing how the loading curtain looks or behaves never risks touching save/progress logic, and vice versa.
* Each state can be tested or reasoned about without needing to understand the other's concerns.
* The order between them is explicit and can be changed or extended (e.g., inserting a remote-config fetch between the two) without restructuring either state internally.

`HubState`

The terminal state in the current flow — represents the main gameplay loop and the primary UI surface the player interacts with (HUD, store button, store itself).

#### 6.3 Why do all states implement `IState`?

At the state-machine level specifically: a common `IState` abstraction means the state machine driver itself only needs to know about `Enter()/Exit()` (or equivalent lifecycle members) — it never needs to know what kind of state it's currently running. This is what allows new states to be added later (e.g., a `PauseState`, a `RewardState`) without the state machine driver itself ever needing to change.

### 7. MVP (Model-View-Presenter)

[_Back to Architecture content_](#architecture)

The UI layer follows the MVP pattern from the MV* family — specifically a **Passive View** variant of MVP, which is generally considered the strongest and most testable form of MVP.

#### 7.1 Why MVP, and not MVC or MVVM?

**Why not MVC?**

In classic MVC, the View can read directly from the Model, and the Controller's role is mostly to translate input into Model updates — the View itself still carries some responsibility for rendering based on Model state it observes directly. In a Unity context this tends to blur the boundary between "Unity-specific rendering code" and "application logic," because the View ends up needing to know about Model shapes directly. MVP was preferred specifically to avoid this: the View here is deliberately passive and has no direct relationship with the Model at all.

**Why not MVVM?**

MVVM relies on a binding layer (the ViewModel exposes observable properties that the View binds to automatically) — which is a natural fit for frameworks with built-in data-binding, but Unity's **UI** system (uG**UI**, used here) has no first-class declarative data-binding mechanism. Implementing MVVM properly in Unity means building a custom binding layer from scratch, which is extra infrastructure this project's scope doesn't need. MVP achieves the same separation of concerns — **UI** logic isolated from rendering — through plain method calls and C# events instead, which uG**UI** already supports natively without extra machinery.

**Why Passive View specifically, rather than classic MVP?**

In classic MVP, the View can still contain some logic and may talk to the Model in limited ways. In a Passive View, the View is reduced to the absolute minimum: it exposes simple methods/properties (e.g., "set this text," "set this button's interactable state") and raises simple input events (e.g., "button clicked"), and contains zero application logic. All decision-making — what to show, when, and why — lives entirely in the Presenter.

#### 7.2 Mapping the pattern onto this project

* **Model** — the data: `PlayerProgress` (and its sub-objects, like `PlayerResources` and `PurchaseData`) and the Triple Offer Event's data (offers, remaining time), depending on which service/domain object a given Presenter is concerned with.
* **View** — the Unity-side components (HUD resource display, the store button with its countdown timer, the store panel itself with its three offer slots). Each View exposes only dumb setters (update displayed amount, update timer text, enable/disable the store button) and forwards raw input (button click) upward — it makes no decisions about what those values should be.
* **Presenter** — owns the actual logic: it reads from the Model/services (`PlayerProgress`, `TripleOfferEvent`, `IIAPService`), decides what the View should currently display, and calls the View's simple setters. It also handles the input events the View raises (e.g., "purchase button clicked" call `IIAPService`.MakePurchase).

**Why is it important that the Presenter knows nothing about Unity **UI** types directly involved in rendering, and the View knows nothing about Unity domain types?**

This boundary is what actually makes the pattern valuable rather than ceremonial. Because the Presenter only calls a small, abstract View interface (not concrete `Text`/`Image`/`Button` components), the Presenter's decision logic — "should the purchase button be interactable," "what value to show after a resource changes" — can be exercised and tested without instantiating any Unity **UI** objects at all. And because the View never reaches into `PlayerProgress` or any service, the visual implementation (how a number is animated, what a "purchased" state looks like) can change completely without ever touching the logic that decides when that state should occur.

#### 7.3 Walking through the concrete example

The store has three offers. Each offer carries a resource type (gems, coins, iron), an amount, and a price. Tapping the store button opens the store; tapping an individual offer purchases it, adds the resource to the player's resources, and triggers a save. Once all three offers are purchased, the store button itself becomes visually disabled (faded).

This last detail — the store button fading once all offers are purchased — is a clean illustration of why MVP plus Observer (next subsection) fit together so well: the button's Presenter doesn't poll "are all offers purchased yet?" every frame. It reacts to an event fired exactly once, at the moment the last offer is bought.

#### 7.4 MVP naturally produces an Observer relationship

Because the Presenter subscribes to change events on the Model (`PlayerProgress`'s data-changed events, and `TripleOfferEvent`.`OnAllOffersPurchased`), the resulting relationship is a genuine Observer pattern. When `PlayerProgress` changes (e.g., a resource amount updates after a purchase), an event fires; the relevant Presenter, having subscribed to it, receives the notification and updates its View accordingly. This means the **UI** doesn't need to be polled or manually refreshed from outside — it reacts automatically and only when something actually changed, which is both more efficient and removes an entire category of "forgot to refresh this **UI** element" bugs.

### 8. Save-Load System

[_Back to Architecture content_](#architecture)

#### 8.1 A dedicated service instead of saves and loads scattered throughout the code.

Persistence is a cross-cutting concern that many parts of the application need (player resources, purchase history, and potentially more state later), but none of those consumers should need to know how or where data is stored. Centralizing this into SaveLoadService, exposed via ISaveLoadService, means every consumer just asks for `PlayerProgress` and trusts that it's correctly loaded/persisted — the file path, the serialization format, and the write strategy are all encapsulated entirely inside one service.

#### 8.2 JSON and Newtonsoft.Json.

JSON was chosen because it's human-readable (useful during development for inspecting/editing save files directly) and is a natural fit for serializing a graph of plain C# data objects like `PlayerProgress`. 

**Newtonsoft.Json** specifically was chosen over Unity's built-in **JsonUtility** because **JsonUtility** has real limitations that matter here — it doesn't support `Dictionary<TKey,TValue>` serialization, doesn't handle polymorphic types or interface-typed fields well, and has limited support for serialization callbacks. Newtonsoft.Json supports all of this robustly, including the kind of richer object graphs (`PurchaseData`, `PlayerResources` as nested objects with their own change events) that this project's save data actually has.

#### 8.3 Saving/loading are made asynchronous, but why?

`SaveLoadService` **is asynchronous because file I/O can take real time, even for a small file**. Disk writes are not guaranteed to be instantaneous, and they can occasionally stall, especially on lower-end mobile storage or under I/O contention. Performing `File.WriteAllTextAsync` (and reading) asynchronously means the main thread — and therefore the rendering/input loop — is never blocked waiting on disk access. This matters more than it might first appear here, because saves in this project happen frequently and automatically, not just at a few deliberate checkpoints — see 8.4 below. A blocking synchronous save firing on every single resource change would risk visible micro-stutters; an async one does not.

#### 8.4 Autosave via events.

`SaveLoadService` subscribes to `PlayerProgress.PurchaseData.OnPurchaseData`Changed and `PlayerProgress.PlayerResources.OnPlayerResourcesDataChanged`, and triggers a save whenever either fires. This solution is better than explicitly calling `SaveProgress()` from every place in the codebase, for several reasons:

* **Correctness by construction**. If saving relied on every call site remembering to manually invoke `SaveProgress()` afterward, it would only take one new feature (a new place that grants resources, say) forgetting to call it for the save system to silently fall out of sync with actual game state. Event-driven autosave makes this impossible by design — any mutation that fires the change event triggers a save.
* **Decoupling responsibility correctly**. The component that causes a data change (e.g., `IAPService`, after a purchase) doesn't need to know or care that a save should happen afterward — it just mutates the data and raises its own domain event ("purchase data changed"). Whether or not that triggers a save is `SaveLoadService's` business, not the mutator's. This keeps `IAPService` from being coupled to persistence concerns it shouldn't need to know about.

This is the same Observer relationship discussed under MVP, applied at the persistence layer instead of the **UI** layer — the same underlying mechanism naturally serves two very different purposes.

#### 8.5 `SaveLoadService` know nothing about **UI**.

`SaveLoadService` exposes data and a "progress loaded" event; it has no reference to any View, Presenter, or UI type, and no reason to. This follows directly from Dependency Inversion and the assembly boundaries described earlier: the **Services** assembly cannot reference UI Logic even if someone wanted it to. Persistence and presentation are different concerns that change for different reasons, and coupling them would mean a UI redesign could risk breaking save logic, or vice versa — exactly the kind of accidental coupling the whole architecture is structured to prevent.

### 9. Static Data

[_Back to Architecture content_](#architecture)

#### 9.1 Static data is separated from runtime data.

There are two genuinely different kinds of "offer" in this system, and conflating them would be a mistake:

* `OfferConfig` / `TripleOfferConfig` — the raw, designer-authored content data: what offers exist, what they cost, when the event starts and ends. This is data that a designer or LiveOps person edits, not something the running game logic should mutate.
* `Offer` — the runtime model actually used by gameplay/UI code, which includes things the config alone doesn't carry (a loaded **Sprite**, for instance) and represents the current, in-memory state of an offer the player can interact with. **To convert `OfferConfig` into `Offer`, a special service is provided: `OfferDataService`.** _[(For more details, see section 9.3.)](#93-offerdataservice-is-a-mapper-not-just-a-config-reader)_.

Keeping these as two distinct types, rather than one shared class, means the config format can evolve (add a new field, restructure the JSON) without ever risking a change to how runtime code consumes `Offer`, and vice versa — runtime-only concerns (like a loaded **Sprite** reference) never need to pollute the serialized config schema.

#### 9.2 Why JSON for configuration?

The event configuration is gamedesigners/developers that benefits from being **human-readable and easy to edit or generate externally** — including, in a real production context, being generated server-side and pushed to update a live event's offers, pricing, or timing without an app update. JSON is a natural fit for this: it's simple to hand-edit, simple to validate, and trivially parseable with Newtonsoft.Json — consistent with the serialization choice already made for save data.

#### 9.3 `OfferDataService` is a Mapper, not just a config reader

It's worth being precise about what `OfferDataService` actually does, because it's doing more than "load JSON." It performs a genuine transformation **DTO into Runtime Model that is mapping**: `OfferConfig` (the deserialized DTO, a near-direct reflection of the JSON shape) is converted into `Offer` (the runtime model), including resolving the image address into an actually-loaded **Sprite** via the `AddressableAssetProvider` along the way. This conversion step is exactly the Mapper pattern an explicit translation layer between an external data representation and the internal model the rest of the application actually works with.

**Why is this mapping step valuable, rather than just using the config object directly everywhere?**

Because it means the JSON schema and the runtime Offer model are allowed to diverge and evolve independently. If the JSON format needs to change (new fields, restructured nesting, a different config source entirely), only the mapping step inside `OfferDataService` needs to change — every consumer downstream that works with Offer is completely unaffected, because they never depended on the JSON shape in the first place.

#### 9.4 Two separate types of config - event config and proposal config.

`TripleOfferConfig` describes the event itself (start time, end time, duration), while each `OfferConfig` inside it describes one purchasable offer (resource type, amount, price, image address). This separation mirrors a real conceptual distinction: "when is this event running" is a property of the event as a whole, while "what can be bought" is a property of each individual offer within it. Keeping them separate means the event's timing can be adjusted without touching offer definitions, and offers can be added, removed, or rebalanced without touching the event's timing — two independent axes of change, modeled as two independent types.

#### 9.5 Convenience of scaling.

Because new event types can introduce their own config/runtime model pairs and their own mapper-equivalent service, following the exact same shape established here, without needing to modify `OfferDataService` or `TripleOfferEvent` at all. The pattern — DTO in, mapper converts, runtime model out — is established once and is fully repeatable.

### 10. Triple Offer Event

[_Back to Architecture content_](#architecture)

`TripleOfferEvent` is the actual gameplay-facing event implementation, and it cleanly demonstrates several of the principles already discussed, working together.

#### 10.1 Responsibility breakdown

* `TripleOfferEvent` — orchestrates the event's lifecycle from the perspective of what offers are currently available: validates that the event is active (not yet started, not yet ended), loads/filters offers (removing already-purchased ones via `ISaveLoadService`), and exposes offers to any Presenter that requests them. It also reacts to purchases (`IIAPService.OnOfferPurchased`) by removing the purchased offer from the active list and signaling when all offers have been bought.
* `OfferDataService` — as covered above, is the data/config layer: it knows how to read and map offer/event configuration into runtime `Offer` objects. `TripleOfferEvent` doesn't know or care how that data was produced — it simply asks for it.
* `TripleOfferEventTimer` — owns only the timing logic: how much time remains, how much time until the event starts, and firing `OnTimerTick`/`OnTimerEnd`.

#### 10.2 Why is timing logic extracted into its own class, rather than living inside `TripleOfferEvent`?

This is the Single Responsibility Principle applied very precisely: `TripleOfferEvent`'s job is to manage which offers are currently valid and purchasable; it should not also need to know how a countdown is implemented (a polling loop with `UniTask.Delay`, a `CancellationTokenSource` for clean cancellation, etc.). By extracting this into `TripleOfferEventTimer` behind a narrow interface (`StartTimer()`, `StopTimer()`, `GetRemainingTime()`, plus `OnTimerTick`/`OnTimerEnd`), `TripleOfferEvent` only ever interacts with a small, stable contract — and the implementation of how time is tracked could change completely (e.g., switching from a polling loop to a server-synced timestamp comparison) without `TripleOfferEvent` needing any changes at all.

This also has a concrete testability payoff consistent with the DI discussion earlier: `TripleOfferEvent`'s offer-filtering and validity logic can be tested by injecting a fake `IEventTimer` that returns controlled time values, without needing a real running countdown or real wall-clock time to pass during a test.

#### 10.3 How architectural solutions are intertwined together

`TripleOfferEvent` depends on `IOfferDataService` (to get offer/config data), `IEventTimer` (to know if the event is active and how much time remains), ISaveLoadService (to filter out already-purchased offers), and `IIAPService` (to react to purchase notifications) — every one of these as an interface, injected through the constructor, exactly per the DI principles established earlier. This single class is a good concrete demonstration of nearly every architectural decision in this document working together at once: DI for its dependencies, SRP for why each dependency exists as a separate class rather than being absorbed into `TripleOfferEvent` itself, and Observer for how it both reacts to (`IIAPService.OnOfferPurchased`) and produces (`OnAllOffersPurchased`, `OnEventStarted`, `OnEventEnded`) events that other parts of the system subscribe to.

### 11. Addressables

[_Back to Architecture content_](#architecture)

The project uses Unity's Addressables system for asset management, accessed through an abstraction — `IAddressableAssetProvider` — rather than calling Addressables APIs directly from consuming code.

#### 11.1 Why Addressables?

The primary, concrete motivation in this project is the **UI prefabs themselves** — the store panel, offer slots, and related interface elements are loaded as Addressable assets rather than being directly serialized scene/prefab references. This was a deliberate choice because it means the visual presentation of the UI can be updated remotely, independently of the rest of the application:

* Addressables decouple an asset's address (a string key, used in code) from its physical location and storage (which can be a local build, a remote CDN-hosted asset bundle, or reassigned to a different bundle entirely) — without changing a single line of the code that requests it.
* In a LiveOps context, this matters concretely: a **UI** prefab referenced by address can be swapped for an updated version delivered remotely (a new store layout, a reskinned offer slot, a seasonal visual refresh) **without requiring an app store update or rebuilding the player**. Direct prefab references or `Resources.Load` calls offer no equivalent path — both require the asset to be baked directly into the build.
* This also keeps initial build/download size smaller in principle, since not every asset needs to be packaged into the base build — assets can be fetched on demand or delivered remotely, which matters more as a project's content grows (more events, more seasonal **UI** variants, etc.) than it does in this sample's current scope.

This sample currently uses Addressables specifically for **UI** prefabs and for offer images (loaded by `OfferDataService` via the same provider), but the same mechanism generalizes to any asset type — the same justification (remote updatability, decoupling logical reference from physical storage) applies regardless of whether the asset is a UI prefab, a sprite, or, in a larger project, audio or level data.

#### 11.2 Addressables access wrapped in `IAddressableAssetProvider`.

This follows the same Dependency Inversion logic applied everywhere else in the project: nothing outside this one provider needs to know that Addressables specifically is the underlying asset system. `OfferDataService` calls `LoadAssetAsync<Sprite>(address)` and has no idea whether that's backed by `Addressables`, `Resources`, or some other future asset pipeline. This means:

* `Addressables`-specific concepts (handles, async operation lifecycle, release semantics) are contained entirely inside one implementation, instead of leaking into every consumer that happens to need an asset.
* If the asset pipeline ever needed to change — a different bundling solution, a custom CDN-backed loader — only `IAddressableAssetProvider`'s implementation would need to change. Every consumer (`OfferDataService`, UI loading code, anything else) continues calling the same interface, completely unaware that anything changed underneath it.


### 12. Asynchronous Programming

[_Back to Architecture content_](#architecture)

As mentioned in other sections, the project utilizes asynchronous programming. We'll discuss this topic in more detail here.

#### 12.1 Why UniTask, rather than .NET's Task?

* **No thread-pool allocation overhead for Unity's single-threaded model**. Task was designed for general .NET multithreading; on Unity, most async work (waiting a frame, waiting on an Addressables handle, a short delay) doesn't need a thread pool thread at all, and Task's machinery carries allocation overhead that's wasted in that context. UniTask is built specifically around Unity's PlayerLoop, so common operations (UniTask.Delay, UniTask.Yield, awaiting an Addressables handle) run with far lower allocation overhead.
* **Native integration with Unity's lifecycle**. UniTask provides first-class support for awaiting Unity-native asynchronous operations (Addressables handles, AsyncOperation) directly, without needing manual wrapper code to bridge them into Task-based awaitables.
* **Cancellation that fits Unity's object lifecycle**. UniTask works cleanly with `CancellationTokenSource`/`CancellationToken` in patterns that map naturally onto Unity object lifetimes (e.g., cancelling a timer loop cleanly via `Dispose()`, as seen in `TripleOfferEventTimer`).

#### 12.2 Why not Coroutines?

Coroutines (`IEnumerator`-based, run via `StartCoroutine`) were not used because they have real, well-known limitations that matter for this codebase specifically:

* **No return values**. A coroutine cannot directly return a result (e.g., "did the purchase succeed," "here is the loaded Sprite") — results have to be smuggled out via callbacks or shared mutable state, which is exactly the kind of implicit, hard-to-follow data flow this project's design principles are trying to avoid everywhere else.
* **No structured exception propagation**. Exceptions inside a coroutine don't propagate to the caller the way an exception in an `async/await` chain does, which makes error handling significantly more awkward.
* **Tied to a MonoBehaviour**. Coroutines require a live `MonoBehaviour` to run on; plain C# service classes (which is what nearly everything in this project's **Services** layer is, deliberately, to stay decoupled from Unity's component model) cannot start a coroutine without an awkward workaround. UniTask, by contrast, works in any plain C# class.
* `async UniTask` methods, by comparison, return values naturally, propagate exceptions through normal `try`/`catch`, and work identically inside a plain service class or a `MonoBehaviour` — which is exactly the uniformity this project's service-oriented design depends on.

#### 12.3 Why is asynchrony used only where it's actually needed?

Not everything in the project is async — e.g., simple synchronous getters and pure data transformations stay synchronous. Async is reserved specifically for operations that involve genuine waiting: file I/O (SaveLoadService), Addressables asset loading (OfferDataService, the asset provider), timed delays (`TripleOfferEvent`Timer), and an interface deliberately shaped for future waiting (`IIAPService`). Using `async/await` indiscriminately on operations that never actually wait on anything adds cognitive overhead (every caller now has to `await` or .Forget() something) without any corresponding benefit. Additionally, it's important to understand that the `async` keyword generates a **State Machine** under the hood, which leads to additional allocations, **GC** calls, and, as a result, system overhead. For all these reasons, asynchronous operation is used only where necessary.

#### 12.4 Example of asynchronous services: `IAPService` and `SaveLoadService`

As established in [Section 8.3](#83-savingloading-are-made-asynchronous-but-why): `SaveLoadService` **is async because of present-day file I/O cost** (real disk operations that can block the main thread if done synchronously, and which happen frequently here due to event-driven autosave).

`IAPService` **is async because of anticipated future need** (a real purchase flow will involve network calls to a store/backend that don't exist yet in this sample's stub implementation). These are independent justifications that happen to compose well together, not one causing the other. 

### 13. Extensibility

[_Back to Architecture content_](#architecture)

This final section is really the payoff of every decision documented above: because the system is built on interfaces, dependency injection, and clear separation of concerns, several categories of future change can be made **without modifying existing, working code — which is the Open/Closed** Principle, demonstrated concretely rather than just claimed. **Examples:**

* **Replacing** `IAPService`. The current implementation is intentionally a simple stand-in. Because every consumer depends on `IIAPService`, not `IAPService`, introducing a real platform-billing/backend-validated implementation later is a matter of writing the new class and changing one binding in the Zenject installer — `TripleOfferEvent`, any Presenter, and anything else consuming `IIAPService` requires zero changes.
* **Replacing the save backend**. Swapping local JSON file storage for, say, cloud-synced saves or a different serialization format is fully contained behind `ISaveLoadService`. Every consumer asks for `PlayerProgress` the same way regardless of where or how it's actually persisted underneath.
* **Adding a new Event**. The `TripleOfferEvent` / `OfferDataService` / `TripleOfferEventTimer` triad establishes a repeatable shape (config - mapper - runtime model, plus an isolated timer) that a new event type can follow independently, without requiring changes to the existing event's classes.
* **Adding new Offers**. Since offers are entirely config-driven (JSON, mapped to the runtime `Offer` model), adding, removing, or rebalancing offers is a pure content/data change — no code changes required at all.
* **Adding a new storage mechanism more generally**. Because persistence is fully encapsulated behind `ISaveLoadService`, and asset loading is fully encapsulated behind `IAddressableAssetProvider`, introducing an entirely new storage or delivery mechanism for either is a matter of writing a new implementation behind the existing interface.
* **Replacing Addressables**. Because nothing outside `IAddressableAssetProvider` calls into the Addressables API directly, the entire asset pipeline could, in principle, be replaced with a different system without touching `OfferDataService`, UI loading code, or anything else that currently just calls `LoadAssetAsync<T>(address)`.

This MVP project is therefore easily scalable, as it represents a solid foundation. **Every point of potential future change in this project is located beyond the interface boundary, which was intentionally and preemptively established**, with planning for future expansion. This was the primary goal of the project — to demonstrate a high-quality development approach and architecture.

## Tech Stack

- Unity 2021.3 LTS
- Zenject
- UniTask
- Newtonsoft.Json
- Assembly Definition
- Addressables
- LINQ