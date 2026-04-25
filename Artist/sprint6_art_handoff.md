# Sprint 6: Art Asset Handoff — Economy & Trading

**From:** Lead Developer  
**To:** Art Director  
**Date:** 2026-02-24  
**Sprint:** 6 — Economy & Trading  

---

## 📐 Tech Specs (Reminder)

- **PPU:** 32 (STRICT)
- **Filter:** Point (no filter)
- **Compression:** None
- **Style:** Sci-Fi Solarpunk (50/50 tech + nature)
- **Palette:** Consult existing art direction

---

## 🧪 Context: Crafting Recipes

These recipes define **what items exist** and thus **what icons are needed:**

| Recipe | Ingredients | Result | Station |
|--------|-------------|--------|---------|
| Astro Stew | 2x Space Potato + 1x Water | 1x Astro Stew | Hand Craft |
| Neon Salad | 2x Neon Carrot + 1x Water | 1x Neon Salad | Hand Craft |
| Bio Fuel Cell | 3x Space Potato | 1x Bio Fuel Cell | Processing Station |
| Nutrient Pack | 1x Astro Stew + 1x Neon Salad | 1x Nutrient Pack | Hand Craft |
| Sprinkler Mk2 | 2x Bio Fuel Cell + 1x Water | 1x Sprinkler Mk2 | Processing Station |

---

## 🎨 Required Assets (10 Total)

### Item Icons (5) — 32x32px Pixel Art

| # | Filename | Description | Visual Direction |
|---|----------|-------------|-----------------|
| 1 | `Icon_AstroStew.png` | Cooked potato dish | Holographic bowl with glowing purple-orange stew, steam wisps |
| 2 | `Icon_NeonSalad.png` | Carrot-based salad | Clear bio-dome bowl, neon-lit veggies (cyan/magenta tones) |
| 3 | `Icon_BioFuelCell.png` | Refined energy cell | Cylindrical cell with amber/green bioluminescent glow, circuit lines |
| 4 | `Icon_NutrientPack.png` | Packaged meal ration | Vacuum-sealed pouch, holographic nutrition label, compact shape |
| 5 | `Icon_SprinklerMk2.png` | Upgraded sprinkler | Tech-enhanced sprinkler head with copper accents and energy rings |

> [!TIP]
> Reference existing item icons (`Icon_SpacePotato`, `Icon_NeonCarrot`, `Icon_WateringCan`) for consistent style.

---

### Machine Sprites (2) — 32x32px / 32x64px Pixel Art

| # | Filename | Size | Description | Visual Direction |
|---|----------|------|-------------|-----------------|
| 6 | `Machine_ProcessingStation.png` | 32x32 | Bio-tech processing table | Organic copper base + glass tubes + green liquid flow, small holographic display |
| 7 | `Machine_TradingPost.png` | 32x64 | NPC trade terminal | Tall kiosk with holographic menu, plant-wrapped base, antenna with leaf motif |

> [!TIP]
> Reference existing machines (`Machine_WaterPump.png`, `Machine_SmallStorage.png`) for consistent scale and style.

---

### UI Assets (2) — Panel Backgrounds

| # | Filename | Size | Description | Visual Direction |
|---|----------|------|-------------|-----------------|
| 8 | `UI_CraftingPanel_BG.png` | 256x320 | Crafting interface BG | Holographic blueprint grid, wireframe elements, deep space (#2B2E3B) with Hologram Blue (#00F3FF) accents |
| 9 | `UI_TradingPanel_BG.png` | 256x320 | Trading shop BG | Terminal screen aesthetic, amber/green readout lines, subtle scan-line effect |

> [!IMPORTANT]
> UI panels use **Hi-Fi vector style** (not pixel art) — same style as existing `InventoryPanel` and `StoragePanel`.

---

### HUD Icon (1) — 16x16px

| # | Filename | Size | Description | Visual Direction |
|---|----------|------|-------------|-----------------|
| 10 | `Icon_TrustPoints.png` | 16x16 | Reputation/Trust icon | Star badge with holographic shimmer, complements existing `UI_Credits_Icon.png` |

---

## 📊 Summary Table

| Category | Count | Style | Priority |
|----------|-------|-------|----------|
| Item Icons | 5 | Pixel Art 32x32 | P1 — Blocks crafting system |
| Machine Sprites | 2 | Pixel Art 32x32/64 | P1 — Blocks processing & trading |
| UI Panels | 2 | Hi-Fi Vector | P2 — Can use placeholder |
| HUD Icons | 1 | Hi-Fi 16x16 | P2 — Can use placeholder |
| **Total** | **10** | | |

---

## 🔗 References
- Existing machine sprites: `Assets/Sprites/Machines/`
- Existing item icons: `Assets/Sprites/UI/`
- Art Direction: `Artist/art_director_handoff.md`
