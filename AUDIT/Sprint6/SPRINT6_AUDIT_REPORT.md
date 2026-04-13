# SPRINT 6 AUDIT REPORT - ZERO TRUST

**Date:** 2026-03-01
**Auditor:** CYGNUS v2.0
**Scope:** Sprint 6 - Crafting & Trading (TICKET-025 s/d TICKET-030)
**Status:** ❌ FAILED (6 Issues Found)

---

## 🚨 CRITICAL (2 Issues)

### C1. CraftingManager: Crafted Item Loss When Inventory Full
**File:** `CraftingManager.cs` L170-183
**Defect:** Ingredients are consumed IMMEDIATELY upon `StartCraft()` (L127-131). But when crafting completes and inventory is full, the result is **silently dropped** with only a warning log.
```csharp
// L127-131: Ingredients consumed NOW
foreach (var ingredient in recipe.ingredients)
    InventoryManager.Instance.RemoveItem(ingredient.item, ingredient.quantity);

// L179-183: Later when craft finishes...
else
{
    Debug.LogWarning("Inventory full! Item dropped."); // ITEM LOST FOREVER
    // TODO: Drop item on ground or hold in buffer ← NOT IMPLEMENTED
}
```
**Impact:** Player loses ingredients + crafted product. No recovery mechanism. Extremely frustrating gameplay.
**Fix:** Hold completed item in a buffer/pickup until claimed. Or prevent starting craft if inventory will be full (pre-check).

---

### C2. ProcessingStation: Same Product Loss Pattern
**File:** `ProcessingStation.cs` L96-106
**Defect:** Identical problem to C1. Ingredients consumed by caller before `StartProcessing()`, but result silently lost if inventory full on completion.
```csharp
Debug.LogWarning("Inventory full! Product lost: ..."); // LOST!
```
**Impact:** Player loses both ingredients AND processing time. No rollback of any kind.
**Fix:** Same as C1 — buffer/hold pattern.

---

## ⚠️ HIGH (2 Issues)

### H1. TradingPost: Day Price Update Not Subscribed
**File:** `TradingPost.cs` L33-36, L41-45
**Defect:** `OnNewDay(int day)` exists but is NEVER subscribed to `TimeManager.OnDayChanged`. Prices only refresh once at `Start()` and never change.
```csharp
void Start()
{
    RefreshPrices(); // Only called ONCE
}
// OnNewDay(int day) exists but nothing calls it!
```
**Impact:** Dynamic pricing (core feature) is non-functional. Prices are static, defeating the market mechanic.
**Fix:** Subscribe to `TimeManager.OnDayChanged` in `Start()`, unsubscribe in `OnDestroy()`.

### H2. CraftingManager: Ingredient Race Condition
**File:** `CraftingManager.cs` L108-146
**Defect:** `CanCraft()` check (L110) and ingredient consumption (L127-131) are NOT atomic. Between the check and the consumption, another system could modify inventory (e.g. bot depositing/removing, trading). The `RemoveItem` calls happen without re-validating availability.
**Impact:** Edge case: ingredients could be partially consumed if inventory changes between check and remove. Low probability but dangerous.
**Fix:** Use a single atomic "check-and-remove" pass, or lock inventory during the transaction.

---

## 📋 MEDIUM (2 Issues)

### M1. ShippingBin: SellItem Doesn't Remove Items
**File:** `ShippingBin.cs` L15-37
**Defect:** `SellItem()` adds credits but does NOT call `InventoryManager.RemoveItem()`. The caller is expected to remove items, but this isn't enforced or documented.
**Impact:** If any caller forgets to remove items before calling `SellItem()`, it becomes infinite money.
**Fix:** Either remove items inside `SellItem()` or add precondition assert.

### M2. CraftingUI: Event Race on Start
**File:** `CraftingUI.cs` L46-51
**Defect:** `CraftingUI.Start()` subscribes to `CraftingManager.Instance` events, but if CraftingManager initializes AFTER CraftingUI (Awake order), Instance is null and events are never subscribed.
**Impact:** UI shows stale data / never updates.
**Fix:** Use `Awake`/`Start` order guarantee or lazy subscription with null check in `Update`.

---

## ✅ GOOD PRACTICES FOUND

| Area | Details |
|:-----|:--------|
| **TradingPost.BuyItem** | ✅ Remove-first + rollback pattern (L98-110) |
| **TradingPost.SellItem** | ✅ Remove-first pattern (L147-152) |
| **CurrencyManager** | ✅ OnDestroy clears Instance, event-driven UI |
| **EconomyUI** | ✅ Event subscription/unsubscription properly paired |
| **CraftingRecipe** | ✅ OnValidate guards for negative values |
| **ProcessingStation** | ✅ IPowerConsumer integrated, pauses on power loss |

---

## 📊 SUMMARY TABLE

| ID | Severity | File | Issue |
|:---|:---------|:-----|:------|
| C1 | CRITICAL | CraftingManager.cs | Crafted item lost on full inventory |
| C2 | CRITICAL | ProcessingStation.cs | Processed item lost on full inventory |
| H1 | HIGH | TradingPost.cs | Daily price update never triggered |
| H2 | HIGH | CraftingManager.cs | Ingredient check-consume race condition |
| M1 | MEDIUM | ShippingBin.cs | SellItem doesn't remove items |
| M2 | MEDIUM | CraftingUI.cs | Event subscription timing |

---

## 🛑 VERDICT: REJECTED
2 CRITICAL issues harus diperbaiki sebelum re-audit.
