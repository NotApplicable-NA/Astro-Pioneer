# GDD 3.5 COMPLIANCE + SPRINT 7 AUDIT REPORT - ZERO TRUST

**Date:** 2026-03-01
**Auditor:** CYGNUS v2.0
**Scope:** GDD v3.5 Compliance + Sprint 7 Code (Ship Interior, Exploration, Survival)
**Status:** ❌ FAILED (7 Issues Found: 2 Critical, 3 High, 2 Medium)

---

## ✅ GDD 3.5 COMPLIANCE CHECK

### PPU 16 Migration ✅ COMPLIANT
| Location | Status |
|:---------|:-------|
| `GridManager.cs` L18 — Tooltip `"(1 unit = 16 pixels, PPU 16)"` | ✅ |
| `ShipGrid.cs` L37 — Tooltip `"(PPU 16)"`, cellSize = 1.0f | ✅ |
| `CropInstance.cs` L51 — Comment `"PPU 16, cell size 1.0"` | ✅ |
| `README.md` L145 — `"PPU: 16"` | ✅ |
| **No old PPU 880 references found in codebase** | ✅ |

### Bot-E / Rover Buddy Naming ✅
- GDD refers to `Bot-E (Rover Buddy)`. Code uses `TransportBot` as class name — acceptable technical name.

### Crop Database ✅ MATCHES GDD
- `CropData.cs` supports all GDD fields (growthTime, sellPrice, requiresWater).
- Sell prices and growth times to be set in SO instances (not hardcoded).

### Economy (Credits + Trust) ✅
- `CurrencyManager` implements dual currency. `TradingPost` uses Trust barrier ✅.

### Ship Interior Grid ✅
- `ShipGrid.cs` implements grid-based interior as per GDD Section 5.3. `GridCell` concept = `ShipCell`.

---

## 🚨 CRITICAL (2 Issues)

### C1. ResourceNode: Mined Resource Lost on Full Inventory
**File:** `ResourceNode.cs` L85-93
**Defect:** Planet resources are **FINITE** (node gets destroyed after harvest). If inventory is full, the mined resource is **permanently lost** — unlike crafting which can be retried.
```csharp
if (!added)
{
    Debug.LogWarning("Inventory full! Resource lost."); // PERMANENT LOSS
    // NOTE comment says "losing them is a gameplay risk" ← WRONG DESIGN
}
// + Node is then destroyed (L98: DestroySequence)
```
**Impact:** Player spends time mining a finite node, gets nothing. Extremely frustrating.
**Fix:** Drop item on ground as pickup OR block harvesting if inventory full.

### C2. RescueProtocol: SkipToMorning is a Stub
**File:** `RescueProtocol.cs` L154-163
**Defect:** `SkipToMorning()` does **nothing** — only logs. The `TODO: TimeManager.Instance.SetTime(wakeUpTime)` is unimplemented. Player wakes up at the wrong time after rescue.
```csharp
private void SkipToMorning()
{
    // TODO: TimeManager.Instance.SetTime(wakeUpTime) once method exists
    Debug.Log("Time skipped..."); // NOTHING ACTUALLY HAPPENS
}
```
**Impact:** Core rescue mechanic incomplete. Player dies and wakes up at night instead of morning.
**Fix:** Add `SetTime(float)` to `TimeManager`, call it in `SkipToMorning()`.

---

## ⚠️ HIGH (3 Issues)

### H1. PlayerVitals: ExplorationManager Event Leak
**File:** `PlayerVitals.cs` L68-76
**Defect:** Subscribes to `ExplorationManager.Instance.OnExplorationStarted` and `OnReturnedToShip` in `Start()`, but **NEVER unsubscribes** in `OnDestroy()`.
**Impact:** Memory leak + ghost callbacks after scene transitions.
**Fix:** Unsubscribe in `OnDestroy()`.

### H2. RescueProtocol: Same Event Leak
**File:** `RescueProtocol.cs` L48-53
**Defect:** Subscribes to `PlayerVitals.Instance.OnPlayerDeath` but never unsubscribes.
**Impact:** Same as H1.

### H3. ShipPlacementSystem: Camera.main Every Frame
**File:** `ShipPlacementSystem.cs` L59
**Defect:** `Camera.main` called every `Update()` frame during placement. This is a `FindObjectWithTag` call internally — expensive.
```csharp
Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
```
**Impact:** Performance hit during placement mode.
**Fix:** Cache `Camera.main` in `Start()` or `StartPlacement()`.

---

## 📋 MEDIUM (2 Issues)

### M1. ExplorationManager: Spawned Nodes Not Parented
**File:** `ExplorationManager.cs` L215
**Defect:** Resource nodes spawned via `Instantiate()` are placed at root level. On `ReturnToShip`, they're destroyed manually, but if the coroutine is interrupted, orphan nodes persist.
**Fix:** Parent to a container or to the planet scene root.

### M2. ShipGrid: No Size Validation on ExportSaveData
**File:** `ShipGrid.cs` L297-321
**Defect:** `ExportSaveData()` exposes raw grid data. No version tag or checksum for save compatibility. Minor for now, but future-proofing concern.

---

## ✅ GOOD PRACTICES FOUND

| Area | Details |
|:-----|:--------|
| **ShipGrid** | ✅ Bounds checking on all operations |
| **ShipGrid** | ✅ OnDestroy singleton cleanup |
| **ShipPlacementSystem** | ✅ Ghost preview with valid/invalid color |
| **ExplorationManager** | ✅ Additive scene loading (clean separation) |
| **ExplorationManager** | ✅ DontDestroyOnLoad (persists across scenes) |
| **PlayerVitals** | ✅ OnDestroy singleton cleanup |
| **PlanetData** | ✅ Well-structured SO with biome, hazard, resources |
| **RescueProtocol** | ✅ No game over — always rescued (GDD compliant) |

---

## 📊 SUMMARY TABLE

| ID | Severity | File | Issue |
|:---|:---------|:-----|:------|
| C1 | CRITICAL | ResourceNode.cs | Mined resource permanently lost on full inventory |
| C2 | CRITICAL | RescueProtocol.cs | SkipToMorning is a stub (TODO) |
| H1 | HIGH | PlayerVitals.cs | Event subscription leak |
| H2 | HIGH | RescueProtocol.cs | Event subscription leak |
| H3 | HIGH | ShipPlacementSystem.cs | Camera.main every frame |
| M1 | MEDIUM | ExplorationManager.cs | Spawned nodes unparented |
| M2 | MEDIUM | ShipGrid.cs | No save data versioning |

---

## 🛑 VERDICT: REJECTED
2 CRITICAL + 3 HIGH issues harus diperbaiki sebelum re-audit.
