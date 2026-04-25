# 📜 SOP ARSITEKTUR BAKU — AstroPioneer V25+

**Dari:** Lead Engineer & Tim QA  
**Tanggal:** 2026-04-21  
**Status:** BERLAKU EFEKTIF SEGERA

---

> Setiap Pull Request atau commit yang melanggar salah satu dari 3 Hukum di bawah ini akan **DITOLAK OTOMATIS** dan dikembalikan untuk ditulis ulang.

---

## 📜 HUKUM 1: ATURAN "SATU OTAK" (No Puppet Brains)

Sistem menggunakan **Arsitektur Deterministik** (`SimulationMaster` Tick). GameObject visual **BUKAN** pengambil keputusan.

### ❌ DILARANG KERAS
Menggunakan `Update()`, `FixedUpdate()`, atau `Coroutine` di dalam skrip entitas visual (seperti `TransportBot.cs`) untuk:
- Menghitung jarak
- Logika inventaris  
- Memicu event gameplay

### ✅ YANG DIIZINKAN
Skrip visual **hanya boleh**:
- Menerima koordinat dari Manajer Data
- Menggunakan `Update()` **murni hanya** untuk interpolasi visual (`Lerp`) dan animasi

### Contoh Benar
```csharp
// TransportBot.cs — Visual Chaser
void Update()
{
    if (data == null) return;
    transform.position = Vector3.Lerp(transform.position, (Vector3)data.currentPos, Time.deltaTime * 10f);
    SyncVisuals(); // Hanya set animator bool
}
```

### Contoh SALAH
```csharp
// ❌ JANGAN LAKUKAN INI
void Update()
{
    float dist = Vector2.Distance(transform.position, targetMachine.position);
    if (dist < 1f) targetMachine.AddInventory(heldItem); // PUPPET BRAIN!
}
```

---

## 📜 HUKUM 2: KEMURNIAN LAPISAN DATA (Zero Reference Pollution)

Otak simulasi (Data Layer) harus bisa berjalan **di layar hitam** tanpa render visual.

### ❌ DILARANG KERAS
Menyimpan referensi fisik di dalam Manajer atau Data Struct:
- `GameObject`
- `Transform`
- `SpriteRenderer`
- `MonoBehaviour` (komponen Unity)

### ✅ YANG DIIZINKAN
Manajer hanya boleh memegang:
- **Angka** (`int`, `float`, `ushort`)
- **ID** (`string`)
- **Koordinat Grid** (`Vector2`, `Vector2Int`)
- Hubungan ke visual dilakukan murni lewat **ID Binding**

### Contoh Benar
```csharp
// BotData — Data Struct Murni
public class BotData
{
    public string id;
    public int entityTypeID;
    public Vector2 currentPos;
    public Vector2 sourcePos;
    public Vector2 targetPos;
    public ushort heldItemID;
    // TIDAK ADA GameObject, Transform, atau MonoBehaviour
}
```

### Contoh SALAH
```csharp
// ❌ JANGAN LAKUKAN INI
public class BotData
{
    public Transform targetTransform;     // POLUSI!
    public MachineStorage storageRef;     // POLUSI!
    public GameObject visualBody;         // POLUSI!
}
```

---

## 📜 HUKUM 3: NAVIGASI MURNI DATA (No Physics for Logistics)

Pathfinding dan deteksi mesin sudah **murni Data-Driven**. Fisika Unity hanya untuk gameplay (collision player, projectile).

### ❌ DILARANG KERAS
Menggunakan fungsi fisika untuk logistik bot/mesin:
- `OnTriggerEnter` / `OnCollisionEnter`
- `Physics2D.Raycast`
- `Physics2D.OverlapBox` / `OverlapCircle`

### ✅ YANG DIIZINKAN
- Bot mendeteksi kedatangan: **membandingkan koordinat data** (`Vector2.Distance(bot.currentPos, targetPos)`)
- Deteksi mesin: **`GridManager.GetStructureAt(pos)`** → **`StructureRegistry.Get(id)`**
- Deteksi halangan: **`GridManager.IsSolidAt(pos)`**

### Contoh Benar
```csharp
// BotStation.cs — Data-Driven Machine Detection
private void ClassifyMachineAt(Vector2Int pos)
{
    ushort structID = GridManager.Instance.GetStructureAt(pos);
    StructureData data = StructureRegistry.Instance.Get(structID);
    if (data == null || data.category != StructureCategory.Machine) return;
    
    // Cek PREFAB ASSET (selalu di memori), bukan scene instance
    if (data.visualPrefab.GetComponent<MachineWaterPump>() != null)
        foundSources.Add(pos);  // Simpan koordinat, BUKAN referensi
}
```

### Contoh SALAH
```csharp
// ❌ JANGAN LAKUKAN INI
private void FindMachines()
{
    int hits = Physics2D.OverlapCircleNonAlloc(transform.position, radius, buffer);
    for (int i = 0; i < hits; i++)
        if (buffer[i].GetComponent<MachineStorage>()) // FISIKA UNTUK LOGISTIK!
            targets.Add(buffer[i].GetComponent<MachineStorage>());
}
```

---

## 🏗️ Arsitektur Referensi

```
┌─────────────────────────────────────────────────┐
│              SimulationMaster (Tick)             │
│         Detak jantung global (0.2 detik)        │
└──────────┬──────────┬──────────┬────────────────┘
           │          │          │
     Priority 0  Priority 50  Priority 100
           │          │          │
    ┌──────▼──┐ ┌─────▼────┐ ┌──▼──────────────┐
    │ Power   │ │  Fluid   │ │ BotSimulation   │
    │ System  │ │  System  │ │ Manager         │
    │ (Future)│ │ (Future) │ │ (Data Layer)    │
    └─────────┘ └──────────┘ └────────┬────────┘
                                      │
                              ┌───────▼────────┐
                              │  BotData (RAM) │
                              │  - id, pos     │
                              │  - state, path │
                              │  - heldItemID  │
                              └───────┬────────┘
                                      │ ID Binding
                              ┌───────▼────────┐
                              │  TransportBot  │
                              │  (Visual Only) │
                              │  Lerp + Anim   │
                              └────────────────┘
```

---

## ⚠️ Jika Bingung

> Jangan ambil jalan pintas. Datanglah ke meja Lead Engineer, dan kita akan gambar alur datanya bersama-sama di papan tulis.
