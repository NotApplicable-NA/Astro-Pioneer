# DEVELOPER FIX BRIEF: GDD 3.5 + Sprint 7 Issues

**To:** Lead Developer
**From:** CYGNUS QA (Zero Trust Audit)
**Priority:** P0

---

## FIX 1: ResourceNode — Drop on Ground (C1)
**File:** `Assets/Scripts/Systems/Exploration/ResourceNode.cs`

### Block harvest if inventory full, or drop as world pickup:
```csharp
// In Harvest(), replace the "lost" branch:
if (!added)
{
    // Option A: Block harvest (simpler for MVP)
    isDepleted = false; // Un-deplete — let player try again
    currentHP = 1;      // Restore 1 HP so it doesn't re-harvest
    Debug.LogWarning($"[ResourceNode] Inventory full! Node preserved.");
    return; // DON'T destroy
}
```

---

## FIX 2: RescueProtocol — SkipToMorning (C2)
**File:** `Assets/Scripts/Systems/Survival/RescueProtocol.cs`
**Also:** `Assets/Scripts/Managers/TimeManager.cs`

### Add SetTime to TimeManager:
```csharp
// In TimeManager.cs, add:
public void SetNormalizedTime(float t)
{
    currentTimeOfDay = Mathf.Clamp01(t);
    OnMinuteChanged?.Invoke(CurrentMinute);
}
```

### Use it in RescueProtocol:
```csharp
private void SkipToMorning()
{
    if (TimeManager.Instance != null)
        TimeManager.Instance.SetNormalizedTime(wakeUpTime);
}
```

---

## FIX 3: Event Leaks (H1 + H2)
**File:** `PlayerVitals.cs` — Add to `OnDestroy()`:
```csharp
if (ExplorationManager.Instance != null)
{
    ExplorationManager.Instance.OnExplorationStarted -= OnEnterPlanet;
    ExplorationManager.Instance.OnReturnedToShip -= OnLeaveplanet;
}
```

**File:** `RescueProtocol.cs` — Add to `OnDestroy()`:
```csharp
if (PlayerVitals.Instance != null)
    PlayerVitals.Instance.OnPlayerDeath -= TriggerRescue;
```

---

## FIX 4: Camera.main Cache (H3)
**File:** `Assets/Scripts/Systems/Ship/ShipPlacementSystem.cs`

### Cache camera reference:
```csharp
private Camera cachedCamera;

void Start()
{
    cachedCamera = Camera.main;
}

// In Update(), replace Camera.main with cachedCamera:
Vector3 mouseWorld = cachedCamera.ScreenToWorldPoint(Input.mousePosition);
```

---

## 🧪 VALIDATION CHECKLIST
- [ ] Mine resource saat inventory penuh → resource TIDAK hilang (node preserved).
- [ ] Mati di planet → waktu skip ke pagi (wakeUpTime).
- [ ] Scene transition → no ghost event callbacks.
- [ ] Ship placement → no Camera.main calls in profiler.
