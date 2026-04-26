# DEVELOPER FIX BRIEF: Sprint 6 Critical Issues

**To:** Lead Developer
**From:** CYGNUS QA (Zero Trust Audit)
**Priority:** P0

---

## FIX 1: CraftingManager — Product Loss (C1)
**File:** `Assets/Scripts/Managers/CraftingManager.cs`

### Tambah output buffer saat inventory penuh (L170-183):
```csharp
// Replace the "dropped" warning with buffer logic:
if (!added)
{
    // HOLD: Store in a "pending pickup" list
    pendingPickups.Add(new PendingItem { item = currentlyCrafting.resultItem, quantity = currentlyCrafting.resultQuantity });
    Debug.LogWarning($"[Crafting] Inventory full! Item held for pickup: {currentlyCrafting.resultItem.displayName}");
    // OR: Retry adding on next frame until successful
}
```
**Minimal fix** (tanpa buffer): Cek space di inventory SEBELUM consume ingredients.
```csharp
// Tambahkan di StartCraft, sebelum consume:
if (!InventoryManager.Instance.CanFit(recipe.resultItem, recipe.resultQuantity))
{
    Debug.LogWarning("[Crafting] No space for result! Craft rejected.");
    OnCraftFailed?.Invoke(recipe);
    return false;
}
```

---

## FIX 2: ProcessingStation — Same Pattern (C2)
**File:** `Assets/Scripts/Machines/ProcessingStation.cs`

Apply fix yang sama seperti C1. Gunakan buffer atau pre-check space.

---

## FIX 3: TradingPost — Day Price Sync (H1)
**File:** `Assets/Scripts/Systems/TradingPost.cs`

### Subscribe ke TimeManager events:
```csharp
void Start()
{
    RefreshPrices();
    if (TimeManager.Instance != null)
        TimeManager.Instance.OnDayChanged += OnNewDay;
}

void OnDestroy()
{
    if (TimeManager.Instance != null)
        TimeManager.Instance.OnDayChanged -= OnNewDay;
}
```

---

## FIX 4: ShippingBin — Remove Items (M1)
**File:** `Assets/Scripts/Systems/ShippingBin.cs`

### Tambahkan RemoveItem di SellItem():
```csharp
public void SellItem(InventoryItem item, int quantity)
{
    // ... existing price calc ...
    
    // Remove from inventory FIRST
    if (!InventoryManager.Instance.RemoveItem(item, quantity))
    {
        Debug.LogWarning("[ShippingBin] Not enough items.");
        return;
    }
    
    CurrencyManager.Instance.AddCredits(totalValue);
}
```

---

## 🧪 VALIDATION CHECKLIST
- [ ] Craft item saat inventory penuh → item TIDAK hilang (buffered/rejected).
- [ ] Processing saat inventory penuh → item TIDAK hilang.
- [ ] Harga di Trading Post berubah setiap hari.
- [ ] Sell via ShippingBin → item berkurang dari inventory.
