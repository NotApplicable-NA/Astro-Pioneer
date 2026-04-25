# 🤖 Astro-Pioneer: Entity & Off-Screen Simulation Architecture (V25)

Dokumen ini menjelaskan arsitektur teknis sistem entitas (Bot) dan mekanisme simulasi latar belakang (Off-Screen) yang diimplementasikan untuk memastikan determinisme dan skalabilitas game.

## 1. Filosofi: Brain-Puppet Split
Sistem ini memisahkan secara total antara data simulasi dan representasi visual.

| Komponen | Nama Arsitektur | Tanggung Jawab |
| :--- | :--- | :--- |
| **Brain (Jiwa)** | `BotData` (C# Class) | Menyimpan koordinat, ID, held item, dan status tugas. Hidup secara permanen di memori. |
| **Logic (Otak)** | `BotSimulationManager` | Menjalankan logika pergerakan, interaksi mesin, dan penugasan (Dispatching) bot. |
| **Puppet (Wayang)**| `TransportBot.cs` | GameObject di scene. Hanya bertugas melakukan interpolasi posisi ke koordinat Brain dan memutar animasi. |

---

## 2. Mekanisme Simulasi Off-Screen (Off-Screen Simulation)
Untuk memungkinkan bot tetap bekerja meskipun pemain berada sangat jauh (chunk visual di-unload), arsitektur ini menggunakan **Simulation Cache**.

### A. Jembatan Data (Simulation-Safe API)
`GridManager` menyediakan API khusus yang tidak bergantung pada visual:
- `GetStructureAtForSimulation()`: Mencari mesin di chunk aktif maupun yang ada di disk.
- `GetOrAllocateComplexStateForSimulation()`: Menulis/membaca inventory mesin di chunk yang tidak aktif.

### B. Simulation Cache (LRU System)
Implementasi di `ChunkManager`:
1. **Active**: Mencari data di chunk yang sedang di-render.
2. **Cache**: Jika tidak ada, mencari di cache memori (chunk yang baru saja diakses simulasi).
3. **Disk Fallback**: Jika tidak ada di cache, memuat data biner chunk langsung dari disk tanpa membuat GameObject.
4. **Auto-Save**: Jika data di cache dimodifikasi oleh bot, cache akan otomatis di-save ke disk sebelum dihapus (Evict) dari memori (setiap 5 detik tanpa aktivitas).

---

## 3. Sistem Dispatching Brain-Level
Sebelumnya, penugasan bot dilakukan oleh `BotStation` (MonoBehaviour). Sekarang, `BotSimulationManager` memiliki logika `TryRedispatch()`:
- Saat bot selesai bekerja dan menjadi `Idle`, Brain akan secara otomatis memindai area sekitar stasiun bot.
- Jika ditemukan pasangan Source (Pompa) dan Sink (Storage), tugas baru akan diberikan secara instan.
- **Hasilnya**: Bot tidak akan pernah berhenti bekerja hanya karena pemain menjauh.

---

## 4. Persistensi & Pemulihan (Save/Load)
- **Binary Serialization**: Seluruh status `BotData` (termasuk Pathfinding yang sedang berjalan) disimpan dalam file `.bin`.
- **Reincarnation**: Saat pemain mendekati area bot, `ChunkRenderer` akan secara otomatis melahirkan kembali Puppet berdasarkan `entityTypeID` dari `EntityRegistry` dan melakukan `BindToSimulation` menggunakan ID bot yang unik.

---

## 5. Pedoman Pengembangan (QA & Dev)
- **Jangan pernah** menaruh logika gameplay (seperti menambah item) di dalam script Puppet (`TransportBot`). Selalu lakukan di `BotSimulationManager`.
- **Enforce Inspector**: Selalu pastikan `EntityRegistry` terhubung di `ChunkRenderer` melalui Inspector untuk menghindari NullReference saat spawning visual.
