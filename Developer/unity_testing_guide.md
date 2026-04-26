# 🧪 Unity Testing Guide — Sprint 8 Features

**Date:** 2026-04-05  
**Tester:** Developer / QA  
**Scene:** `SampleScene.unity`

> [!IMPORTANT]
> Sebelum memulai testing, pastikan Anda sudah **membuka project di Unity** dan tidak ada **compile error** di Console. Jika ada error, selesaikan dulu sebelum melanjutkan.

---

## 📋 Pre-Test Setup (Wajib)

### Langkah 0: Buka Project & Compile Check
1. Buka Unity Hub → Buka project **Astro-Pioneer**.
2. Buka **Console** (`Window > General > Console`). 
3. Pastikan **TIDAK ADA** error merah. Jika ada, cek script mana yang bermasalah.
4. Buka scene utama: `Assets/Scenes/SampleScene.unity`.

### Langkah 0.1: Pastikan Komponen Baru Sudah Terpasang
Beberapa script **baru** perlu dipasang secara manual di scene:

| Script | Pasang Di Mana | Cara |
|:---|:---|:---|
| `ExplorationTracker.cs` | **Player GameObject** (yang ber-tag `Player`) | Klik Player > Add Component > cari "ExplorationTracker" |
| `SleepPod.cs` | **Klik kanan `--- MACHINES ---`** > Create Empty bernama "SleepPod" | Add Component `SleepPod`. WAJIB tambah **BoxCollider2D** dan **SpriteRenderer** agar bisa diklik. |
| `ShadowZone.cs` | **Klik kanan `--- DEBUG ---`** > Create Empty bernama "ShadowZone_Test" | Add Component `ShadowZone`. Letakkan posisinya di atas grid tanah. |
| `UVLightPillar.cs` | **Klik kanan `--- MACHINES ---`** > Create Empty bernama "UVLightPillar_Test" | Add Component `UVLightPillar`. Letakkan di dalam atau di dekat Shadow Zone. |

> [!TIP]
> Untuk SleepPod, pastikan sudah ada **BoxCollider2D** agar `OnMouseDown()` bisa di-trigger. Anda juga perlu **SpriteRenderer** (pakai placeholder sprite apapun) agar bisa diklik di Game View.

---

## ✅ TEST 1: Siklus Waktu 15 Menit (TimeManager)

**Tujuan:** Memastikan 1 hari in-game = 15 menit real-time.

### Langkah:
1. Tekan **Play**.
2. Buka **Inspector** pada GameObject yang memiliki `TimeManager`.
3. Perhatikan field `Current Time` (0.0 = Tengah Malam, 0.25 = 06:00, 0.5 = Siang, 0.75 = 18:00).
4. Perhatikan field `Days Passed`.

### Yang Diverifikasi:
- [✅] `Current Time` bergerak perlahan dari kiri ke kanan (0.0 → 1.0).
- [✅] Saat `Current Time` melewati 1.0, ia kembali ke 0.0 dan `Days Passed` bertambah 1.
- [✅] Di Console, muncul log: `[TimeManager] Day X Started!`

### Shortcut Cepat:
Jika tidak mau menunggu 15 menit, ubah `realSecondsPerDay` di Inspector menjadi **30** (30 detik per hari) untuk testing cepat. **Kembalikan ke 900 setelah selesai.**

---

## ✅ TEST 2: Inventory 8x8 dan Stack 64

**Tujuan:** Memastikan inventory 64 slot dan max stack 64.

### Langkah:
1. Tekan **Play**.
2. Buka Inspector pada `InventoryManager`.
3. Periksa field `slotCount` = **64**.
4. Buka ScriptableObject dari salah satu `InventoryItem` (misalnya Space Potato).
5. Periksa field `maxStackSize` = **64**.

### Yang Diverifikasi:
- [✅] Total slot = 64.
- [✅] Max stack per slot = 64.

---

## ✅ TEST 3: Holographic Tablet & Fog of War

**Tujuan:** Memastikan tablet bisa dibuka/tutup dan menampilkan Fog of War.

### Pre-Requisite:
- `ExplorationTracker` sudah terpasang di **Player**.
- `HolographicTabletUI` sudah terpasang dan `mapIconPrefab` sudah di-assign (jika belum, buat simple UI Image prefab).

### Langkah:
1. Tekan **Play**.
2. Tekan **[M]** untuk membuka tablet.
3. Perhatikan peta di dalam tablet.

### Yang Diverifikasi:
- [✅] Tekan **[M]** → Tablet muncul.
- [✅] Tekan **[M]** lagi → Tablet tertutup.
- [✅] Saat pertama kali dibuka: sebagian besar sel peta berwarna **gelap (Fog)**.
- [✅] Ikon Player (kuning) terlihat di peta dan bergerak sesuai posisi pemain.
- [✅] Tutup tablet, jalan beberapa langkah, buka lagi → ada jejak sel yang sudah terbuka.
- [✅] Ikon Mesin/Tanaman **TIDAK** terlihat di area yang masih gelap (Fog).

### Fog of War Persistence Test:
1. Berjalan dan buka tablet — catat berapa % area sudah terbuka.
2. Simulasikan "pindah lokasi" (kalau ada sistem planet switching) atau restart play mode.
3. ⚠️ **CATATAN**: Persistence hanya aktif selama satu sesi Play Mode. Data FoW hilang saat Stop karena belum ada save/load system.

---

## ✅ TEST 4: Fatigue Scaling Debuff

**Tujuan:** Memastikan fatigue meningkat setiap hari dan mempercepat drain O2.

### Langkah:
1. Tekan **Play**.
2. Buka Inspector pada **PlayerVitals**.
3. Perhatikan field:
   - `daysSinceLastSleep` (harus mulai dari **1**)
   - `Fatigue Scaling Per Day` (harus **0.25**)

### Test A — Day 1 (Normal):
4. **Wajib Tes Manual:** Di Inspector `PlayerVitals`, **centang kotak `Is On Planet`**. (Karena kita di SampleScene, auto-trigger planet tidak jalan).
5. Perhatikan kecepatan drain O2 di HUD.
6. Di Console, perhatikan drain per frame (Log: `[PlayerVitals] Oxygen Drain: ...`).

### Test B — Day 2 (Percepatan):
7. **Shortcut**: Di Inspector TimeManager, ubah `realSecondsPerDay` ke **10** agar cepat.
8. Tunggu sampai `Days Passed` bertambah.
9. Cek Console: `[PlayerVitals] New day started. Days without sleep: 2. Multiplier: 1.25x`
10. **Berjalan lagi** di area baru (Exploration) — O2 harus drain **25% lebih cepat**.

### Test C — HUD Warning:
11. Saat `daysSinceLastSleep >= 2`, periksa HUD.
12. Harus muncul teks kuning: **"FATIGUED: 1.2x Drain"** (atau 1.5x, tergantung hari).

### Yang Diverifikasi:
- [✅] Day 1: Multiplier = 1.0x (normal).
- [✅] Day 2: Multiplier = 1.25x. Console log benar.
- [✅] Day 3: Multiplier = 1.5x.
- [✅] HUD menampilkan warning "FATIGUED" saat multiplier > 1.0.
- [✅] HUD warning **HILANG** saat multiplier = 1.0 (setelah tidur).

---

## ✅ TEST 5: Sleep Pod (Tidur & Reset Fatigue)

**Tujuan:** Memastikan Sleep Pod mereset fatigue dan memajukan waktu ke 06:00.

### Pre-Requisite:
- GameObject "SleepPod" sudah ada di scene dengan:
  - `SleepPod.cs` component
  - `BoxCollider2D`
  - `SpriteRenderer` (placeholder sprite)

### Langkah:
1. Ubah `realSecondsPerDay` ke **10** untuk percepatan.
2. Tekan **Play**.
3. Tunggu sampai `daysSinceLastSleep` = 2 atau lebih.
4. Perhatikan Console: multiplier sudah > 1.0.
5. Dekati Sleep Pod (player harus dalam radius **2.5 unit**).
6. **Klik** pada Sleep Pod.

### Yang Diverifikasi:
- [✅] Console: `[SleepPod] Going to sleep... Zzz...`
- [✅] Setelah ~2 detik → Console: `[SleepPod] Woke up! Fatigue reset to Day 1.`
- [✅] Inspector PlayerVitals: `daysSinceLastSleep` kembali ke **1**.
- [✅] Inspector TimeManager: `Current Time` sekarang = **0.25** (06:00 AM).
- [✅] HUD: Warning "FATIGUED" **HILANG**.
- [✅] O2 terisi penuh (100%).

### Edge Case:
- [✅] Klik Sleep Pod dari **jauh** (> 2.5 unit) → Console: `[SleepPod] Too far away` dan TIDAK ada efek.

---

## ✅ TEST 6: Rescue Protocol (O2 = 0)

**Tujuan:** Memastikan pemain tidak mati dan di-teleport ke Sleep Pod tanpa penalti.

### Langkah:
1. Tekan **Play**.
2. **Berjalan menjauh** dari area starting (Trigger Exploration).
3. **Shortcut**: Di Inspector PlayerVitals, set `currentOxygen` ke **5** secara manual.
4. Tunggu O2 habis (beberapa detik).

### Yang Diverifikasi:
- [✅] Console: `[PlayerVitals] O2 depleted! Player incapacitated.`
- [✅] Console: `[Rescue] O2 depleted! Bot-E is coming to rescue...`
- [✅] Setelah ~3 detik: Console: `[Rescue] Player teleported to SleepPod at (X,Y,Z)`
- [✅] Player berpindah posisi ke lokasi Sleep Pod.
- [✅] Console: `[Rescue] Player woke up on ship. Walk of Shame (No Penalty) begins.`
- [✅] **PENTING — Cek Zero Penalty:**
  - [✅] Buka Inspector `CurrencyManager` → Credits dan Trust **TIDAK BERUBAH**.
  - [✅] Buka Inspector `InventoryManager` → Isi inventory **TIDAK BERUBAH**.
  - [✅] Days Passed **TIDAK bertambah** (tidak ada time skip pada rescue).

---

## ✅ TEST 7: Dual Currency (Shipping Bin → Trust, Trading Post → Credits)

**Tujuan:** Memastikan Shipping Bin memberikan Trust dan Trading Post memberikan Credits.

### Pre-Requisite:
- Ada item di inventory yang punya `sellPrice > 0`.

### Test A — Shipping Bin:
1. Catat nilai **Trust** dan **Credits** saat ini di `CurrencyManager` Inspector.
2. Panggil `ShippingBin.SellItem()` via **Debug console** atau custom test button.
   - Atau: klik Shipping Bin jika sudah ada interaksi.
3. Cek Console: `[ShippingBin] Shipped Xx ItemName for Y Trust`

### Yang Diverifikasi (Shipping Bin):
- [✅] **Trust** bertambah sesuai `sellPrice` item.
- [✅] **Credits** tetap **TIDAK berubah**.

### Test B — Trading Post:
1. Catat nilai **Trust** dan **Credits** saat ini.
2. Jual item melalui Trading Post.
3. Cek Console: `[TradingPost] SOLD Xx ItemName for Ycr.`

### Yang Diverifikasi (Trading Post):
- [✅] **Credits** bertambah sesuai harga jual.
- [✅] **Trust** tetap **TIDAK berubah**.

---

## ✅ TEST 8: Shadow Canyons & UV Light Pillar

**Tujuan:** Memastikan tanaman tidak tumbuh di area gelap kecuali ada UV Pillar.

### Setup di Scene:
1. **Klik kanan pada folder `--- DEBUG ---` di Hierarchy**, pilih Create Empty.
2. Rename menjadi **"ShadowZone_Test"**.
3. Add Component: `ShadowZone`.
4. Di Inspector:
   - `Use Current Position` = **true**
   - `Shadow Size` = **(3, 3)** (area grid 3x3)
5. Pindahkan posisi "ShadowZone_Test" menggunakan Move Tool (`W`) ke area tanah yang kosong.

### Test A — Growth Block (Tanpa UV):
1. Tekan **Play**.
2. Tanam tanaman (Space Potato) **di dalam** area Shadow Zone.
3. Siram tanaman.
4. Ubah `realSecondsPerDay` ke **10** di TimeManager. Tunggu Day Rollover.

### Yang Diverifikasi:
- [✅] Console: `[CropInstance] Growth paused at (X,Y) due to lack of light.`
- [✅] Tanaman **TETAP di Stage 0** (tidak bertumbuh).
- [✅] Sprite tanaman terlihat **REDUP / kebiruan** (Color dimmed).

### Test B — UV Pillar Recovery:
5. **Tanpa menghentikan Play Mode**, buat UV Pillar:
   - **Klik kanan folder `--- MACHINES ---`**, pilih Create Empty.
   - Rename jadi **"UV Pillar"**.
   - Add Component: `UVLightPillar`.
   - Geser posisinya menggunakan Move Tool agar berada **di dalam** atau **di batas** Shadow Zone.
6. Siram tanaman lagi.
7. Tunggu Day Rollover berikutnya.

### Yang Diverifikasi:
- [✅] Console: **TIDAK ADA** log "Growth paused" untuk tanaman tersebut.
- [✅] Tanaman **NAIK ke Stage 1**.
- [✅] Sprite tanaman kembali **CERAH** (Color = White).

### Test C — Remove UV:
8. Delete GameObject UV Pillar dari scene (saat masih Play Mode).
9. Siram tanaman.
10. Tunggu Day Rollover.

### Yang Diverifikasi:
- [✅] Console: `[CropInstance] Growth paused at (X,Y) due to lack of light.`
- [✅] Tanaman **berhenti tumbuh** lagi.
- [✅] Sprite kembali **REDUP / kebiruan**.

---

## 🐛 Bug yang Sudah Diperbaiki Saat Code Review

| # | File | Bug | Status |
|:---:|:---|:---|:---:|
| 1 | `TimeManager.cs` | **CRITICAL**: `GetFormattedTime()` tidak punya closing brace `}`, menyebabkan `SkipToMorning()` ter-nested di dalamnya. **Compile Error.** | ✅ Fixed |
| 2 | `GridManager.cs` | `[Header]` attribute pada field `private` tanpa `[SerializeField]` — tidak muncul di Inspector. Diganti jadi comment. | ✅ Fixed |

---

## 📝 Catatan Penting

> [!WARNING]
> **Data tidak persisten antar Play Mode!** 
> Fog of War, fatigue counter, dan shadow state semuanya hilang saat Anda menekan **Stop** di Unity. Ini normal karena kita belum implementasi Save/Load system.

> [!TIP]
> **Shortcut Testing Cepat:**
> - Ubah `realSecondsPerDay` di TimeManager ke **10–30** untuk mempercepat siklus hari.
> - Ubah `currentOxygen` di PlayerVitals langsung di Inspector untuk test rescue.
> - Ubah `daysSinceLastSleep` di PlayerVitals langsung di Inspector untuk test fatigue level tertentu.

---
*Dokumen ini dibuat otomatis oleh Dev Agent (Antigravity). Pastikan semua checklist dicentang sebelum handoff ke QA.*
