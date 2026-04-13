# Astro-Pioneer
## Developer Handoff Context (Sprint 1 - Sprint 9)

Welcome to the **Astro-Pioneer** codebase! This document serves as the definitive technical breakdown of all C# scripts developed up to Sprint 9. Use this guide to understand the system architecture, core loops, and individual script responsibilities. 

The project uses a **Manager-Pattern architecture** combined with a robust **2D Grid System**. All scripts are located in `Assets/Scripts/`.

---

### 1. Core Data Structures (`Data/`)
These are defining `ScriptableObjects` and pure classes that hold static parameters and serialize state data without containing behavioral logic.

- **`CropData.cs`**: `ScriptableObject` defining crop parameters (name, growth days, harvest item, sprites per stage, rendering layers).
- **`CraftingRecipe.cs`**: `ScriptableObject` tracking input dependencies (ingredients) and output results for the crafting and processor stations.
- **`InventoryItem.cs`**: Base item definition (name, ID, description, UI icon, stack size, and `ItemType`).
- **`InventorySlot.cs`**: Pure data structure holding an `InventoryItem` reference and its current integer `quantity`.
- **`PlanetData.cs`**: `ScriptableObject` that defines the constraints of a visitable environment (oxygen limits, available resource nodes, spawn weighting).
- **`ShipRoom.cs`**: `ScriptableObject` classifying unlockable/expandable ship interior spaces.

---

### 2. Core Managers (`Managers/`)
These Singletons govern the global game state and systems.

- **`GridManager.cs`**: The heart of the game spatial reasoning. Tracks the 2D array of grid cells. Checks availability (`IsPositionAvailable`), manages occupied states (`TryOccupyCell`, `ReleaseCell`), and translates between Grid and Global World Space coordinates.
- **`CropManager.cs`**: Instantiates and oversees `CropInstance` objects. Routes watering orders and ensures visual pivot alignments so multiple crops scale seamlessly into the Grid cells. Works heavily with `GridManager`.
- **`PlacementManager.cs`**: Handles the logic for spawning machines and base-building. Converts screen clicks to grid coordinates, renders the build hologram, verifies collision geometry bounds for Multi-Tile (1x1, 2x2, etc.) buildings, and triggers their initialization.
- **`SaveGameManager.cs`**: Handles JSON serialization for game persistence using `PlayerPrefs`. Iterates through all registered `CropInstance`, `MachineIDTag`, and `InventoryManager` data, saving/loading positions and IDs securely across play sessions.
- **`InventoryManager.cs`**: Tracks the global player inventory array. Handles Add/Remove item logic, stack-splitting limits, and broadcasts update events to UI layers.
- **`TimeManager.cs`**: Manages the continuous day/night time cycle, game tick evaluations, and triggers `OnDayChanged` broadcasts to push crop growth phases forward.
- **`BotManager.cs` & `CraftingManager.cs`**: Oversee their respective domains, orchestrating processing pipelines and automated drone path-dispatch validations.
- **`CurrencyManager.cs`**: Simple wrapper around the player's 'Credits' (money) balance for unified trading logic.
- **`EcoTracker.cs` & `PowerManager.cs`**: Manages base logistics variables (Power generation vs Consumption limits). 
- **`SettingsManager.cs`**: Handles visual/audio prefs (not game simulation mechanics).
- **`UIManager.cs`**: Standard overarching UI overlay manager.
- **`ResourceManager.cs` & `ShipUpgradeManager.cs`**: Evaluates conditions for upgrading the spaceship tier based on collected materials.
- **`TechTreeManager.cs`**: Tracks unlocking tiers of specific blueprints/seeds over the campaign progression.

---

### 3. Player Controllers & Interactions (`Player/` & Interaction Systems)
- **`PlayerMovement.cs`**: Uses integer-filtered or normalized 8-directional physics movements combined with a Kinematic `Rigidbody2D` to traverse smoothly. Listens sequentially for INC outputs.
- **`CameraFollow.cs`**: Interpolating wrapper scripting the Main Camera's transition toward the Player origin.
- **`PlayerToolState.cs`**: The interaction switchboard. Maps hotbar slots to active items. Listens to Mouse Inputs via `MouseInteractionSystem.cs`. Interprets the collision context to apply the selected tool (e.g., if holding a Hoe -> attempts clearing empty grid space; if holding Seeds -> routes `CropManager.PlantCrop`).
- **`MouseInteractionSystem.cs`**: Raycast & Grid-Raycasting handler that interprets where the user cursor lands and transmits `Vector2Int` targets back to the active systems.

---

### 4. Exploring, Survival, & Environments (`Systems/`)

- **Survival & Vitals**
  - **`Survival/PlayerVitals.cs`**: Analyzes standard limits specifically around the Oxygen meter. Decays continuously down based on conditions.
  - **`Survival/RescueProtocol.cs`**: Handles zero-HP penalties. If Oxygen depletes, flags the player as incapacitated, waits for a Bot rescue sequence, and teleports them to their Sleep Pod inside the ship without dropping items via a "Walk of Shame" sequence.

- **Exploration Loop**
  - **`Exploration/ExplorationManager.cs`**: Manages instances where the player exits the spaceship onto new planets. Spawns scattered geometries around a radius layout instead of strict predefined grid maps.
  - **`Exploration/OxyFlora.cs`**: Rare luminescent plants that, when touched via `OnTriggerEnter2D`, restore a fraction of the Player’s Oxygen immediately.
  - **`Exploration/ResourceNode.cs`**: Geometries scattered by ExplorationManager that fracture into base materials when hit by specific tool levels.
  - **`Exploration/ExplorationTracker.cs`**: Progression logs tracking POIs explored on distinct planet instances.
  - **`Environment/ShadowZone.cs`**: Collision layers preventing crop photosynthesis. Also triggers UV penalties if not overridden.

---

### 5. Automation, Machines & Devices (`Machines/` & Interfaces)
- **Shared Architecture (`Interfaces/` & Tags)**:
  - **`IGridInteractable.cs`, `IPowerConsumer.cs`, `IPowerGenerator.cs`**: Common contracts standardizing machine triggers and power net integrations.
  - **`MachineIDTag.cs`**: Placed on ALL spawned object instances to log their Origin Grid Coordinate and original ID string. Vital for the `SaveGameManager` serialization pipeline.
  - **`MachineDirection.cs`**: Multi-state Enum mapper dictating which orientation (Up, Down, Left, Right) animated sprites utilize.

- **The Machines**:
  - **`AgriMech.cs`**: A 2x2 multi-tiled automation mecha. Subscribes to `PathfindingManager` sweeps to determine row routes. It uses a custom `Initialize()` offset math sequence to keep its origin stable and dynamically deposits assigned Crop Seeds (`cropID`) into its route.
  - **`TransportBot.cs` & `BotController.cs`**: Logistics drone resolving interactions between empty crop zones, harvested drop zones, and `MachineStorage.cs`, ferrying items directly.
  - **`MachineComposter.cs` & `ProcessingStation.cs`**: Delayed timer conversion machines that interpret input items and generate altered forms using `CraftingRecipe.cs` maps.
  - **`Sprinkler.cs`**: Scans 3x3 radiuses upon the daily refresh pulse (`OnDayChanged`) routing `CropManager.WaterCropAt(pos)` to overlapping coordinates.
  - **`MachineHarvester.cs`**: Listens for Stage 3 status on nearby grid offsets to automate harvesting calls independent of the Player state.
  - **`SleepPod.cs`**: The interaction node to trigger a time-skip manually and restore vital metrics globally.
  - **`UVLightPillar.cs`**: Generates a grid-based radial zone overriding the `isDim` and `inShadowZone` logic flags for Crops, allowing them to advance their growth stages indoors or inside shadows.
  - **`MachineWaterPump.cs`, `MachineGenerator.cs`, `MachineBattery.cs`, `AdvancedStorage.cs`, `MachineStorage.cs`**: Node controllers managing pipe arrays mapping capacity checks vs generation limits.

- **`DayNightLightController.cs`**: Modifies the global 2D environment luminosity mapping to match `TimeManager.cs` hours.

---

### 6. Progression Systems (`Systems/Ship/` & Pathfinding)
- **`Ship/ShipGrid.cs`**: Sub-system isolating valid "Floor" spaces separate from the bare dirt grids. 
- **`Ship/ShipPlacementSystem.cs`**: Specialized variant of `PlacementManager.cs` strictly governing interior decorations vs structural boundary blocks (`ShipRoom`). Validates overlaps internally.
- **`Pathfinding/PathfindingManager.cs`, `PathfindingGrid.cs`, `PathNode.cs`**: Subsystem powering A* calculation. Maps physical colliders back into heuristic weights globally so Bots can navigate autonomously.
- **`TradingPost.cs` & `ShippingBin.cs`**: Interprets items thrown onto them. Applies market weight valuation directly to `CurrencyManager` balance matrices.
- **`Husbandry/EnclosureSystem.cs`**: Analyzes linked fence layouts recursively to create bounded pens.
- **`Husbandry/FaunaAI.cs`**: AI nodes attempting random walks locked entirely within boundaries managed by `EnclosureSystem`.

---

### 7. Crop lifecycle Management
- **`Systems/CropInstance.cs`**: The heart of farming. Represents an active plant. Scales geometrically scaled at ~`0.0625f` to lock into 16-PPU resolution. Listens via observer pattern to `TimeManager.OnDayChanged`. It evaluates environment constraints via spatial overlays (shadows / UV hits) and toggles specific VFX references inside its timeline to simulate visual age. Interfacing components route `Harvest()` which eventually cascades back to `CropManager.RemoveCrop` explicitly.

---

### 8. User Interface (Diegetic Holographic & Standard Layers) (`UI/`)
All files exist under `Scripts/UI/` connecting strictly via Unity UI components rather than overlapping update loops.

- **`HolographicTabletUI.cs`**: The centerpiece of GDD 3.5. Emulates a physical tablet space switching views seamlessly instead of layering standard 2D popups. Renders contextual app frames like Storage, Trade, & Automation logic overlays.
- **`InventoryUI.cs` & `InventorySlotUI.cs`**: Dynamic canvas population observing `InventoryManager`. Handles sprite swaps, font scaling, and drag-and-drop reference callbacks.
- **`ToolBarUI.cs`**: Specialized subset tracking strictly slots 0-5 and broadcasting `PlayerToolState` callbacks.
- **`AutomationUI.cs`, `CraftingUI.cs`, `EconomyUI.cs`, `ResourceUI.cs`, `VitalsUI.cs`, `StorageUI.cs`, `TradingUI.cs`, `ShipUpgradeUI.cs`**: Niche UI controllers responding exclusively to specific Manager updates.
- **`PauseMenuUI.cs` & `TutorialManager.cs`**: Pure flow state scripts controlling Game pausing context.

---

### 9. Visual Feedbacks (`VFX/`)
Short-lived components injected dynamically to enhance tactile interaction.

- **`GrowthTransitionVFX.cs`**: Simple particle pop when crop data iterates. 
- **`HarvestableGlow.cs`**: Material shader parameter tweaker pulsing emissions across mature objects.
- **`HarvestVFX.cs` & `WateringVFX.cs`**: Quick instantiated effects pooling from memory when tools are active. Also maps coordinates over `SprinklerVFX.cs` specifically.

--- 

### Next Steps for Maintainers
> [!IMPORTANT]
> The Engine operates on strict Cartesian coordinates centered globally for mapping items, scaling tightly via `0.0625f` (for 16-Pixel per unit models). Ensure all new `.asset` data implementations register properly onto the predefined **Managers** grids or they will drop from memory cycles across Load/Save actions in the `SaveGameManager.cs`!
