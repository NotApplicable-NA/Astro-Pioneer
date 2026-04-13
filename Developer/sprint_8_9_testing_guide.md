# 🧪 Unity Testing Guide — Sprint 8 & 9 Features

**Date:** 2026-04-11  
**Tester:** Developer / QA  
**Scene:** `SampleScene.unity`

> [!IMPORTANT]
> Sebelum memulai testing, pastikan Anda sudah **membuka project di Unity** dan tidak ada **compile error** di Console. Jika ada error, selesaikan dulu sebelum melanjutkan. Anda HARUS menjalankan Tool Generator sebelum mengetes fitur-fitur ini!

---

## 📋 Pre-Test Setup (Wajib)

### Langkah 0: Buka Project & Jalankan Tool Generator
1. Buka Unity Hub → Buka project **Astro-Pioneer**.
2. Klik menu di bagian paling atas Editor: **`Astro-Pioneer`** → **`Generate Expansion Content (Sprint 8 & 9)`**.
3. Buka **Console** (`Window > General > Console`). Periksa apakah muncul pesan: `[ContentGenerationSetup] Sprint 8 & 9 Extra Content Generated successfully!`.
4. Buka folder `Assets/Data/Crops`, `Assets/Data/Items`, dan `Assets/Data/Planets` untuk memastikan file konfigurasi baru sudah ter-generate.

### Langkah 0.1: Siapkan Sistem Singleton Baru
Beberapa sistem baru merupakan Singleton global yang harus dimuat di Scene:

| Script | Pasang Di Mana | Cara |
|:---|:---|:---|
| `TechTreeManager.cs` | **Klik kanan `--- MANAGERS ---`** > Create Empty bernama "TechTreeManager" | Add Component `TechTreeManager`. |
| `EcoTracker.cs` | **Klik kanan `--- MANAGERS ---`** > Create Empty bernama "EcoTracker" | Add Component `EcoTracker`. |
| `EnclosureSystem.cs` | **Klik kanan `--- MANAGERS ---`** > Create Empty bernama "EnclosureSystem" | Add Component `EnclosureSystem`. |
| `PlacementManager.cs` | **Klik kanan `--- MANAGERS ---`** > Create Empty bernama "PlacementManager" | Add Component `PlacementManager`. Ghost Visual otomatis dibuat saat Play. |
| `SaveGameManager.cs` | **Klik kanan `--- MANAGERS ---`** > Create Empty bernama "SaveGameManager" | Add Component `SaveGameManager`. Lihat langkah 0.3 untuk setup registry. |

### Langkah 0.2: Siapkan Starter Inventory (PENTING!)
Agar saat Play Mode Anda sudah memiliki item di Hotbar tanpa perlu cheat:

1. Klik GameObject yang memiliki komponen **`InventoryManager`** di Hierarchy.
2. Di Inspector, cari bagian **Starter Kits (For Testing)**.
3. Klik tombol **+** pada list `Starter Items` dan drag item-item berikut dari `Assets/Data/Items`:
   - `Item_Hoe` (Cangkul — WAJIB, dibutuhkan untuk menghancurkan mesin)
   - `Item_AgriMech` (Agri-Mech Drone)
   - `Item_Harvester` (Auto Harvester)
   - `Item_Composter` (Bio-Composter)
   - `Item_Fence` (Energy Fence)
   - `Item_Seed_SpacePotato` (Bibit kentang)
4. Centang juga **Infinite Mode** jika Anda ingin menempatkan barang tanpa batas jumlah.

### Langkah 0.3: Setup Save Registry
1. Klik GameObject `SaveGameManager` di Hierarchy.
2. Di Inspector, buka bagian **Placeable Items Registry**.
3. Klik **+** dan drag semua item `Crafted` yang memiliki prefab (AgriMech, Harvester, Composter, Fence).
4. Ini memungkinkan sistem Save/Load mengenali mesin saat dimuat ulang.

> [!TIP]
> **Anda sekarang bisa menempatkan mesin langsung saat Play Mode!** Cukup tekan tombol hotbar `1-6` untuk memilih item, lalu klik kiri pada grid. Ghost hijau = valid, Ghost merah = tabrakan.  
> **Untuk menghancurkan mesin:** Pilih **Hoe** di hotbar, lalu klik kiri pada mesin target.

> [!WARNING]
> **`// TODO: REMOVE HOE DESTRUCTION BEFORE PUBLISH`** — Fitur "hancurkan mesin dengan Hoe" bersifat SEMENTARA untuk testing. Fitur ini WAJIB dihapus atau diganti sebelum game di-publish!

---

## ✅ TEST 0: Placement Mechanics & Grid Snapping (BARU!)

**Tujuan:** Memastikan pemain bisa menempatkan mesin dari Inventory ke Grid secara dinamis saat Play Mode.

### Langkah Persiapan:
Pastikan Langkah 0.2 (Starter Inventory) sudah selesai.

### Langkah Testing:
1. Tekan **Play**.
2. Tekan angka **1** pada keyboard untuk memilih item pertama di Hotbar (misal: Hoe).
3. Tekan angka **2** untuk memilih item kedua (misal: AgriMech). Anda akan melihat **Ghost hologram transparan** mengikuti kursor mouse Anda di grid.
4. Arahkan kursor ke area grid kosong — Ghost berwarna **Hijau** (valid).
5. Arahkan kursor ke area yang sudah terisi — Ghost berwarna **Merah** (tidak bisa ditaruh).
6. **Klik kiri** pada area hijau untuk menempatkan mesin.
7. Pilih **Hoe** di hotbar (tekan `1`), lalu **klik kiri** pada mesin yang baru ditaruh untuk menghancurkannya.
8. Tekan **Stop** (berhenti Play Mode), lalu tekan **Play** lagi. Mesin yang Anda taruh di sesi sebelumnya harus muncul kembali otomatis (Auto-Load dari `WorldSave.json`).

### Yang Diverifikasi:
- [✅] Ghost visual muncul saat item Crafted dipilih di Hotbar.
- [✅] Ghost menempel (snap) tepat ke titik-titik grid.
- [✅] Warna Ghost berubah Hijau/Merah sesuai ketersediaan cell.
- [✅] Mesin berhasil di-instantiate saat klik kiri di area valid.
- [✅] Mesin hilang saat diklik dengan Hoe, Console log: `[PlayerToolState] Destroyed ... with Hoe.`
- [✅] Setelah Stop & Play ulang, mesin otomatis ter-load dari file save. Console log: `[SaveGameManager] Successfully loaded X machines.`

---

## ✅ TEST 1: Macro-Grid & Dual Grid Placement (GridManager)

**Tujuan:** Memastikan mesin besar (Macro 2x2) bisa menempati grid ganda dan memblokir tanaman.

### Langkah Persiapan:
1. Gunakan **Placement System** (TEST 0) untuk menaruh AgriMech di grid.
2. AgriMech otomatis menempati 2x2 cell di GridManager.

### Langkah Testing:
1. Tekan tombol **Play** di atas editor.
2. Tempatkan AgriMech via hotbar, lalu coba tanam bibit di area yang sudah terisi AgriMech.

### Yang Diverifikasi:
- [x] Ada Gizmo (kotak biru transparan) sebesar 2x2 mengelilingi AgriMech di *Scene View*.
- [✅] Jika AgriMech ditempatkan di atas grid tanaman, tanaman yang bersangkutan gagal ditanam atau memunculkan peringatan. (Note : Ada bug kalo udah nanem, dan posisi agri mech diatas tanaman, kemudian game di close, dia akan hilang ketika game di start lagi, seakan fitur play nya ga berjalan ketika dia ada di atas tanaman)
- [-] Pipa (Micro Grid) dapat ditempatkan di cell yang sama dengan Pagar (bisa lebih dari satu Micro objek per GridCell).

---

## ✅ TEST 2: Deteksi Kandang Sapi (Animal Husbandry)

**Tujuan:** Memastikan EnclosureSystem dapat mendeteksi kurungan Fence tertutup menggunakan *Flood Fill*.

### Langkah Persiapan:
1. Saat Play Mode, pilih **Fence** di Hotbar (tekan tombol angka yang sesuai).
2. Tempatkan pagar membentuk **kotak tertutup** (minimal 8 pagar membentuk ring 3x3 yang bolong di tengahnya). Klik satu per satu di grid yang bersebelahan.
3. Buat satu GameObject kosong bernama `Sapi_Alien` di Hierarchy **SEBELUM** Play Mode.
4. Di **Inspector** `Sapi_Alien`, tambahkan **SpriteRenderer** (pilih sprite bundar, Scale 1,1) dan komponen `FaunaAI`.
5. Posisikan Sapi ini tepat di tengah-tengah area yang akan dikurung pagar.

### Langkah Testing:
1. Tekan tombol **Play**, tempatkan pagar, lalu amati Console.

### Yang Diverifikasi:
- [✅] Buka Console: Lihat pesan `[EnclosureSystem] Found Enclosure 'Enclosure_0' with X tiles capacity.`
- [✅] Sapi (FaunaAI) yang ditempatkan di dalam kandang akan mondar-mandir secara diam-diam.
- [✅] Sapi tersebut **TIDAK PERNAH** berjalan menembus atau melebihi batas Pagar (tertahan di dalam `validMoves`).

---

## ✅ TEST 3: Mekanisme Traktor Tanam (AgriMech 2x2)

**Tujuan:** Memastikan AgriMech bergerak linear maju dan langsung menanam bibit di belakangnya.

### Langkah Persiapan:
1. Tempatkan AgriMech lewat **Placement System** (TEST 0).
2. **SEBELUM Play**, pada prefab AgriMech pastikan field **Seed To Plant** terisi (drag `CropData_SpacePotato` dari `Assets/Data/Crops`).
3. Pastikan `bypassPower` dicentang (default-nya `true`) agar mesin ini berjalan tanpa perlu sistem listrik.

### Langkah Testing:
1. Tekan tombol **Play**.
2. Biarkan game berjalan selama beberapa detik tanpa input apapun.

### Yang Diverifikasi:
- [✅] AgriMech otomatis bergeser maju setiap beberapa detik.
- [✅] Tepat di atas *tile* yang baru saja ditinggalkannya, muncul `CropInstance` bibit Space Potato.(NOTE : tapi mesin akan berhenti ketika dia mentok dan dibelakangnya udah ditanami)
- [✅] Console mencatat pesan: `[AgriMech] Moved to (X, Y)`.
- [✅] Jika menabrak tembok atau ujung Grid, AgriMech membalikkan arah pergerakannya (Reversing direction).

---

## ✅ TEST 4: Auto Harvester (Scanning 3x3)

**Tujuan:** Memastikan Auto Harvester secara rutin mengambil hasil panen di radius 3x3-nya.

### Langkah Persiapan:
1. Tempatkan **Auto Harvester** lewat Placement System di dekat area tanaman.
2. Tanam crop di sekitarnya menggunakan bibit di Hotbar.
3. Buat tanaman tersebut mencapai fase *harvestable* (Tumbuh maksimal = Stage 3) menggunakan *cheat* CropInstance di Inspector.

### Langkah Testing:
1. Tekan **Play**.

### Yang Diverifikasi:
- [✅] Console: `[MachineHarvester] Scanning 3x3 at (X, Y)`.
- [✅] Saat tanaman sudah di-stage klimaks, Harvester membabatnya secara otomatis.
- [✅] Console: `[MachineHarvester] Auto-harvesting Space Potato at (X, Y)`.

---

## ✅ TEST 5: Mesin Kompos (Circular Economy)

**Tujuan:** Memastikan Composter dapat mengubah material tidak terpakai menjadi cairan Fertilizer.

### Langkah Persiapan:
1. Tempatkan **Composter** lewat Placement System.
2. **SEBELUM Play**, pada prefab Composter pastikan field **Input Item** dan **Output Item** terisi. Drag `Item_BioFuelCell` ke Input, dan `Item_BioFertilizer` ke Output.

### Langkah Testing:
1. Tekan **Play**.
2. Agar cepat tesnya, ubah angka **Processing Time** di Inspector dari 60 menjadi 3 detik.
3. Coba panggil method eksekusinya: Di Inspector `Composter_Test`, klik titik tiga di sudut kanan atas komponen, atau langsung klik collidernya di Game Mode dengan membawa item yang sesuai. Panggil **TryAddInput()**.

### Yang Diverifikasi:
- [✅] Saat dimasukkan input, Composter langsung menghitung mundur waktu *processingTime*.
- [✅] Saat waktu telah habis, muncul log `[MachineComposter] Processed 1 Fertilizer.`.
- [✅] Saat diklik dengan tangan kosong (TryCollectOutput), item perlahan muncul atau menambah ke `InventoryManager`.

---

## ✅ TEST 6: Trigger Tamat (EcoTracker Utopia)

**Tujuan:** Memastikan game dapat selesai (memunculkan Event Tamat) kalau Poin Ekologi tercapai.

### Langkah:
1. Tekan **Play**.
2. Buka Inspector dari `EcoTracker`.
3. Set `requiredEndGameEco` ke 80 dan `requiredEndGameTrust` ke 1000.
4. Gunakan Inspector di `CurrencyManager` untuk memanipulasi nilai Trust menjadi **1500**.
5. Gunakan Inspector `EcoTracker`, dan ubah `currentEcoScore` secara manual menjadi **85**.
6. Klik pada game *checkbox*, atau panggil `AddEcoScore(1)`.

### Yang Diverifikasi:
- [✅] Nilai akan terhitung *Clamp* di limit 100.
- [✅] Console meledak dengan tulisan: `[EcoTracker] END GAME CONDITIONS MET! Epilogue Transmission Incoming...`.
- [✅] Log di atas hanya akan muncul **satu kali** berkat Flag `hasTriggeredEndGame = true`.

---

## 📝 Catatan Penting

> [!WARNING]
> Harap **jangan menghancurkan pagar (Destroy)** sebelum Anda yakin sapi (`FaunaAI`) Anda bisa di-handle, karena sistem ini bekerja melalui pembacaan ulang (Reevaluate). Jika pagar terbuka dan sistem terlambat scan, sapi akan keluar kandang *free range*!

> [!CAUTION]
> **SEBELUM PUBLISH:** Cari dan hapus semua kode bertanda `// TODO: REMOVE HOE DESTRUCTION BEFORE PUBLISH` di `PlayerToolState.cs`. Fitur Hoe Destroy hanya untuk development/testing!

> [!TIP]
> **Shortcut Testing Cepat:**
> - Set `processingTime` pada `MachineComposter` menjadi **3 detik** alih-alih 60 detik.
> - Manfaatkan tombol `[P]` untuk memunculkan `AutomationUI` untuk mengecek koneksi grid.
> - Setup **Time.timeScale** via `Escape` (`PauseMenuUI`) jika log Console terlalu gila bergulir!
> - Data save tersimpan di `Application.persistentDataPath/WorldSave.json`. Hapus file ini jika ingin reset total.

---
*Dokumen ini dibuat otomatis oleh Dev Agent (Antigravity). Pastikan semua checklist dicentang sebelum handoff ke QA untuk Quality Gate tahap akhir Build 1.0!*
