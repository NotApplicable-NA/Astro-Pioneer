# 🧪 Developer Testing Checklist

**Current Date:** 2026-04-05  
**Owner:** Developer (Antigravity)  
**Status:** Pre-QA Verification

> [!IMPORTANT]
> The following features have been implemented or updated in the codebase but require thorough manual testing before being handed over to the formal QA team.

---

## 🛠️ Features to Test

### 1. [Core] - 15 Minute Day Cycle (#13)
- **Implemented:** `TimeManager.cs`
- **Testing Criteria:**
    - [ ] Wait 1 full day (15 minutes). Verify the clock rolls over precisely at 900s.
    - [ ] Verify `OnDayChanged` event triggers on day increment.
    - [ ] Verify light color interpolation remains smooth over the longer duration.

### 2. [Core] - Updated Inventory Specs (#16)
- **Implemented:** `InventoryManager.cs`, `InventoryItem.cs`
- **Testing Criteria:**
    - [ ] Check total slots = **64**.
    - [ ] Add items and verify they stop stacking at **64**.
    - [ ] Verify the UI Layout ($8 \times 8$) is visually coherent.

### 3. [UI] - Inventory Staggered Loading
- **Implemented:** `InventoryUI.cs`
- **Testing Criteria:**
    - [ ] Open the inventory the first time. Verify no frame spike/freeze.
    - [ ] Verify all slots are interactable after the loading coroutine finishes.

### 4. [System] - Holographic Tablet (#33)
- **Implemented:** `HolographicTabletUI.cs`, `UIManager.cs`
- **Testing Criteria:**
    - [ ] Press **[M]** to open and close.
    - [ ] Verify the Player Icon (Yellow) moves as the player walks.
    - [ ] Place a machine (Sprinkler/Pump) and verify a Cyan icon appears on the map.
    - [ ] Verify game pauses when the tablet is open (for cozy safety).

### 5. [Survival] - Fatigue Scaling Debuff (#15)
- **Implemented:** `PlayerVitals.cs`
- **Testing Criteria:**
    - [ ] Wait for a Day Rollover (15 mins). Check console for "Multiplier: 1.25x".
    - [ ] Go to a planet and verify O2 bar drains 25% faster than Day 1.
    - [ ] Verify HUD shows "FATIGUED: 1.2x Drain" warning.

### 6. [Machine] - Sleep Pod Interaction
- **Implemented:** `SleepPod.cs`, `TimeManager.cs`
- **Testing Criteria:**
    - [x] Click on Sleep Pod (must be nearby).
    - [x] Verify time jumps to 06:00 AM and Day increments if it was night.
    - [x] Verify Fatigue counter resets to Day 1 and HUD warning disappears.

### 7. [System] - Rescue Protocol GDD 3.5 Compliance (#36)
- **Implemented:** `RescueProtocol.cs`
- **Testing Criteria:**
    - [x] Pass out on Planet (O2 = 0).
    - [x] Verify teleported to Sleep Pod position on ship.
    - [x] Verify Inventory and Credits are UNCHANGED.
    - [x] Verify no time skip occurred (Rescue != Sleep).

### 9. [Exploration] - Fog of War (Tablet Discovery) (#60)
- **Implemented:** `GridManager.cs`, `ExplorationTracker.cs`, `HolographicTabletUI.cs`
- **Testing Criteria:**
    - [ ] Open Tablet ([M]) on a new planet. Verify map is mostly dark (Fog).
    - [ ] Walk around the planet and reopen tablet. Verify a path is revealed.
    - [ ] Verify POIs (Machines) are HIDDEN in the fog and only appear when revealed.
    - [ ] Persistence Test: Explore 20%, return to ship, go back to planet. Verify 20% is still revealed.

### 10. [Environment] - Shadow Canyons & UV Pillars (#57)
- **Implemented:** `GridManager.cs`, `CropInstance.cs`, `UVLightPillar.cs`, `ShadowZone.cs`
- **Testing Criteria:**
    - [x] Place a **ShadowZone** in the scene.
    - [x] Plant a crop in the ShadowZone. Verify it is VISUALLY DIMMED.
    - [x] Wait for a day change (15 mins or skip). Verify growth STOPS.
    - [x] Place a **UV Light Pillar** near the crop. Verify visual returns to NORMAL.
    - [x] After the next day change, verify growth RESUMES.

### 8. [Economy] - Dual Currency System (#29)
- **Implemented:** `ShippingBin.cs`, `TradingPost.cs`
- **Testing Criteria:**
    - [x] Ship a Space Potato via Shipping Bin. Verify **Trust** increases (Rate 1:1), but Credits stay same.
    - [x] Sell a Space Potato via Trading Post. Verify **Credits** increase (Rate 1:1), but Trust stays same.
    - [x] Buy an item via Trading Post. Verify **Credits** decrease.

---

## 📈 Past Implementation Status (Need Re-Verification)

| Feature | Ticket Ref | Status | Note |
|:---|:---:|:---:|:---|
| Planting/Watering Logic | #10 | 🟢 | Verify with new 15m day logic |
| Rover Buddy Transport | #21 | 🟢 | Pathfinding check |
| Crafting System | #25 | 🟢 | Atomic transaction check |
| Trading Post | #28 | 🟢 | Credit exchange check |

---
*Created by Dev Agent (Antigravity).*
