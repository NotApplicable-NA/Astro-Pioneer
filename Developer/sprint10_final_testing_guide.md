# 🧪 Release Candidate Testing Guide (Sprint 10 Features)

Panduan ini berisi metode terstruktur (QA TDD Style) untuk memverifikasi tiga fitur _engine_ terakhir yang baru saja diimplementasikan: **Object Pooling (Performa)**, **Dual-Grid System (Kabel/Pipa)**, dan **Localization Framework (Bahasa)**.

---

## 🛠️ Persiapan (Pre-requisites) di Unity Editor

Sebelum memulai pengujian, pastikan struktur _Scene_ Unity Anda sudah mewarisi komponen-komponen baru ini:

1. **Object Pool Manager**
   - Cari _GameObject_ kosong atau buat baru bernama `[MANAGERS]`.
   - Tambahkan komponen *script* `ObjectPoolManager.cs`. Script ini akan menangani daur ulang partikel VFX secara otomatis.
2. **Grid Layer Controller**
   - Di *GameObject* yang sama (`[MANAGERS]`), tambahkan *script* `GridLayerController.cs`.
3. **Localization Manager**
   - Tambahkan *script* `LocalizationManager.cs` ke _GameObject_ `[MANAGERS]`.
   - Di _Project window_, buat folder: `Assets/Resources/Lang`.
   - Di dalam folder `Lang`, buat file teks murni bernama `en.txt` (Hanya *plain text*, hilangkan ekstensi `.txt` jika terbaca ganda).
   - Isi file tersebut dengan format TSV (Pisahkan kunci dan nilai dengan tombol TAB):
     ```text
     ui_play_button    Play Game
     ui_farm_name    Astro Farm
     ```
4. **Siapkan Item Pipa/Kabel (Micro Grid)**
   - Pergi ke folder `Assets/Data/Items` atau buat sebuah `InventoryItem` (tipe _ScriptableObject_) baru. Beri nama `Pipe_Item`.
   - Di inspector `Pipe_Item`, **CENTANG** kotak `isMicroGridItem`. 

---

## 🧪 TEST CASE 1: Object Pooling (Stress Test Performa)

**Tujuan:** Memastikan partikel _Harvest_ dan _Watering_ didaur ulang di RAM, bukan dibuat lalu dibakar terus-menerus yang akan menyebabkan game _lag_.

### Langkah Eksekusi:
1. Buka Hierarchy Unity saat mode Play berjalan.
2. Siapkan alat penyiram *(Watering Can)*.
3. Klik untuk menyiram tanaman secara brutal (spam klik) lebih dari 10 kali secara cepat pada *tile* yang sama.
4. Di jendela **Hierarchy**, perhatikan _GameObject_ `[MANAGERS]`. Di bawahnya akan muncul _clone_ dari `WateringVFX`.
5. **Verifikasi:**
   - [ ] _GameObjects_ VFX tersebut **TIDAK** di-*Destroy* dari Hierarchy setelah percikannya selesai.
   - [ ] _GameObjects_ VFX berubah status menjadi abu-abu (Nonaktif / `SetActive(false)`) dan mengantre di dalam Pool.
   - [ ] Klik siram lagi; Unity harusnya menghidupkan kembali partikel yang mati tadi (`SetActive(true)`), dan **BUKAN** meng-_Instantiate_ objek baru di bawah _scene root_.

---

## 🧪 TEST CASE 2: Exterior Dual-Grid (Sistem Kabel Bawah Tanah)

**Tujuan:** Memastikan Traktor (Macro) dan Jalur Pipa Wires (Micro) dapat diletakkan di **KORDINAT KOTAK YANG SAMA PERSIS** tanpa memblokir satu sama lain.

### Langkah Eksekusi:
1. Pada `PlayerToolState` di Inspector, pastikan Anda menaruh `Pipe_Item` di *Hotbar Slot 1* Anda (Item dengan centang `isMicroGridItem`).
2. Taruh sembarang alat mesin makro (misal: Sprinkler atau Storage) pada titik *(0,0)*.
3. Dekati titik *(0,0)*, lalu pilih *Hotbar 1* (`Pipe_Item`). Coba tempatkan Pipa tersebut di kotak penyimpanan yang sama persis: *(0,0)*.
   - **Verifikasi 1:** Pipa berhasil di-*place* (Tidak muncul peringatan "Space is blocked").
4. Saat berjalan normal, kursor _hover_ Anda akan selalu menyorot **Mesin Macro** (Sprinkler).
5. Tekan tombol **`TAB`** di *keyboard* Anda. Ini memicu `ToggleLayerMode` di dalam C#.
6. Arahkan kursor Anda lagi ke kotak *(0,0)*. 
   - **Verifikasi 2:** Kursor *highlight* kini menangkap **Pipa/Kabel** (Micro Grid) dan mengabaikan eksistensi mesin Sprinkler di atasnya.
7. Pakai *Hoe* (Cangkul) dan klik untuk *destroy* di kotak tersebut.
   - **Verifikasi 3:** Yang hancur (hilang) HANYA pipa/kabelnya. Mesin makro tidak ikut hancur.

---

## 🧪 TEST CASE 3: Localization (Dinamika Teks)

**Tujuan:** Memastikan teks UI berubah warna/kata tanpa harus _re-compile_ game sehingga _translator_ bisa bekerja menggunakan Notepad saja.

### Langkah Eksekusi:
1. Di layar Canvas (UI) sembarang, buat sebuah `Text - TextMeshPro`.
2. Klik *Add Component* di Inspector teks tersebut, lalu masukkan `LocalizedText`.
3. Di *script* `LocalizedText`, isi `Localization Key` dengan kata: `ui_play_button`.
4. Tekan **Play Game**.
5. **Verifikasi 1:** Teks kusam di editor tiba-tiba otomatis terganti menjadi "Play Game" sesuai isi file `.txt` Anda.
6. Biarkan game tetap *Play*. Buka file `en.txt` di editor luar (Notepad/VSCode). Ganti tulisan `Play Game` menjadi `Mulai Petani`. Lalu *Save*.
7. Kembali ke Unity Editor, pergi ke _GameObject_ yang menyimpan `LocalizationManager`, klik kanan skripnya (atau *trigger event* jika ada) untuk memuat ulang bahasanya.
   - **Verifikasi 2:** UI langsung terganti seketika secara _Real-Time_.

---

Jika seluruh tes verifikasi di atas sudah tervalidasi dengan tanda ceklis, arsitektur *backbone* GDD 3.5 dinyatakan 100% lulus QA!
