# DEVELOPER FIX BRIEF: Sprint 5 Critical Issues

**To:** Lead Developer
**From:** CYGNUS QA (Zero Trust Audit)
**Priority:** P0

---

## FIX 1: TransportBot Item Leak (C1)
**File:** `Assets/Scripts/Machines/TransportBot.cs`

### A. Abort jika pickup gagal (Line ~137):
```csharp
if (!pickupSuccess)
{
    Debug.LogWarning("[Transport] Pickup FAILED. Aborting task.");
    ResetState();
    yield break; // ← TAMBAHKAN
}
```

### B. Clear item data di ResetState():
```csharp
private void ResetState()
{
    currentState = TransportState.Idle;
    hasTask = false;
    heldItem = null; // ← TAMBAHKAN
}
```

---

## FIX 2: MachineStorage Stack Overflow (C2)
**File:** `Assets/Scripts/Machines/MachineStorage.cs`

### Clamp quantity saat add ke empty slot (Line ~67):
```csharp
slot.quantity = Mathf.Min(amount, maxStackSize); // ← CLAMP
```

---

## FIX 3: StorageUI Drag-Drop Duplication (C3)
**File:** `Assets/Scripts/UI/StorageUI.cs`

### Gunakan Remove-first pattern (Line ~130):
```csharp
InventoryManager.Instance.RemoveItem(invSlot.item, qty);
if (!currentStorage.TryAddItem(invSlot.item, qty))
{
    InventoryManager.Instance.AddItem(invSlot.item, qty); // Rollback
    Debug.LogWarning("[StorageUI] Storage full, rollback.");
}
```

---

## FIX 4: BotManager Singleton (H3)
**File:** `Assets/Scripts/Managers/BotManager.cs`
```csharp
void OnDestroy()
{
    if (Instance == this) Instance = null;
}
```

---

## 🧪 VALIDATION CHECKLIST
- [ ] Bot gagal pickup → kembali Idle tanpa item.
- [ ] Item > maxStackSize ke Storage → di-clamp/reject.
- [ ] Drag ke Storage penuh → item tetap di Inventory.
- [ ] Scene reload → tidak NullReferenceException.
