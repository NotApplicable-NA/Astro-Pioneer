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

### Langkah 0.1: Siapkan Objek UI & Sistem Baru
Beberapa sistem baru merupakan Singleton global yang harus dimuat:

| Script | Pasang Di Mana | Cara |
|:---|:---|:---|
| `TechTreeManager.cs` | **Klik kanan `--- MANAGERS ---`** > Create Empty bernama "TechTreeManager" | Add Component `TechTreeManager`. |
| `EcoTracker.cs` | **Klik kanan `--- MANAGERS ---`** > Create Empty bernama "EcoTracker" | Add Component `EcoTracker`. |
| `EnclosureSystem.cs` | **Klik kanan `--- MANAGERS ---`** > Create Empty bernama "EnclosureSystem" | Add Component `EnclosureSystem`. |

> [!TIP]
> Untuk ngetes mesin-mesin UI seperti `AdvancedStorage` dan `AutomationUI`, pastikan GameObject di-assign komponen Collider2D agar bisa diklik saat Game Mode.

---

## ✅ TEST 1: Macro-Grid & Dual Grid Placement (GridManager)

**Tujuan:** Memastikan mesin besar (Macro 2x2) bisa menempati grid ganda dan memblokir tanaman.

### Langkah Persiapan:
1. Di jendela **Hierarchy**, klik kanan lalu pilih **Create Empty**, beri nama `AgriMech_Test`.
2. Klik `AgriMech_Test`, lalu di jendela **Inspector**, klik **Add Component** dan cari `AgriMech`.

### Langkah Testing:
1. Tekan tombol **Play** di atas editor.
2. Klik GameObject `AgriMech_Test` di *Hierarchy*, lalu arahkan kursor ke *Scene View*.

### Yang Diverifikasi:
- [✅] Ada Gizmo (kotak biru transparan) sebesar 2x2 mengelilingi AgriMech di *Scene View*.
- [✅] Jika AgriMech ditempatkan di atas grid tanaman, tanaman yang bersangkutan gagal ditanam atau memunculkan peringatan.
- [✅] Pipa (Micro Grid) dapat ditempatkan di cell yang sama dengan Pagar (bisa lebih dari satu Micro objek per GridCell).

---

## ✅ TEST 2: Deteksi Kandang Sapi (Animal Husbandry)

**Tujuan:** Memastikan EnclosureSystem dapat mendeteksi kurungan Fence tertutup menggunakan *Flood Fill*.

### Langkah Persiapan:
1. Di jendela **Hierarchy**, klik kanan dan pilih **Create Empty**, beri nama `Pagar_1`, lalu copy-paste hingga membentuk setidaknya 8 pagar (membuat ring 3x3 yang bolong tengahnya).
2. Posisikan pagar-pagar tersebut secara kotak bersambung menggunakan Move Tool (W). Pastikan tidak ada rongga.
3. Buat satu GameObject kosong lagi bernama `Sapi_Alien`.
4. Di **Inspector** `Sapi_Alien`, klik **Add Component** dan cari `FaunaAI`. Posisikan Sapi ini tepat di tengah-tengah kurungan pagar tadi.

### Langkah Testing:
1. Tekan tombol **Play**.

### Yang Diverifikasi:
- [✅] Buka Console: Lihat pesan `[EnclosureSystem] Found Enclosure 'Enclosure_0' with X tiles capacity.`
- [✅] Sapi (FaunaAI) yang ditempatkan di dalam kandang akan mondar-mandir secara diam-diam.
- [✅] Sapi tersebut **TIDAK PERNAH** berjalan menembus atau melebihi batas Pagar (tertahan di dalam `validMoves`).

---

## ✅ TEST 3: Mekanisme Traktor Tanam (AgriMech 2x2)

**Tujuan:** Memastikan AgriMech bergerak linear maju dan langsung menanam bibit di belakangnya.

### Langkah Persiapan:
1. Klik GameObject `AgriMech_Test` yang sudah kita buat di Test 1.
2. Di **Inspector** komponen `AgriMech`, cari field bernama **Seed To Plant**.
3. Buka folder `Assets/Data/Items` di *Project window*, lalu klik dan drag file `Item_Seed_SpacePotato` ke dalam field **Seed To Plant** tersebut.

### Langkah Testing:
1. Tekan tombol **Play**.
2. Biarkan game berjalan selama beberapa detik tanpa input apapun.

### Yang Diverifikasi:
- [✅] AgriMech otomatis bergeser maju setiap beberapa detik.
- [✅] Tepat di atas *tile* yang baru saja ditinggalkannya, muncul `CropInstance` bibit Space Potato.
- [✅] Console mencatat pesan: `[AgriMech] Moved to (X, Y)`.
- [✅] Jika menabrak tembok atau ujung Grid, AgriMech membalikkan arah pergerakannya (Reversing direction).

---

## ✅ TEST 4: Auto Harvester (Scanning 3x3)

**Tujuan:** Memastikan Auto Harvester secara rutin mengambil hasil panen di radius 3x3-nya.

### Langkah Persiapan:
1. Di **Hierarchy**, klik kanan -> **Create Empty**, beri nama `AutoHarvester_Test`.
2. Di **Inspector**, klik **Add Component** dan cari `MachineHarvester`.
3. Tanam crop (tanaman) menggunakan `CropManager` atau taruh prefab tanaman di sel yang berbatasan langsung dengan Harvester (kiri/kanan/atas).
4. Buat tanaman tersebut mencapai fase *harvestable* (Tumbuh maksimal = Stage 3). Jika malas menunggu, saat Play Mode klik GameObject tanaman tersebut, cari `CropInstance` di Inspector, dan ubah angkanya ke 3.

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
1. Di **Hierarchy**, klik kanan -> **Create Empty**, beri nama `Composter_Test`.
2. Di **Inspector**, tambahkan komponen `MachineComposter` dan komponen `BoxCollider2D` (agar bisa diklik).
3. Buka folder `Assets/Data/Items`.
4. Drag file `Item_BioFuelCell` ke kotak field **Input Item** di Inspector.
5. Drag file `Item_BioFertilizer` (opsi lain jika belum ada, pakai item apapun) ke field **Output Item** di Inspector.

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

> [!TIP]
> **Shortcut Testing Cepat:**
> - Set `processingTime` pada `MachineComposter` menjadi **3 detik** alih-alih 60 detik.
> - Manfaatkan tombol `[P]` untuk memunculkan `AutomationUI` untuk mengecek koneksi grid.
> - Setup **Time.timeScale** via `Escape` (`PauseMenuUI`) jika log Console terlalu gila bergulir!

---
*Dokumen ini dibuat otomatis oleh Dev Agent (Antigravity). Pastikan semua checklist dicentang sebelum handoff ke QA untuk Quality Gate tahap akhir Build 1.0!*
