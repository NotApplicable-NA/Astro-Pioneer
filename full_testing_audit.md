# Astro-Pioneer Issue Audit & Testing Plan (Developer Focus)

## 🚨 Systemic Regression (Architecture & Logic)
Testing stabilitas logika inti setelah refaktor `ChunkManager`, `ServiceLocator`, dan `SimulationMaster`.

### [ ] **#56** - [P1] TICKET-056: Power Management System
  - [✅] Verify power grid connects across chunk boundaries (Data-link check)
  - [✅] Test generator output and battery storage persistence (Serialization check)
  - [???] Ensure Solar Panels react to TimeManager cycles (Not developed)

### [ ] **#35** - [P1] TICKET-035: Oxygen/HP System
  - [✅] Verify O2 depletes correctly during exploration (Logic check)
  - [??] Test O2 regeneration logic near Oxy-Flora/Base (Not developed)
  - [✅] Check Fatigue trigger conditions in Simulation loop

### [ ] **#34** - [P1] TICKET-034: Resource Gathering
  - [??] Test mining/logging tool logic on world objects (Not developed)
  - [??] Verify resource drops and inventory addition logic (Not developed)
  - [??] Ensure mined objects persist via ChunkData (Not developed)

### [✅] **#22** - [P0] TICKET-022: Bot Pathfinding System
  - [✅] Verify bot uses A* pathfinding on the new Chunk grid
  - [✅] Ensure bot recalculates if path is blocked by new structure
  - [✅] Test deterministic behavior across chunk borders

### [✅] **#14** - [P1] TICKET-014: Time-Based Events
  - [✅] Verify SimulationMaster correctly increments time & triggers daily events
  - [✅] Test tick-based fast-forwarding (Sleep)

### [✅] **#7** - [P0] TICKET-007: Crop System Foundation
  - [✅] Verify CropManager communicates with GridManager (Data Layer)
  - [✅] Test object pooling (Spawn/Despawn) efficiency in ChunkRenderer
  - [✅] Verify structure data is saved to binary files

### [✅] **#2** - [P0] TICKET-002: Setup 2D Global Lighting
  - [✅] Verify global 2D light syncs with TimeManager ticks

---

## 🏗️ Active Developer Tasks (New Architecture Implementation)
Fitur yang sedang dalam pengembangan. Wajib mematuhi SOP Arsitektur (Hukum 1, 2, 3).

### [ ] **#64** - [Developer] - Automation: Circular Economy Processing
  - [ ] Test Composter converting bio-mass to fertilizer
  - [ ] Verify input/output ratios

### [✅] **#63** - [Developer] - Building Engine: Exterior Dual-Grid System
  - [✅] Test placing fences, paths, and large structures
  - [✅] Verify structure bounds checking

### [✅] **#62** - [Developer] - Exploration: Fog of War & Auto-Discovery UI
  - [✅] Test fog of war clears upon player movement
  - [✅] Verify chunk boundary edge cases

### [ ] **#61** - [Developer] - Progression: Tech Tree & Blueprint System
  - [ ] Test unlocking new items with Research Data
  - [ ] Verify blueprints appear in crafting menu

### [ ] **#60** - [Developer] - Narrative: Ecological Footprint & Endgame State
  - [ ] Test endgame trigger conditions
  - [ ] Verify pollution/ecological variables

### [ ] **#59** - [Developer] - Animal Husbandry: Enclosures & Fauna AI Engine
  - [ ] Test Enclosure detection logic (closed-loop fence check)
  - [ ] Verify fauna breeding/happiness

### [ ] **#57** - [Developer] - Core System: Shadow Canyons Ruleset
  - [ ] Test UV Light Pillar functionality in dark zones
  - [ ] Ensure normal crops don't grow without UV light

### [ ] **#55** - [Developer] - Ops: Build & Deployment
  - [ ] Test building standalone executable
  - [ ] Verify no Unity Editor dependencies in scripts

### [ ] **#54** - [Developer] - Data: Localization Framework
  - [ ] Test switching languages at runtime
  - [ ] Verify all UI text updates dynamically

### [ ] **#53** - [Developer] - Core: Release Bug Fixing
  - [ ] Fix all existing null reference errors
  - [ ] Review Unity crash logs

### [ ] **#52** - [Developer] - Core: Performance Optimization
  - [ ] Optimize ChunkRenderer Object Pool
  - [ ] Reduce GC allocations in Update loops

### [ ] **#51** - [Developer] - Core System: Save/Load State
  - [ ] Test writing binary chunk data
  - [ ] Test reading and deserializing chunk data
  - [ ] Handle corrupted save file recovery

### [ ] **#49** - [Developer] - System: Additional Planet Scaling
  - [ ] Test procedural generation parameters for new planets
  - [ ] Verify biome data loading

### [ ] **#48** - [Developer] - UI: Pause Menu Implementation
  - [ ] Ensure game pauses (Time.timeScale = 0) when menu opens
  - [ ] Test resume functionality

### [ ] **#47** - [Developer] - UI: Settings Logic
  - [ ] Test volume sliders (Music/SFX)
  - [ ] Save settings to PlayerPrefs

### [ ] **#46** - [Developer] - System: Tutorial Implementation
  - [ ] Implement step-by-step UI guides
  - [ ] Add highlighting to important UI elements

### [ ] **#42** - [Developer] - Data: Advanced Crafting Recipes
  - [ ] Implement late-game recipes (Quantum Core, etc)
  - [ ] Verify crafting time balancing

### [ ] **#41** - [Developer] - Farming: Late Game Crops Logic
  - [ ] Test multi-harvest crops (e.g., Flux Berry)
  - [ ] Test trellis logic (Solar-Vine)

### [ ] **#40** - [Developer] - UI: Automation Control Panel
  - [ ] Implement UI to toggle machines on/off
  - [ ] Show power consumption stats

### [ ] **#39** - [Developer] - Core System: Advanced Storage
  - [ ] Implement linked storage networks
  - [ ] Test auto-sorting algorithms

### [ ] **#38** - [Developer] - Automation: Harvester Logic
  - [ ] Test Harvester machine picking up mature crops automatically
  - [ ] Verify deposit into adjacent storage

### [ ] **#37** - [Developer] - Automation: Agri-Mech Logic
  - [ ] Test Agri-Mech planting, watering, and harvesting
  - [ ] Verify fuel/battery consumption

### [ ] **#36** - [Developer] - Mechanic: Rescue Protocol Script
  - [ ] Implement drone rescue when O2 hits 0
  - [ ] Teleport player to bed, reset O2

### [ ] **#33** - [Developer] - System: Planet Exploration Framework
  - [ ] Implement resource node generation on new planets
  - [ ] Test landing sequence

### [ ] **#32** - [Developer] - System: Ship Upgrade Logic
  - [ ] Implement upgrading ship modules (O2 capacity, Engine)
  - [ ] Deduct resources on upgrade

### [ ] **#31** - [Developer] - Core System: Ship Interior Grid System
  - [ ] Separate interior ship grid from exterior planet grid
  - [ ] Ensure saves don't overlap

### [ ] **#30** - [Developer] - UI: Trading Interface
  - [ ] Implement buy/sell logic in trading menu
  - [ ] Verify dynamic pricing

### [ ] **#29** - [Developer] - Core System: Currency & Trust Tracking
  - [ ] Implement database for Credits and Trust points
  - [ ] Bind to UI top bar

### [ ] **#28** - [Developer] - System: Trading Post Logic
  - [ ] Implement Trading Post item acceptance logic
  - [ ] Trigger daily shipment event

### [ ] **#27** - [Developer] - Automation: Resource Processing
  - [ ] Implement Smelter/Assembler conversion logic
  - [ ] Test input queue and output slot

### [ ] **#26** - [Developer] - UI: Crafting Interface Logic
  - [ ] Implement UI layout for Crafting
  - [ ] Show required vs available materials

### [ ] **#25** - [Developer] - Core System: Crafting System Core
  - [ ] Implement base crafting manager
  - [ ] Handle inventory item consumption

### [ ] **#21** - [Developer] - Automation: Rover Buddy (Local Transport Bot)
  - [ ] Implement Transport Bot inventory
  - [ ] Set A->B logic for moving items between chests

### [ ] **#16** - [Developer] - Core System: Inventory Storage & Limit logic
  - [ ] Implement inventory array logic
  - [ ] Enforce stack limits (e.g., 99 per slot)

### [ ] **#15** - [Developer] - Core System: Fatigue & Sleep Mechanic
  - [ ] Implement fatigue debuff (slower movement, fast O2 drain)
  - [ ] Reset fatigue upon sleeping

### [ ] **#13** - [Developer] - Core System: Day/Night Cycle
  - [ ] Calculate sun position based on game time
  - [ ] Trigger OnDayStart and OnNightStart events

### [ ] **#10** - [Developer] - Mechanics: Planting & Watering Logic
  - [ ] Check inventory for seed/water
  - [ ] Apply to target grid via MouseInteractionSystem
