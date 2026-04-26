# DEVELOPER FIX BRIEF: Sprint 4 Critical Issues

**To:** Lead Developer
**From:** CYGNUS QA (Zero Trust Audit)
**Priority:** P0 (Blocking Deployment)

---

## FIX 1: Crop Growth Logic (TICKET-010 / TICKET-013)
**File:** `Assets/Scripts/Systems/CropInstance.cs`
**Action:**
1. Remove `Update()` method entirely.
2. Subscribe to `TimeManager.OnDayChanged` in `Start()`.
3. In event handler:
```csharp
private void HandleDayChanged(int day)
{
    if (isWatered)
    {
        AdvanceStage();
        isWatered = false;
    }
}
```

## FIX 2: Inventory Transaction Integrity (TICKET-016)
**File:** `Assets/Scripts/Managers/InventoryManager.cs`
**Action:** Implement "Dry Run" pattern — calculate available space first, reject if insufficient.

## FIX 3: TimeManager Event Throttle
**File:** `Assets/Scripts/Managers/TimeManager.cs`
**Action:** Throttle `OnTimeChanged` to per-minute granularity.

## FIX 4: DayNightLightController → Event-Driven
**File:** `Assets/Scripts/Systems/DayNightLightController.cs`
**Action:** Subscribe to `TimeManager.OnTimeChanged`, remove `Update()` polling. Fix typo `UpdateLikght`.

## FIX 5: ShippingBin Exploit
**File:** `Assets/Scripts/Systems/ShippingBin.cs`
**Action:** Wrap `OnMouseDown` in `#if UNITY_EDITOR`.

---

## 🧪 VALIDATION CHECKLIST
- [ ] `Time.timeScale` changes must NOT affect crop growth speed.
- [ ] Spamming "Add Item" near full inventory must NOT lose items.
- [ ] Profiler must show 0 GC alloc from TimeManager in idle state.
