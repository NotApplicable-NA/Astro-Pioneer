# GAME DESIGN DOCUMENT: ASTRO-PIONEER
**Version:** 3.5 (Design Revision — PPU 16, Clean 16-Bit Style, Rover Buddy)
**Date:** 19 December 2025
**Genre:** Cozy Automation / Survival Logistics
**Theme:** Industrial Solarpunk, Bio-Innovation, Sustainable Crisis Management

## 1. HIGH CONCEPT (Concept Lock)
Astro-Pioneer adalah game simulasi manajemen di mana pemain mengubah kapal survei industri yang kaku menjadi ekosistem pertanian otonom yang cerdas. Pemain berperan sebagai Innovator, Farmer, dan Engineer yang harus menyeimbangkan efisiensi produksi pangan dengan eksplorasi planet demi menyelamatkan koloni manusia.

## 2. NARRATIVE & LORE (Revised Logic)

### 2.1. The Backstory: "The Desperate Launch"
* **Situasi:** Krisis Bumi memuncak lebih cepat. Proyek kolonisasi jangka panjang dibatalkan demi solusi darurat.
* **The Ship ("The Hull"):** Menggunakan rangka Ex-Surveyor Class Ships (Kapal Industri) yang diretrofit seadanya. Mesinnya canggih, tapi interiornya minim.
* **Kondisi Awal:** Belum ada modul pertanian yang besar di kapal, hanya satu ruang kecil yang menjadi start pertanian. Tidak ada robot canggih siap pakai.
* **Misi:** "Eksplorasi, Beradaptasi, Produksi." Kapal diluncurkan dengan peralatan dasar. Pemain harus mengembangkan teknologi pertanian dan sistem Life Support sendiri di perjalanan.

### 2.2. Player Role: "The Field Innovator"
* Pemain adalah Active Operator, bukan ilmuwan pasif di balik meja.
* **Survival through Tech:** Pemain harus turun tangan langsung—mulai dari mencangkul tanah hingga mengelas instalasi interior kapal atau membangun pangkalan di permukaan planet.

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
    * **Trading & Trust Barrier:** Jika Trust turun, item logistik tertentu menjadi terkunci (locked). Ini memaksa pemain untuk kembali "berbuat baik" kepada koloni.

### Active Systems (Sprint 4)
- **Time System:** Day/Night cycle (5 min/day), global lighting.
- **Resource System:** Oxygen, Energy, Water management.
- **Inventory System:** Slot-based storage, hotbar integration.
- **Power Management:** Wireless energy distribution (Generators & Batteries).
- **Economy:** Selling via Shipping Bin, Credits tracking.

### 3.2. Automation Systems
* **Grid System:** Interior kapal berbasis grid untuk penempatan presisi.
* **Sprinklers & Pipes:** Manajemen aliran air. Pemain harus merancang tata letak pipa agar air dapat menjangkau semua crop.
* **Bot-E (Rover Buddy):** Bukan magic, butuh Charging Station dan rute efisien. Rover kecil yang bergerak di grid, mulai dari pengangkut sederhana hingga rover pemanen.

### 3.3. Resource Management (Solarpunk Twist)
* **Circular Economy:** Limbah tanaman (daun kering/buah busuk) BISA DIOLAH di Composter.
* **Energy Balance (The Dual-Grid):**
    * *Essential Power:* Sistem pendukung kehidupan (statis, tidak bisa mati).
    * *Automation Power:* Sistem otomatisasi (Rover, Conveyor) butuh daya aktif. Jika konsumsi melebihi produksi, sistem ini berhenti.
* **Power Indicators:** Lampu indikator (Hijau/Kuning/Merah) pada setiap modul dan status hover detail.
* **Oxygen Management (The Cozy Survival):**
    * *Depletion:* Saat pemain keluar dari kapal atau area aman untuk eksplorasi planet, tangki oksigen perlahan berkurang.
    * *The Rescue (No Penalty):* Jika oksigen mencapai 0%, pemain tidak mati. Karakter akan pingsan (layar memudar). **Bot-E (Rover Buddy)** akan datang menyelamatkan dan mengangkut pemain kembali ke kasur di dalam kapal. Waktu dunia tidak terpotong (tidak *skip* hari), *Credits* tidak berkurang, dan barang di *inventory* tetap utuh. Hukuman satu-satunya hanyalah keharusan berjalan kaki kembali ke lokasi eksplorasi terakhir (*Walk of Shame*).
    * *Oxy-Flora Expansion:* Untuk menjelajah lebih jauh, pemain dapat merekayasa genetika dan menanam bibit khusus penghasil oksigen (**Oxy-Flora**) di permukaan planet. Tanaman ini menciptakan "oase udara" alami yang mengisi ulang tangki pemain saat berada di dekatnya, menggantikan kebutuhan akan pilar/kabel oksigen mekanis.
### 3.4. Planetary Hazards (Cozy Logistics Challenge)
* **Shadow Canyons (Zona Gelap Ekstrem):**
    Sesuai dengan pilar desain Cozy Automation, planet tidak memiliki musuh atau bahaya mematikan, melainkan tantangan logistik lingkungan.

* **Konsep:** Terdapat area geografis tertentu di permukaan planet (seperti ngarai dalam atau gua) yang tidak terjangkau sinar matahari sama sekali.

* **Efek:** Karena ketiadaan cahaya matahari, bibit penyuplai oksigen (Oxy-Flora) tidak dapat ditumbuhkan secara natural di area ini. Selain itu, mesin dan Rover bertenaga surya akan kehilangan daya dan mati total.

* **Solusi Logistik:** Pemain harus membangun infrastruktur energi. Pemain perlu menarik jaringan listrik (menggunakan tiang baterai atau kabel estafet) dari area terang di luar ngarai, lalu memasang UV Light Pillars (Tiang Lampu UV) di dalam zona gelap. Cahaya UV buatan ini akan memicu pertumbuhan Oxy-Flora dan menyalakan mesin, perlahan mengubah zona gelap yang tidak bisa dieksplorasi menjadi pabrik tambang yang aman dan otomatis.

## 4. VISUAL & ART DIRECTION (Final Consensus)

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

## 5. PROGRESSION & SCALING (The "Sleeping Giant")

### 5.1. The Fabrication Process
* **Internal Machine:** Component Fabricator.
* **Input:** Raw Materials (Bijih Besi, Silika, Titanium).
* **Output:** Structural Components (Hull Plates, Frames, Glass Panels, Pipes).

### 5.2. Production Goals
* **Misi:** Memenuhi kuota logistik pangan/bibit ke koloni.
* **Reward:** Reputasi & Tech Data.

### 5.3. The Construction Process
* **Interior Evolution:** Kapal memiliki bentuk luar tetap, bagian dalamnya berbasis grid.
* **The Tool (Omni-Welder):** Alat genggam untuk memotong, memposisikan, dan mengelas (Fusion Welding).
* **Planetary Building (The Outpost):** Saat mendarat, pemain bisa membangun Planetary Outpost dengan kebebasan lebih tinggi (tangki air raksasa, gudang mineral, dll).

## 6. TECHNICAL DATA TABLES

### 6.1. Crop Database
| ID | Nama | Waktu Tumbuh | Harga Jual | Syarat Khusus |
| :--- | :--- | :--- | :--- | :--- |
| CRP_001 | Space Potato | 120s | 18 Cr | Water Only |
| CRP_002 | Neon Carrot | 300s | 60 Cr | Water Only |
| CRP_003 | Flux Berry | 600s | 250 Cr | UV Light |
| CRP_004 | Quantum Corn | 1200s | 550 Cr | High Fertilizer |
| CRP_005 | Iron Root | 450s | N/A | Menghasilkan Bijih Besi (Ore) |

### 6.2. Crafting & Automation Tiers
| ID | Item Name | Crafting Cost | Function | Tier |
| :--- | :--- | :--- | :--- | :--- |
| MCH_01 | Basic Sprinkler | 5x Iron, 2x Copper | Auto-water 4 tiles | Tier 1 |
| MCH_02 | Water Pump | 10x Iron, 5x Gear | Sumber air area 10x10 | Tier 1 |
| BOT_01 | Bot-E (Rover Buddy) | 15x Titanium, 5x Chip | Transport hasil panen | Tier 2 |
| AND_01 | Agri-Android | 50x Titanium, 10x CPU | Full cycle (Tanam-Panen) | Tier 3 |

## 7. TECHNICAL FOUNDATION
* [cite_start]**Grid System:** Class `GridCell` menyimpan status (Tanah, Lantai, Dinding) dan koordinat (x,y)[cite: 101].
* **Interaction:** Implementasi Physics2D. Raycast untuk deteksi mouse.
* **State Machine Tanaman:** Seed -> Growing -> Ready (Visual berubah, Harvest aktif).

## 8. UI/UX SPECIFICATIONS
* **Diegetic UI:** Status Oksigen dan Energi pada layar helm atau monitor dinding.
* **Inventory:** Sistem slot murni (64x64px), stacking limit 99.
* **Blueprint Mode:** Tampilan wireframe holografik saat merancang tata letak.

---

## 📋 Project Information

* **Engine:** Unity 2022.3.62f1 (LTS)
* **Platform:** Windows (x64)
* **Visual:** Pixel Art Top-Down 2D (PPU: 16)
* **Input:** Mouse & Keyboard (WASD + Raycast Interaction)
