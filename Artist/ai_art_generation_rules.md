# 🎨 AI Art Generation Rules — Learned from Experience

> **Purpose:** Dokumen ini berisi semua aturan dan pelajaran yang dipelajari selama proses generate sprite bot menggunakan AI image generator (ChatGPT/DALL-E). Gunakan sebagai referensi saat generate asset baru.

---

## ⚠️ KATA KUNCI YANG HARUS DIHINDARI

### ❌ Jangan gunakan "THIS EXACT" atau "EXACT SAME"
- **Masalah:** AI akan copy-paste gambar identik di setiap frame, tidak ada animasi/perubahan posisi
- **Solusi:** Gunakan bahasa lebih soft: `"Use the same robot design, colors, and viewing angle as the reference"`
- **Contoh buruk:** `"Keep the EXACT SAME design"` → frame identik
- **Contoh bagus:** `"Use the same robot design"` → desain konsisten tapi posisi bervariasi

### ❌ Jangan gunakan "TRANSPARENT" background
- **Masalah:** AI image generator TIDAK BISA membuat transparent background. Hasilnya selalu ada background
- **Solusi:** Selalu gunakan `"Background: SOLID WHITE"` — remove white background nanti di post-processing
- **Solusi ALT:** Jika asset berwarna PUTIH/TERANG (misal ship tiles), gunakan `"Background: SOLID BRIGHT MAGENTA (#FF00FF)"` — warna chroma key klasik yang kontras dengan semua warna asset

### ❌ Jangan gunakan "ANIMATION" atau "MOVEMENT"
- **Masalah:** Kata-kata ini trigger AI untuk generate VIDEO, bukan static image
- **Solusi:** Gunakan `"SPRITE SHEET"`, `"POSES"`, `"horizontal strip"`, `"side by side in ONE image"`
- **Contoh buruk:** `"Generate a 4-FRAME MOVEMENT ANIMATION"` → video
- **Contoh bagus:** `"Generate a PIXEL ART SPRITE SHEET showing 4 POSES"` → static image

### ❌ Jangan minta "ONLY 2 images"
- **Masalah:** AI sangat sulit generate tepat 2 frame. Selalu default ke 1 (zoom besar blur) atau 4
- **Solusi:** Selalu minta 4 frame, lalu crop yang dibutuhkan
- **Workaround:** Generate 4 frame (open-UP, target-UP, open-DOWN, target-DOWN) → crop frame 2 dan 4

### ❌ Jangan minta ukuran pixel KECIL secara literal (misal "16x16 pixels")
- **Masalah:** AI image generator output minimal ~1024x1024. Jika diminta "16x16", AI malah menggambar *sprite editor interface* yang menampilkan tile kecil di dalamnya, bukan tile itu sendiri
- **Solusi:** Generate di canvas BESAR (512x512 atau 1024x1024) dengan instruksi `"contains ONLY about 16x16 logical pixels drawn as large blocky squares"` — AI menggambar pixel art chunky yang di-zoom
- **Post-processing:** Resize down ke target size menggunakan **Nearest Neighbor** interpolation

---

## ✅ ATURAN YANG TERBUKTI BERHASIL

### 1. Format Output
- Selalu specify: `"in a horizontal strip (4 images side by side in ONE image)"`
- Selalu akhiri dengan: `"Output: ONE SINGLE static image, NOT a video"`
- Gunakan `"SPRITE SHEET"` dan `"POSES"` bukan `"ANIMATION"` dan `"FRAMES"`

### 2. Konsistensi Desain
- Upload reference image DAN describe `"Use the same robot design, colors, and viewing angle as the reference"`
- JANGAN gunakan `"THIS EXACT"` — terlalu rigid, mematikan variasi posisi yang diinginkan

### 3. Bobbing/Posisi Vertikal
- Setiap frame HARUS punya posisi vertikal berbeda: MIDDLE → UP → MIDDLE → DOWN
- Specify jarak: `"shifted UP/DOWN by 2-3 pixels"`
- Untuk DOWN: explicitly say `"BELOW reference position — the body sinks lower than where it normally sits"`
- Tanpa instruksi "BELOW" yang kuat, AI hanya generate NEUTRAL sebagai posisi terendah (bukan benar-benar DOWN)

### 4. Reference Sheet (Multi-view)
- Layout: `"2 rows × 4 columns"` untuk 8 arah
- WAJIB specify jumlah: `"EXACTLY 8 views"` — tanpa ini AI bisa generate 12-16 views
- Arah diagonal HARUS dideskripsikan sangat detail:
  - SE: `"FRONT face visible, EYES visible, angled toward bottom-right"`  
  - NE: `"BACK visible, NO EYES, angled toward top-right"` — ini BERBEDA dari SE!
  - SW: `"Mirror/flip of SE"` — paling efektif untuk konsistensi
- Specify mana yang punya mata, mana yang tidak: `"Views 1,2,8 show FRONT FACE with eyes. Views 4,5,6 show BACK with NO eyes"`

### 5. Overlay Sprite (Separate Layer)
- Untuk objek yang ditaruh di atas karakter (crate, hat, dll): generate TERPISAH sebagai overlay sprite
- JANGAN generate karakter+objek dalam satu gambar — hasilnya selalu:
  - Objek terlalu kecil
  - Objek masuk/overlap ke dalam karakter
  - Jumlah view bertambah sendiri
  - Layout berantakan
- Upload reference sheet sebagai `"rotation reference"` untuk matching sudut pandang

### 6. Editing Existing Image
- JANGAN minta AI untuk "edit" atau "add to" gambar existing — hasilnya buruk dan tidak konsisten
- Lebih baik generate ulang dari scratch dengan reference sebagai panduan desain

### 7. Frame Count
- AI paling konsisten dengan **4 frame** → selalu target 4
- Untuk kebutuhan 2 frame: generate 4 frame alternating, lalu crop 2 yang dibutuhkan

---

## 📝 TEMPLATE PROMPT YANG TERBUKTI BERHASIL

### Template: Static Reference Sheet (8 views)
```
A PIXEL ART CHARACTER TURNAROUND SHEET showing 8 static views of the SAME [character], 
arranged in 2 rows of 4. Camera is OVERHEAD looking slightly down (top-down 3/4 view 
like Stardew Valley). This is NOT isometric.
[CHARACTER DESCRIPTION]
ROW 1: [view descriptions with eyes/no-eyes details]
ROW 2: [view descriptions with eyes/no-eyes details]
All 8 = SAME character, SAME size. Each view 64x64 pixels. Background: SOLID WHITE.
```

### Template: 4-Pose Sprite Sheet (bobbing/movement)
```
[UPLOAD reference image] Generate a PIXEL ART SPRITE SHEET showing 4 POSES of this 
robot in a horizontal row (4 images side by side in ONE image). Use the same robot 
design, colors, and viewing angle as the reference.
Pose 1: [description]
Pose 2: [description] 
Pose 3: [description]
Pose 4: [description]
Each pose must look VISIBLY different.
64x64 pixel art per pose. Background: SOLID WHITE. Output: ONE SINGLE static image, NOT a video.
```

### Template: Blink/Variant (2 needed, generate 4)
```
[UPLOAD reference image] Generate a PIXEL ART SPRITE SHEET showing 4 POSES of this 
robot in a horizontal row. Use the same robot design, colors, and viewing angle.
Pose 1: robot UP position, [feature NORMAL]
Pose 2: robot UP position, [feature CHANGED]
Pose 3: robot DOWN position, [feature NORMAL]
Pose 4: robot DOWN position, [feature CHANGED]
64x64 pixel art per pose. Background: SOLID WHITE. Output: ONE SINGLE static image, NOT a video.
→ CROP: Use pose 2 and 4 only.
```

---

## 🔄 POST-PROCESSING WORKFLOW

1. **Generate** → AI output (white background, 4 poses in a strip)
2. **Crop** → Pisahkan setiap pose jadi individual frame
3. **Remove white background** → Buat transparent di image editor (Aseprite, Photoshop, dll)
4. **Assemble sprite sheet** → Gabungkan ke final sprite sheet dengan layout yang benar
5. **Import to Unity** → Set sprite mode, slice, assign to animations

---

## 📊 UKURAN & SPESIFIKASI

| Asset | Per-frame Size | Frames |
|-------|---------------|--------|
| Bot Reference | 64x64 | 8 static views |
| Crate Overlay | 64x64 | 8 static views |
| Idle Bobbing | 64x64 | 4 poses |
| Idle Blink | 64x64 | 2 poses (crop dari 4) |
| Move | 64x64 | 4 poses |
| VFX Dust | 32x32 | 4 poses |
| VFX Exhaust | 16x16 | 4 poses |
