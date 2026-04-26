# GAME DESIGN DOCUMENT: ASTRO-PIONEER
**Version:** 3.5 (Design Revision — PPU 16, Clean 16-Bit Style, Animal Husbandry)
**Date:** 5 April 2026
**Genre:** Cozy Automation / Survival Logistics
**Theme:** Industrial Solarpunk, Bio-Innovation, Sustainable Crisis Management

## 1. Ringkasan Permainan (Overview)
Astro-Pioneer adalah game simulasi manajemen di mana pemain mengubah kapal survei industri yang kaku menjadi ekosistem pertanian otonom yang cerdas. Pemain bertindak sebagai Inovator, Petani, dan Insinyur yang harus menyeimbangkan efisiensi produksi makanan dengan eksplorasi planet untuk menyelamatkan koloni manusia.

## 2. Pilar Desain (Design Pillars)
- **Pilar 1 — Cozy Automation Payoff:** Dari pekerjaan manual yang berat di awal menjadi pertanian yang berjalan sendiri dengan efisiensi tinggi.
- **Pilar 2 — No Death, Only Logistics:** Tidak ada HP dan pemain tidak bisa mati. Tantangan berasal dari teka-teki logistik, energi, dan oksigen.
- **Pilar 3 — Solarpunk Circular Systems:** Ekonomi sirkular, bio-inovasi (Oxy-Flora), dan sistem tenaga grid ganda yang menekankan keberlanjutan.

## 3. Rincian Mekanik Gameplay Loop & Endgame
**Objektif Utama:** Mengembangkan kapal survei dan pos terdepan (outpost) di berbagai planet menjadi ekosistem pertanian dan logistik yang mandiri.

### 3.1. Siklus Mikro (Daily Loop - Menit ke Menit)
1. **Eksplorasi & Pemetaan (Explore & Scan):** Menjelajah permukaan planet, memindai flora/fauna jinak, dan membuka Fog of War (tidak ada minimap).
2. **Ekspansi Jaringan Oksigen (Expand):** Membangun pos-pos oksigen atau menanam flora penghasil oksigen khusus (Oxy-Flora) untuk memperluas jangkauan jelajah.
3. **Konstruksi Bebas (Build):** Membangun infrastruktur di dalam interior kapal atau di permukaan planet.
4. **Manajemen & Pertanian (Cultivate):** Menanam, merawat, dan memproses material mentah secara manual atau otomatis (irigasi/rover).
5. **Pemeliharaan Sistem (Maintain):** Menyeimbangkan energi dan mendaur ulang limbah organik menjadi kompos.

### 3.2. Siklus Makro (Progression Loop - Jam ke Jam)
1. **Penyelesaian Tujuan Unik Planet:** Setiap planet memiliki objektif utama (Refuel, Riset Biologi, Ekstraksi Berat).
2. **Jaringan Logistik Antar-Planet:** Setelah Warp ke planet berikutnya, base sebelumnya tetap beroperasi dan dapat mengirimkan sumber daya via Drone Antar-Planet.

### 3.3. Endgame (Kondisi Tamat & Mode Tanpa Batas)
Setelah menyelesaikan misi planet utama, pemain menerima transmisi epilog dari Bumi. Pemain diberikan dua pilihan:
- **Selesaikan Misi (Roll Credits):** Mengakhiri cerita utama dan pensiun dengan tenang.
- **Lanjutkan Eksplorasi (Endless Mode):** Menjadi pelopor independen di planet-planet baru (procedurally generated) tanpa batas waktu.

## 3. GAMEPLAY MECHANICS (The Hybrid Loop)

### 3.1. Core Loop: Explore - Innovate - Cultivate
Siklus permainan dirancang untuk memuaskan hasrat optimasi tanpa tekanan waktu yang mematikan.

1.  **Planetary Layer (Exploration & Farming):** Mendarat di planet untuk menambang mineral mentah (Silika, Titanium, Besi) dan mencari bibit tanaman eksotis. Mampu menanam crop endemik di planet itu.
2.  **Ship Layer (Management):** Mengolah material, membangun modul baru, dan mengotomatisasi pertanian di atas kapal yang sedang melaju.
3.  **Farming (Manual Start):** Di awal, pemain menyiram manual (Watering Can), membuat kompos manual. Terasa berat dan lambat (sengaja, untuk memicu keinginan optimasi).
4.  **Exploration (Resource & Data):** Turun ke planet untuk menambang mineral mentah dan memindai flora lokal untuk data riset.
5.  **Fabrication (Engineering):** Menggunakan data eksplorasi untuk membuka Blueprint mesin baru. Mengolah mineral menjadi komponen konstruksi di kapal.
6.  **Automation (The Payoff):** Membangun sistem (Pipa, Conveyor, Rover) untuk menggantikan tugas manual. Goal: Melihat kebun berjalan sendiri dengan efisiensi 100%.
7.  **Economy & Progression:**
    * **Credits:** Untuk alat fisik, bahan bakar, dan upgrade komponen kapal.
    * **Trust (Reputation):** Diperoleh dengan mengirimkan logistik pangan ke koloni. Digunakan untuk membuka Blueprint teknologi tinggi.
    * **Trading Post:** BUKAN NPC fisik. Trading Post adalah mesin (Logistics Capsule / Comm-Terminal) tempat pemain memasukkan barang dan mengakses UI pertukaran. Mendukung imersi "Lone Innovator".

## 4. Gameplay & Sistem

### 4.1. Sistem Aktif
- **Sistem Waktu & Tidur:** Siklus Siang/Malam berlangsung selama 15 menit per hari dengan pencahayaan global dinamis. Pemain tidak diwajibkan untuk tidur, namun jika terjaga berhari-hari, karakter akan mengalami **Fatigue** (konsumsi O2 & Energi lebih boros). Tidur di kasur mereset debuff ini.
- **Sistem Sumber Daya:** Manajemen Oksigen (O2), Energi, dan Air.
- **Inventaris:** Penyimpanan berbasis slot (64x64) + hotbar untuk akses cepat.
- **Manajemen Daya:** Distribusi energi nirkabel (Generator & Baterai).
- **Sistem Ekonomi Ganda:** 
    - **Contribution Points (Trust):** Diperoleh via Shipping Bin resmi untuk proyek utama.
    - **Credits (Mata Uang Pribadi):** Diperoleh dari perdagangan independen.

### 4.2. Sistem Eksplorasi & Pemetaan
- **Auto-Discovery (Scanner Pasif):** Otomatis mengenali flora/fauna baru saat masuk layar dan memberikan Research Points.
- **Peta Dunia (Fog of War):** Tidak ada minimap. Peta tertutup kabut dan terbuka saat dijelajahi secara fisik. Diakses via tablet holografik ([M]).

### 4.3. Sistem Otomatisasi & Logistik
- **Sistem Grid (Dual Grid System):** 
    - **Macro-grid (1x1 besar):** Untuk bangunan utama, mesin, dan kandang.
    - **Micro-grid (sub-grid kecil):** Untuk pipa, kabel, conveyor belt, dan dekorasi.
    - **Closed-Loop Detection:** Pagar yang terhubung penuh otomatis teregistrasi sebagai "Kandang" (Enclosure).
- **Irigasi & Transportasi Lokal:** Desain pipa air (Sprinkler) dan rute Drone/Rover (Bot-E).
- **Logistik Antar-Planet:** Inter-Planetary Pad untuk menerima pengiriman otomatis dari base planet lain.

### 4.4. Oksigen & Bahaya Lingkungan (Cozy Survival — No Death)
- **Mekanik O2:** O2 menurun saat eksplorasi. Jika 0%, layar meredup dan pemain diselamatkan oleh drone kembali ke tempat tidur (**Walk of Shame** tanpa penalti item).
- **Flora Penghasil Oksigen (Oxy-Flora):** Tanaman khusus menciptakan "oasis udara" untuk mengisi O2.
- **Area Tanpa Cahaya (Shadow Canyons):** Area gelap tanpa sinar matahari. Flora tidak tumbuh, panel surya mati. Solusi: Pilar cahaya buatan (UV Light Pillar) yang ditarik dari jaringan listrik area terang.

### 4.5. Sistem Peternakan & Fauna (Animal Husbandry)
- **Penjinakan & Kandang:** Fauna dapat dijinakkan dan ditempatkan di enclosure yang dibangun di atas world grid.
- **Efisiensi Ruang Eksponensial:** Kapasitas kandang tidak linier. Contoh: 1 ekor butuh 3 tiles, tapi 10 tiles bisa menampung 5 ekor.
- **AI Fauna:** Menggunakan pathfinding di dalam batas enclosure. Memiliki behavioral states (mencari makan, tidur, berkeliaran).
- **Panen Produk Fauna:** Memanen susu alien, bulu, dll menggunakan alat khusus. Kesejahteraan fauna (makanan, kebahagiaan) mempengaruhi kualitas panen.

### 4.6. Sistem Pengumpulan Sumber Daya Lingkungan (Resource Gathering)
- **Mining & Logging (Tambang & Penebangan):** Menggunakan alat (Tools) untuk menghancurkan mineral dan pohon yang menjatuhkan loot. Objek dapat respawn.
## 5. Lingkungan & Alam (Planetary Biomes & Variations)

### 5.1. Grassland (Padang Rumput)
- **Planet A (Starter):** Pink Grassland. Rumput merah muda, pohon ungu. Aman, cuaca cerah. (Space Potato, Alien Wood)
- **Planet B (Es):** Frost-Meadow. Rumput beku kebiruan. Badai salju kecil. (Ice Shards, Frost-Berries)
- **Planet C (Panas):** Ash-Grassland. Rumput abu vulkanik. Geyser panas. (Bio-Mass kering, Sulfur)

### 5.2. Desert (Gurun)
- **Planet A:** Blue Desert. Pasir biru neon, kaktus bercahaya. (Silicon Sand, Copper Ore)
- **Planet B:** Tundra Wastes. Gurun salju kering. Angin kencang. (Frozen Quartz)
- **Planet C:** Glass Desert. Pasir meleleh menjadi kaca. (Glass Sand, Gold Ore)

### 5.3. Canyons & Caves (Lembah & Gua Gelap)
- **Planet A:** Shadow Canyons. Lembah gelap dengan jamur fosfor (Star-Shroom). (Iron Ore, Lumi-Bug)
- **Planet B:** Abyssal Ice Caves. Gua es tanpa cahaya. (Cryo-Crystals)
- **Planet C:** Magma Trenches. Lembah curam dengan aliran lava. (Obsidian)

## 6. VISUAL & ART DIRECTION (Final Consensus)

### 4.1. The Ship: "Industrial Lab"
* **Style Utama:** Clean 16-Bit Solarpunk. Optimis, bersih, minim tekstur noise (shading halus), dan ramah lingkungan.
* **Visual Pillars:**
    * **Industrial Precision:** Aerospace engineering aesthetic (clean lines, modular construction, exposed but organized cabling).
    * **Symbiotic Technology:** Technology that mimics or integrates with nature (e.g., leaf-shaped solar panels, vine-wrapped circuits).
    * **Holographic Interfaces:** Projected digital displays floating near organic machinery (HUDs, tracking monitors).
    * **Bio-Luminescence:** Living light sources, glowing organic elements (Biolume green accents).
    * **Verdant Integration:** Plants growing on/through technology (moss on machines), but controlled and maintained.
* **Exterior:** Bodi komposit dengan garis panel presisi tinggi. Aerodinamis & Modern. Fixed Silhouette.
* **Interior:** Kontras antara teknologi bersih (putih/logam) dengan elemen organik (tanaman, panel kayu sintetis, pencahayaan sirkadian).
* **Color Palette:**

| Category | Name | Hex | Usage |
|----------|------|-----|-------|
| **Tech/Structure** | Lab White | `#F0F4F8` | Main walls, panels |
| | Deep Space | `#2B2E3B` | Technical background |
| | Industrial Grey| `#78909C` | Machinery parts |
| **Base** | Bio-Lume Green | `#99e550` | Glow effects, energy |
| | Sage Green | `#9DC183` | Crop foliage |
| | Soil Dark | `#4A3F35` | Ground base |
| **Material** | Copper Main | `#B87333` | Tool bodies, tech frames |
| | Living Wood | `#8B7355` | Organic handles |
| | Warm Amber | `#FFB347` | UI glow, warmth |
| **Crops** | Space Potato | `#8B7BA8` | Primary color |
| | Neon Carrot | `#FF8C00` | Primary color |
| **VFX** | Bio glow | `#99e550` | Energy active |
| | Hologram Blue | `#00F3FF` | Digital projections |

* **Material Reference:**
    * **Aerospace Composites:** Clean white/light grey panels, matte finish.
    * **Holographic Glass:** Semi-transparent, projecting digital data.
    * **Copper/Brass:** Tool bodies, machine frames (Warm patina) - *Accent Only*.
    * **Living Wood:** Handles, organic parts - *Accent Only*.
    * **Biolume:** Energy, glow, active elements (`#99e550`).

## 7. Kebutuhan Aset & Tabel Data Teknis

### 7.1. Flora & Pertanian (Crops & Farming)
| Nama | Kegunaan | Syarat | Tumbuh |
| :--- | :--- | :--- | :--- |
| **Space Potato** | Umbi dasar konsumsi | Tanah, Air | 2 hari (Sekali) |
| **Neon Carrot** | Sayuran bercahaya | Tanah, Air | 3 hari (Sekali) |
| **Iron Root** | Ekstrak mineral besi | Tanah keras, Air | 5 hari (Sekali) |
| **Crystal Wheat** | Tepung alien | Tanah, Air | 4 hari (Sekali) |
| **Star-Shroom** | Jamur nutrisi tinggi | Area gelap/gua | 3 hari (Sekali) |
| **Flux Berry** | Buah semak | Tanah, Air | 4 hari (Berkala) |
| **Aqua-Melon** | Buah kaya air (2x2) | Tanah basah | 5 hari (Berkala) |
| **Solar-Vine** | Bio-Mass tinggi | Trellis | 6 hari (Berkala) |
| **Oxy-Bulb** | Radius O2 3x3 | Bio-Fertilizer, Air | Utilitas |
| **Solar-Flower** | Daya listrik siang (10W) | Sinar matahari | Utilitas |
| **Purify-Fern** | Bersihkan tanah beracun | - | Utilitas |
| **Lume-Tree** | Cahaya alami (2x2) | - | Utilitas |

### 7.2. Fauna & Peternakan (Entities & Animal Husbandry)
| Nama | Produk | Grid Rule (Ekor=Tiles) |
| :--- | :--- | :--- |
| **Moo-Lien** | Astro-Milk | 1=3, 2=5, 5=10 |
| **Wooly-Pod** | Alien Wool | 1=2, 3=4, 5=6 |
| **Lumi-Bug** | Bio-Resin | 1=1, 5=3, 10=5 |
| **Hover-Fowl** | Alien Egg & Bulu | 1=1, 5=3 |
| **Slime-Snail** | Industrial Slime | 1=2 |
| **Geo-Tortoise** | Menggali Ore | 1=4 |

### 7.3. Material & Sumber Daya
- **Raw:** Copper, Iron, Titanium, Gold, Bio-Mass, Alien Wood, Silicon Sand, Ice Shards.
- **Refined:** Copper Wire, Iron Ingot, Titanium Plate, Glass, Plastic, Machine Gears.
- **Electronics:** Microchip, CPU, Quantum Core.
- **Utility:** Compost, Bio-Fertilizer, Bio-Fuel, O2 Canister.

### 7.4. Struktur & Otomatisasi
- **Pertanian:** Water Pump, Pipes, Sprinkler, Composter, Seed Maker, Hydroponic Basin.
- **Tenaga:** Solar Panel, Wind Turbine, Bio-Generator, Battery Bank, Power Node.
- **Logistik:** Fabricator, Smelter, Assembler, Conveyor Belt, Auto-Miner.
- **Fauna:** Energy Fence, Feeding Trough, Auto-Harvester, Incubator.
- **Utilitas:** O2 Emitter, UV Light Pillar, Sleep Pod (Save).

### 7.5. Alat & Peralatan (Tools & Equipment)
- **Tools:** Multi-Tool, Scanner, Watering Can, Hoe.
- **Equipment:** O2 Tank Upgrade, Exo-Boots, Jetpack, Hazard Suit, Headlamp.

---

## 📋 Project Information
- **Engine:** Unity 2022.3.62f1 (LTS)
- **Platform:** Windows (x64)
- **Visual:** Pixel Art Top-Down 2D (PPU: 16)
- **Input:** Mouse & Keyboard (WASD + Raycast Interaction)
