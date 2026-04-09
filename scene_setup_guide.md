# 🏗️ Panduan Scene Cleanup & Player Setup

**Tujuan:** Reorganisasi `SampleScene` dari testing scene menjadi playable scene.  
**Estimasi waktu:** ~15 menit

---

## FASE 1: Reorganisasi Hierarchy (5 menit)

Kita akan grouping objek menggunakan **Empty GameObject sebagai folder**. Ini tidak mengubah fungsionalitas, hanya merapikan.

### Langkah:

1. **Klik kanan di Hierarchy** → Create Empty → Rename jadi `--- MANAGERS ---`
2. **Drag & drop** objek-objek berikut ke dalam `--- MANAGERS ---`:
   - `GridManager`
   - `CropManager`
   - `BotManager`
   - `PathfindingManager`
   - `EventSystem`
   - `Managers` (yang berisi TimeManager, CurrencyManager, dll)

3. **Buat Empty GO lagi** → Rename jadi `--- MACHINES ---`
4. **Drag & drop**:
   - `Machine_Generator`
   - `Machine_Battery`
   - `Machine_WaterPump`
   - `Machine_Sprinkler`
   - `Machine_SmallStorage`
   - `Structure_ShippingBin`

5. **Buat Empty GO lagi** → Rename jadi `--- UI ---`
6. **Drag & drop**:
   - `Canvas`
   - `WorldCursor`
   - `MouseInteractionSystem`

7. **Buat Empty GO lagi** → Rename jadi `--- BOTS ---`
8. **Drag & drop**:
   - `TransportBot_E`

9. **Buat Empty GO lagi** → Rename jadi `--- DEBUG ---`
10. **Drag & drop**:
    - `Wall_Test`
    - `Debug_Tester`

> [!TIP]
> Untuk group separator, tambahkan `---` di awal nama agar mudah dilihat. Pastikan Transform position semua folder separator tetap **(0, 0, 0)** agar tidak menggeser child objects.

---

## FASE 2: Buat Player GameObject (5 menit)

### Langkah:

1. **Klik kanan di Hierarchy** → Create Empty → Rename jadi **`Player`**
2. **Set Tag** = `Player` (KRITIS — banyak script yang mencari tag ini)
   - Di Inspector, klik dropdown Tag → pilih `Player`
   - Jika tag "Player" belum ada, klik "Add Tag" → tambahkan
3. **Set Position** = `(0, 0, 0)` atau di tengah grid Anda

### Tambahkan Components (melalui Add Component di Inspector):

| Urutan | Component | Cara Setup |
|:---:|:---|:---|
| 1 | **SpriteRenderer** | Assign sprite placeholder (kotak/lingkaran). Set Sorting Layer = "Default", Order = **10** (agar di atas terrain). Set Color = **hijau terang** untuk mudah dilihat. |
| 2 | **Rigidbody2D** | Otomatis ditambahkan saat Add `PlayerMovement`. Body Type = **Kinematic**, Freeze Rotation Z = ✅ |
| 3 | **BoxCollider2D** | Size sesuai sprite. Untuk sementara: (0.8, 0.8) |
| 4 | **PlayerMovement** | Add Component → cari "PlayerMovement". Default moveSpeed = 5. |
| 5 | **PlayerVitals** | Add Component → cari "PlayerVitals". Default sudah benar. |
| 6 | **ExplorationTracker** | Add Component → cari "ExplorationTracker". Default radius = 1 (3x3). |

### Pindahkan PlayerToolState:

7. **PENTING**: `PlayerToolState` saat ini adalah **standalone object** di hierarchy.
   - **Pilih** objek `PlayerToolState` di hierarchy.
   - **Copy semua nilai** dari Inspector (catat referensi CropData, VFX, dll)
   - **Hapus** objek `PlayerToolState` dari hierarchy.
   - **Klik Player** → Add Component → cari "PlayerToolState"
   - **Paste kembali** semua referensi yang tadi dicatat.

> [!WARNING]
> **JANGAN langsung drag PlayerToolState ke dalam Player sebagai child!** Yang kita inginkan adalah script-nya di-attach langsung ke Player, bukan sebagai child object. 
> Pilihan lain: jika terlalu repot mindahin referensi, biarkan PlayerToolState sebagai child dari Player. Ini juga berfungsi karena script-nya Singleton.

---

## FASE 3: Setup Camera Follow (2 menit)

### Langkah:

1. **Klik Main Camera** di hierarchy.
2. **Add Component** → cari "CameraFollow"
3. Di field **Target** → drag objek **Player** dari hierarchy.
4. **Settings default**:
   - Smooth Speed = `8`
   - Offset = `(0, 0, -10)` ← penting! Z = -10 agar kamera di depan scene 2D.

> [!TIP]
> Jika Anda lupa assign Target, script CameraFollow otomatis mencari object dengan tag "Player". Tapi lebih baik assign manual agar tidak ada delay di frame pertama.

---

## FASE 4: Buat SleepPod (2 menit)

### Langkah:

1. **Klik kanan di `--- MACHINES ---`** → Create Empty → Rename jadi **`SleepPod`**
2. **Set Position** = Dekat area "base/kapal" (misalnya `(-2, 0, 0)`)
3. **Add Components**:
   - `SpriteRenderer` → Assign placeholder sprite. Color = **biru** agar beda.
   - `BoxCollider2D` → Size = `(1, 1)`
   - `SleepPod` → Add Component → cari "SleepPod"
4. SleepPod settings default sudah benar:
   - Sleep Duration = 2.0
   - Interaction Distance = 2.5

---

## FASE 5: Verify & Play (1 menit)

### Checklist Sebelum Play:

- [ ] Player ada di scene, tag = **"Player"**
- [ ] Player punya: `SpriteRenderer`, `Rigidbody2D`, `PlayerMovement`, `PlayerVitals`, `ExplorationTracker`
- [ ] Main Camera punya `CameraFollow`, target = Player
- [ ] SleepPod ada di scene dengan `BoxCollider2D` + `SleepPod` script
- [ ] Console: **TIDAK ADA error merah**
- [ ] Hierarchy sudah dikelompokkan rapi

### Test Play:

1. Tekan **Play** ▶️
2. Tekan **WASD** → Player bergerak
3. Kamera mengikuti player secara smooth
4. Tekan **[M]** → Tablet map terbuka
5. Tekan **[1]-[6]** → Hotbar selection berfungsi (cek Console log)

Jika semua ✅, scene siap untuk testing penuh fitur-fitur sprint 8!

---

## Hierarchy Akhir yang Diharapkan

```
📁 SampleScene
├── 🎥 Main Camera              [CameraFollow]
├── 💡 Global Light 2D
├── 💡 Light 2D
│
├── 📁 --- MANAGERS ---
│   ├── GridManager
│   ├── CropManager
│   ├── BotManager
│   ├── PathfindingManager
│   ├── EventSystem
│   └── Managers
│
├── 👤 Player                    [PlayerMovement, PlayerToolState, 
│                                 PlayerVitals, ExplorationTracker,
│                                 SpriteRenderer, Rigidbody2D, BoxCollider2D]
│
├── 📁 --- MACHINES ---
│   ├── Machine_Generator
│   ├── Machine_Battery
│   ├── Machine_WaterPump
│   ├── Machine_Sprinkler
│   ├── Machine_SmallStorage
│   ├── Structure_ShippingBin
│   └── SleepPod                 [SleepPod, SpriteRenderer, BoxCollider2D]
│
├── 📁 --- UI ---
│   ├── Canvas
│   ├── WorldCursor
│   └── MouseInteractionSystem
│
├── 📁 --- BOTS ---
│   └── TransportBot_E
│
└── 📁 --- DEBUG ---
    ├── Wall_Test
    └── Debug_Tester
```

---
*Panduan ini dibuat oleh Dev Agent (Antigravity).*
