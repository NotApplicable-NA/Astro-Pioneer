# Sprint 7: Art Asset Handoff — Ship & Exploration

**From:** Developer
**To:** Art Director
**Date:** 2026-03-01
**GDD Version:** 3.5 Rev.2 (PPU 16, Clean 16-Bit Solarpunk, No HP System)

---

## ⚙️ Technical Specs

| Property | Value |
|----------|-------|
| **PPU** | 16 |
| **Grid Cell** | 16x16 px |
| **Player Scale** | 1x2 grid (16x32 px) |
| **Bot/Machine Scale** | 1x1 grid (16x16 px) |
| **Large Machine** | 2x1 or 2x2 grid (32x16 or 32x32 px) |
| **Direction** | 4-directional (S, W, N, E) — South = default |
| **Format** | PNG, transparent BG |

---

## 🚀 SHIP INTERIOR ASSETS

### S7-ART-01: Ship Interior Floor Tile
- **Size:** 16x16 px (tileable)
- **Style:** Clean aerospace metal flooring, subtle grid lines
- **Colors:** Lab White `#F0F4F8`, Industrial Grey `#78909C`
- **Notes:** This is the base walkable surface inside the ship

### S7-ART-02: Ship Wall Tile
- **Size:** 16x16 px (tileable, 4 edges: top/bottom/left/right + corners)
- **Style:** Thick metallic boundary, visible rivets, panel seams
- **Colors:** Deep Space `#2B2E3B`, Industrial Grey `#78909C`

### S7-ART-03: Locked Cell Overlay
- **Size:** 16x16 px
- **Style:** Dark/crossed-out grid, "not yet unlocked" look
- **Colors:** Semi-transparent dark `#2B2E3B` at 60% opacity with X pattern
- **Notes:** Placed over areas player hasn't expanded/purchased yet

### S7-ART-04: Room Icons (4 types)
- **Size:** 16x16 px each
- **Types:**
  1. **Bridge** — steering wheel / nav console icon
  2. **Engine** — gear / turbine icon
  3. **Greenhouse** — plant / leaf icon
  4. **Storage** — crate / box icon
- **Colors:** Per room accent, Bio-Lume Green `#99e550` glow hints

---

## 🌍 PLANET EXPLORATION ASSETS

### S7-ART-05: Desert Planet Tileset
- **Size:** 16x16 px per tile (tileable surface)
- **Variants needed:** Sand base, rocky sand, cracked ground, small rocks
- **Colors:** Warm sandy tones, Soil Dark `#4A3F35` accents
- **Notes:** First planet biome. Arid, minimal vegetation

### S7-ART-06: Resource Node — Rock
- **Size:** 16x16 px
- **Style:** Mineable rock/boulder, slightly angular, visible mineral veins
- **Colors:** Grey-brown base, Copper Main `#B87333` vein accents
- **Needs:** 3 damage states (full → cracked → almost broken)

### S7-ART-07: Resource Node — Crystal
- **Size:** 16x16 px
- **Style:** Glowing alien crystal cluster, faceted edges
- **Colors:** Hologram Blue `#00F3FF` base, Bio-Lume `#99e550` inner glow
- **Needs:** 3 damage states

### S7-ART-08: Resource Node — Alien Plant
- **Size:** 16x16 px
- **Style:** Strange, bioluminescent alien vegetation, tentacle-like leaves
- **Colors:** Sage Green `#9DC183`, Bio-Lume `#99e550` tips
- **Needs:** 3 damage states (full → wilting → harvested)

### S7-ART-09: Shadow Canyon Darkness Overlay
- **Size:** 16x16 px (tileable)
- **Style:** Dark fog/shadow overlay for Shadow Canyon zones (no sunlight areas)
- **Colors:** Deep Space `#2B2E3B` at 60-70% opacity, subtle dark blue `#1A1C2C` edges
- **Notes:** Binary system — area is Lit or Dark. This overlay marks Dark zones.
- **GDD Ref:** Section 3.4 — Shadow Canyons. No radiation/freezing/toxic/heat.

### S7-ART-09b: UV Light Pillar
- **Size:** 16x32 px (1x2 grid, tall structure)
- **Style:** Futuristic light pillar, glowing UV rings, Solarpunk industrial
- **Colors:** Hologram Blue `#00F3FF` glow, Industrial Grey `#78909C` body, Bio-Lume `#99e550` tip
- **Needs:** 2 states: OFF (no glow, dark) and ON (full UV glow)
- **GDD Ref:** UV Light Pillars menerangi Shadow Canyons, memungkinkan Oxy-Flora tumbuh

### S7-ART-09c: Oxy-Flora Plant
- **Size:** 16x16 px
- **Style:** Bioluminescent oxygen-producing plant, glowing aura
- **Colors:** Sage Green `#9DC183` body, Bio-Lume `#99e550` glow, Hologram Blue `#00F3FF` spore tips
- **Needs:** 3 growth stages (seed → growing → full bloom with O2 aura)
- **GDD Ref:** Section 3.3 — Oxy-Flora creates oxygen oases on planet surface

---

## 💗 UI / HUD ASSETS — HYBRID HD

> **Style:** Hybrid HD (referensi: Dave the Diver)
> - Bars & Panels → HD Vector, resolusi layar
> - Icons → Pixel art
> **GDD DESIGN LOCK:** Tidak ada HP system. Pemain tidak bisa mati.

### ~~S7-ART-10: HP Bar~~ ❌ DIHAPUS
> **Alasan:** GDD Section 3.3.1 — "Tidak ada HP/Health system. Pemain TIDAK BISA MATI."

### S7-ART-11: Oxygen Bar
- **Style:** HD Vector (Hybrid HD), BUKAN pixel art
- **Generate:** 512x64 px
- **Fill Colors:** Cyan `#00F3FF` gradient, bubble particles inside
- **Danger Zone:** 20% kiri = red tinted (threshold sebelum pingsan)
- **BG:** Deep Space `#2B2E3B`
- **GDD Ref:** O2 depletes di planet. 0% = pingsan. Bot-E rescue. No death.

### S7-ART-12: Warning Icon — Low O2 Only
- **Size:** 16x16 px (pixel art)
- **Type:** Air bubble with exclamation mark, Hologram Cyan `#00F3FF` + Warm Amber `#FFB347`
- **Note:** ~~Low HP icon dihapus~~ — tidak ada HP system

### S7-ART-13: Ship Upgrade Panel Background
- **Style:** HD Vector (Hybrid HD)
- **Generate:** 576x768 px
- **Colors:** Deep Space `#2B2E3B` base, Hologram Blue `#00F3FF` border glow, Warm Amber `#FFB347` accent lines
- **Note:** Center harus KOSONG (untuk diisi konten di code)

---

## 📋 DELIVERY CHECKLIST

| # | Asset ID | File Name | Size | Status |
|---|----------|-----------|------|--------|
| | **SHIP INTERIOR** | | | |
| 1 | S7-ART-01 | `ship_floor_tileset.png` | 16x16 set | ✅ Manual |
| 2 | S7-ART-02 | `ship_wall_tileset.png` | 16x16 15-tile | ✅ Manual |
| 3 | S7-ART-03 | `ship_locked_cell.png` | 16x16 | ⬜ |
| 4 | S7-ART-04 | `icon_room_*.png` | 16x16 x4 | ⬜ |
| | **PLANET EXPLORATION** | | | |
| 5 | S7-ART-05 | `desert_tileset.png` | 16x16 47-tile | ⬜ |
| 6 | S7-ART-06 | `node_rock.png` | 16x16 x3 | ⬜ |
| 7 | S7-ART-07 | `node_crystal.png` | 16x16 x3 | ⬜ |
| 8 | S7-ART-08 | `node_alien_plant.png` | 16x16 x3 | ⬜ |
| 9a | S7-ART-09 | `shadow_canyon_overlay.png` | 16x16 | ⬜ |
| 9b | S7-ART-09b | `uv_light_pillar.png` | 16x32 x2 | ⬜ |
| 9c | S7-ART-09c | `oxy_flora.png` | 16x16 x3 | ⬜ |
| | **UI / HUD (Hybrid HD)** | | | |
| ~~10~~ | ~~S7-ART-10~~ | ~~`ui_hp_bar.png`~~ | ~~❌~~ | ❌ No HP |
| 11 | S7-ART-11 | `ui_oxygen_bar.png` | HD 512x64 | ⬜ |
| 12 | S7-ART-12 | `icon_warning_o2.png` | 16x16 | ⬜ |
| 13 | S7-ART-13 | `ui_upgrade_panel.png` | HD 576x768 | ⬜ |

**Total: 14 asset groups (~28 individual files)**

---

**Save to:** `Assets/Art/Sprint7/` (subfolders: `Ship/`, `Planet/`, `UI/`)
