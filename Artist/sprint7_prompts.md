# Sprint 7: Art Prompts — Ship & Exploration

**Created by:** Art Director  
**Date:** 2026-03-01  
**GDD Version:** 3.5 (PPU 16, Clean 16-Bit Solarpunk)  
**Purpose:** Prompt-only handoff. User generates assets using these prompts.

---

## ⚠️ RULES UMUM

1. **AI tidak bisa output gambar kecil** — generate di resolusi besar, resize down pakai **Nearest Neighbor**
2. **Background:** Selalu `SOLID BRIGHT MAGENTA (#FF00FF)` — remove di post-processing
3. **Jangan mention "16x16 pixels" ke AI** — cukup bilang "pixel art" dan biarkan AI generate besar
4. **Jangan pakai style Solarpunk** untuk tile polos (floor/wall) — itu bikin AI taro elemen hijau/coklat random
5. **Per-prompt max 4 tile** — AI paling konsisten generate 4 gambar dalam 1 strip

---

## 🚀 SECTION A: SHIP INTERIOR — 15-Tile Bitmask

> **Format:** 15-tile + 1 center = **16 tile total**
> **Bitmask:** 4-bit kardinal (N=1, E=2, S=4, W=8)
> **Penggunaan:** Unity Rule Tile / Tilemap autotile untuk dinding kapal

### Referensi Layout 15-Tile

Setiap tile ditentukan oleh neighbor mana yang JUGA merupakan wall:

```
Bit values: N=1, E=2, S=4, W=8

 0 = Isolated (no neighbors)     8 = W only
 1 = N only                      9 = N+W (corner ┘)
 2 = E only                     10 = E+W (horizontal ─)
 3 = N+E (corner └)             11 = N+E+W (T-up ┴)
 4 = S only                     12 = S+W (corner ┐)
 5 = N+S (vertical │)           13 = N+S+W (T-left ┤)
 6 = E+S (corner ┌)             14 = E+S+W (T-down ┬)
 7 = N+E+S (T-right ├)          15 = All (center ┼)
```

### ~~Ship Floor Tileset~~ ✅ DIBUAT MANUAL DI ASEPRITE

### Ship Wall Tileset (15+1 = 16 tiles, 4 batches)

> **Style untuk SEMUA batch wall:** Thick dark metallic wall. Deep Space (#2B2E3B) base, Industrial Grey (#78909C) highlights, small rivet dots. NO green, NO copper, NO glow.

**S7-ART-02 Batch 1 (tiles 0-3):**
> `A PIXEL ART SPRITE SHEET showing 4 spaceship wall tiles in a horizontal row. VIEWED FROM DIRECTLY ABOVE (bird's eye / top-down view). Each tile is a flat SQUARE. Style: pixel art, retro SNES game. Chunky visible pixels. NO anti-aliasing.
> These are wall tiles for a TOP-DOWN 2D game. You are looking STRAIGHT DOWN at the floor plan. Walls appear as THICK DARK BARS or LINES inside the square tile — NOT as tall pillars or columns from the side.
> Colors: Deep Space (#2B2E3B) for wall, Industrial Grey (#78909C) for edges/rivets, Light (#F0F4F8) for empty floor area. NO green, NO copper, NO glow.
> Tile 1 — ISOLATED: A thick dark square block in the center of the tile. No connections to any edge. Like a lone wall dot on a floor plan.
> Tile 2 — NORTH only: A thick dark bar extending from the CENTER to the TOP edge of the tile. Bottom half is empty floor.
> Tile 3 — EAST only: A thick dark bar extending from the CENTER to the RIGHT edge. Left half is empty floor.
> Tile 4 — NORTH+EAST corner (└ shape): A thick dark L-shape — bar goes from CENTER up to TOP edge AND from CENTER right to RIGHT edge. Bottom-left area is empty floor.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

**S7-ART-02 Batch 2 (tiles 4-7):**
> `A PIXEL ART SPRITE SHEET showing 4 spaceship wall tiles in a horizontal row. VIEWED FROM DIRECTLY ABOVE (bird's eye / top-down view). Each tile is a flat SQUARE. Style: pixel art, retro SNES game. Chunky visible pixels. NO anti-aliasing.
> TOP-DOWN floor plan view. Walls are THICK DARK BARS — NOT pillars. Deep Space (#2B2E3B) wall, Industrial Grey (#78909C) edges, Light (#F0F4F8) floor. NO green, NO copper, NO glow.
> Tile 1 — SOUTH only: Thick dark bar from CENTER to BOTTOM edge. Top half is empty floor.
> Tile 2 — NORTH+SOUTH vertical (│ shape): Thick dark vertical bar running from TOP edge to BOTTOM edge through the center. Left and right sides are empty floor.
> Tile 3 — EAST+SOUTH corner (┌ shape): Thick dark L-shape — bar goes from CENTER down to BOTTOM edge AND from CENTER right to RIGHT edge. Top-left area is empty floor.
> Tile 4 — NORTH+EAST+SOUTH T-junction (├ shape): Thick dark bar from TOP to BOTTOM (vertical), with a branch extending RIGHT from the center. Only the left side has empty floor.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

**S7-ART-02 Batch 3 (tiles 8-11):**
> `A PIXEL ART SPRITE SHEET showing 4 spaceship wall tiles in a horizontal row. VIEWED FROM DIRECTLY ABOVE (bird's eye / top-down view). Each tile is a flat SQUARE. Style: pixel art, retro SNES game. Chunky visible pixels. NO anti-aliasing.
> TOP-DOWN floor plan view. Walls are THICK DARK BARS. Deep Space (#2B2E3B) wall, Industrial Grey (#78909C) edges, Light (#F0F4F8) floor. NO green, NO copper, NO glow.
> Tile 1 — WEST only: Thick dark bar from CENTER to LEFT edge. Right half is empty floor.
> Tile 2 — NORTH+WEST corner (┘ shape): Thick dark L-shape — bar from CENTER up to TOP edge AND from CENTER left to LEFT edge. Bottom-right is empty floor.
> Tile 3 — EAST+WEST horizontal (─ shape): Thick dark horizontal bar running from LEFT edge to RIGHT edge through the center. Top and bottom are empty floor.
> Tile 4 — NORTH+EAST+WEST T-junction (┴ shape): Thick dark horizontal bar LEFT to RIGHT, with a branch extending UP from center. Only the bottom has empty floor.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

**S7-ART-02 Batch 4 (tiles 12-15):**
> `A PIXEL ART SPRITE SHEET showing 4 spaceship wall tiles in a horizontal row. VIEWED FROM DIRECTLY ABOVE (bird's eye / top-down view). Each tile is a flat SQUARE. Style: pixel art, retro SNES game. Chunky visible pixels. NO anti-aliasing.
> TOP-DOWN floor plan view. Walls are THICK DARK BARS. Deep Space (#2B2E3B) wall, Industrial Grey (#78909C) edges, Light (#F0F4F8) floor. NO green, NO copper, NO glow.
> Tile 1 — SOUTH+WEST corner (┐ shape): Thick dark L-shape — bar from CENTER down to BOTTOM edge AND from CENTER left to LEFT edge. Top-right is empty floor.
> Tile 2 — NORTH+SOUTH+WEST T-junction (┤ shape): Thick dark vertical bar TOP to BOTTOM, with a branch extending LEFT from center. Only right side has empty floor.
> Tile 3 — EAST+SOUTH+WEST T-junction (┬ shape): Thick dark horizontal bar LEFT to RIGHT, with a branch extending DOWN from center. Only top has empty floor.
> Tile 4 — ALL SIDES cross (┼ shape): Thick dark cross/plus shape — bars extend from center to ALL 4 edges. Small floor corners visible in the 4 diagonal gaps.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

---

### Ship Overlay & Icons

**S7-ART-03: ship_locked_cell.png**
> `A single PIXEL ART overlay tile. Style: pixel art, retro SNES game. Chunky pixels.
> Subject: A "locked/unavailable" cell overlay. Dark semi-transparent fill with an X crossing from corner to corner. Deep Space (#2B2E3B) dark fill with two diagonal cross lines in lighter grey. Ultra-simple — just a dark square with an X.
> Draw ONLY the tile. No editor frame.
> Background: SOLID BRIGHT MAGENTA (#FF00FF).`

**S7-ART-04: Room Icons (4 icons)**
> `A PIXEL ART SPRITE SHEET showing 4 room icons in a horizontal row. Style: pixel art, retro SNES game. Bold simple shapes. Each icon is a small distinct symbol.
> NO Solarpunk glow — keep clean and simple.
> Icon 1 — BRIDGE: A simple steering wheel shape. White (#F0F4F8) with light blue (#00F3FF) accent.
> Icon 2 — ENGINE: A simple gear/cog shape. Grey (#78909C) with amber (#FFB347) accent.
> Icon 3 — GREENHOUSE: A simple leaf or sprout shape. Green (#9DC183) with lighter green (#99e550) tip.
> Icon 4 — STORAGE: A simple box/crate shape. Brown (#8B6914) with dark lines.
> Draw ONLY the icons. No frame.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

---

## 🌍 SECTION B: PLANET TERRAIN — 47-Tile Blob

> **Format:** 47-tile blob autotile
> **Bitmask:** 8-bit (4 kardinal + 4 diagonal, diagonal hanya dihitung jika kedua kardinal adjacent-nya aktif)
> **Penggunaan:** Unity Rule Tile untuk terrain planet desert

### Referensi Layout 47-Tile

47-tile blob menghasilkan transisi smooth antara terrain types. Tile dikategorikan:

```
CATEGORY A — EDGES (4 tiles):
  Top edge, Right edge, Bottom edge, Left edge

CATEGORY B — OUTER CORNERS (4 tiles):
  Top-left corner, Top-right corner, Bottom-left corner, Bottom-right corner

CATEGORY C — INNER CORNERS (4 tiles):
  Inner top-left, Inner top-right, Inner bottom-left, Inner bottom-right

CATEGORY D — T-JUNCTIONS & ENDS (16 tiles):
  Single connections, T-shapes, peninsulas

CATEGORY E — SPECIAL (19 tiles):
  U-shapes, single dots, thin lines, complex combinations

TOTAL: 47 unique tiles
```

> ⚠️ **CATATAN:** 47 tile = **12 sesi generate** (4 tile per sesi). Ini banyak.
> **STRATEGI:** Generate Category A-C dulu (12 tiles, 3 sesi) karena ini yang paling sering muncul. Category D-E bisa ditambah nanti.

### Desert Tileset — Priority Batch (12 core tiles, 3 prompts)

**S7-ART-05 Batch 1 — Edges (4 tiles):**
> `A PIXEL ART SPRITE SHEET showing 4 desert terrain edge tiles in a horizontal row. Style: pixel art, retro SNES game. Chunky visible pixels. NO anti-aliasing. Alien desert planet.
> Colors: Sandy Yellow (#D4A76A) for sand surface, Soil Dark (#4A3F35) for the void/border side.
> Each tile shows a desert sand surface meeting an edge — the sand fills PART of the tile, the rest is dark void/border.
> Tile 1 — TOP EDGE: Sand fills the bottom half, dark void on top. Smooth wavy border between them.
> Tile 2 — RIGHT EDGE: Sand fills the left half, dark void on right.
> Tile 3 — BOTTOM EDGE: Sand fills the top half, dark void on bottom.
> Tile 4 — LEFT EDGE: Sand fills the right half, dark void on left.
> All 4 must use the SAME sand texture and color. The border between sand and void should be slightly irregular (not a straight line).
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

**S7-ART-05 Batch 2 — Outer Corners (4 tiles):**
> `A PIXEL ART SPRITE SHEET showing 4 desert terrain outer corner tiles in a horizontal row. Style: pixel art, retro SNES game. Chunky visible pixels. NO anti-aliasing.
> Colors: Sandy Yellow (#D4A76A) for sand, Soil Dark (#4A3F35) for void.
> Each tile shows a corner where two edges of sand meet — sand fills ONE corner quadrant, the rest is void.
> Tile 1 — OUTER TOP-LEFT: Sand fills only the bottom-right area. Top and left are void. The sand border curves inward.
> Tile 2 — OUTER TOP-RIGHT: Sand fills only the bottom-left area. Top and right are void.
> Tile 3 — OUTER BOTTOM-LEFT: Sand fills only the top-right area. Bottom and left are void.
> Tile 4 — OUTER BOTTOM-RIGHT: Sand fills only the top-left area. Bottom and right are void.
> Same sand texture as edge tiles.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

**S7-ART-05 Batch 3 — Inner Corners + Center (4 tiles):**
> `A PIXEL ART SPRITE SHEET showing 4 desert terrain tiles in a horizontal row. Style: pixel art, retro SNES game. Chunky visible pixels. NO anti-aliasing.
> Colors: Sandy Yellow (#D4A76A) for sand, Soil Dark (#4A3F35) for void notch.
> Tile 1 — INNER TOP-LEFT: Sand fills MOST of the tile. A small dark void notch cuts into the top-left corner only.
> Tile 2 — INNER TOP-RIGHT: Sand fills most. Small void notch in top-right corner only.
> Tile 3 — INNER BOTTOM-LEFT: Sand fills most. Small void notch in bottom-left corner only.
> Tile 4 — INNER BOTTOM-RIGHT: Sand fills most. Small void notch in bottom-right corner only.
> Same sand texture. The notch should be a small curved triangle in the corner.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

**S7-ART-05 Bonus — Center + Variants (4 tiles):**
> `A PIXEL ART SPRITE SHEET showing 4 desert terrain tiles in a horizontal row. Style: pixel art, retro SNES game. Chunky visible pixels. NO anti-aliasing.
> Colors: Sandy Yellow (#D4A76A) base.
> Tile 1 — CENTER/FILL: Full sand tile (no edges, no void). Plain sandy surface with subtle grain.
> Tile 2 — VARIANT A: Full sand with a few small rocky pebbles (grey-brown #7A6855 dots).
> Tile 3 — VARIANT B: Full sand with slight color variation (lighter patches).
> Tile 4 — VARIANT C: Full sand with faint crack lines (Soil Dark #4A3F35).
> These are detail variants for large sand areas to avoid repetition.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

> 📝 **Remaining 47-tile tiles (35 more):** Akan ditambahkan setelah core tiles berhasil di-generate. Category D-E mencakup thin strips, U-shapes, peninsulas, dan single dots yang lebih jarang muncul di gameplay.

---

## 🪨 SECTION C: RESOURCE NODES & OVERLAYS

### S7-ART-06: node_rock.png (3 damage states)
> `A PIXEL ART SPRITE SHEET showing 3 versions of a mineable rock in a horizontal row. Style: pixel art, retro SNES game. Chunky visible pixels. Alien planet.
> Tile 1 — FULL: A solid angular rock/boulder. Grey-brown (#7A6855) base with Copper (#B87333) mineral vein lines. Intact.
> Tile 2 — CRACKED: Same rock with dark crack lines. Some chunks chipped off edges.
> Tile 3 — ALMOST BROKEN: Mostly shattered, only small fragments. Much smaller.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

### S7-ART-07: node_crystal.png (3 damage states)
> `A PIXEL ART SPRITE SHEET showing 3 versions of a glowing alien crystal cluster in a horizontal row. Style: pixel art, retro SNES game. Chunky visible pixels.
> Tile 1 — FULL: 2-3 crystal shards pointing up. Hologram Blue (#00F3FF) with green (#99e550) glow at base.
> Tile 2 — CRACKED: Same cluster with fracture lines. One shard chipped. Glow dimmer.
> Tile 3 — ALMOST BROKEN: One small shard remaining. Glow faint.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

### S7-ART-08: node_alien_plant.png (3 damage states)
> `A PIXEL ART SPRITE SHEET showing 3 versions of an alien plant in a horizontal row. Style: pixel art, retro SNES game. Chunky visible pixels.
> Tile 1 — FULL: 2-3 tentacle-like curling leaves. Sage Green (#9DC183) with bright green (#99e550) tips. Vibrant.
> Tile 2 — WILTING: Drooping, muted colors, dimmer glow.
> Tile 3 — HARVESTED: Only a small stump remaining.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

### S7-ART-09a: Shadow Canyon Darkness Overlay — ✋ MANUAL DI ASEPRITE

> **GDD Design Lock (Section 3.4):** Game TIDAK punya hazard damage zones.
> Tantangan planet = **logistik** (Shadow Canyons, light-based puzzles).
> Shadow Canyon = zona gelap binary (Lit / Dark). Simple dark overlay.

> **Buat manual** — sama seperti hazard, ini hanya overlay gelap sederhana.
> **Ukuran:** 16x16px. **Warna:** Deep Space `#2B2E3B` fill, dark blue `#1A1C2C` edge wisps.
> **Opacity:** 60-70% — area harus terasa gelap tapi ground masih sedikit terlihat.
> **Atau:** Buat fully opaque, atur di Unity via `SpriteRenderer.color.alpha = 0.65f`.

---

### S7-ART-09b: UV Light Pillar (2 states)
> `A PIXEL ART SPRITE SHEET showing exactly 2 small sprites side by side on a magenta background. 3/4 TOP-DOWN RPG PERSPECTIVE (like SNES RPG games — slightly angled so you can see the front and top of the object). Style: pixel art, retro SNES game. Chunky visible pixels. NO anti-aliasing.
> Draw ONLY the 2 sprites. NO title text, NO labels, NO descriptions, NO infographic.
> Sprite 1 — An unpowered metal light pillar. Tall cylindrical shape, you can see its height. Industrial Grey (#78909C) metal body with dark rings. Deep Space (#2B2E3B) cap on top. No glow. Dark and inactive.
> Sprite 2 — Same pillar but POWERED ON. Same grey metal body but now with bright Hologram Blue (#00F3FF) glowing horizontal rings wrapping around the pillar, and a Bio-Lume Green (#99e550) glowing light at the top. Bright, lit up.
> Each sprite should be small and simple — this is a game tile sprite.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

---

### S7-ART-09c: Oxy-Flora Plant (3 growth stages)
> `A PIXEL ART SPRITE SHEET showing 3 versions of an alien oxygen-producing plant in a horizontal row. Style: pixel art, retro SNES. Chunky visible pixels.
> Subject: A bioluminescent plant that produces breathable oxygen on planet surfaces. Creates "oxygen oases."
> Stage 1 — SEED: Tiny green sprout, 2-3 pixels tall. Sage Green (#9DC183). Minimal.
> Stage 2 — GROWING: Medium plant with small bell-shaped leaves opening. Sage Green body, faint Bio-Lume (#99e550) glow starting.
> Stage 3 — FULL BLOOM: Fully grown, bell-shaped translucent leaves glowing brightly. Bio-Lume Green (#99e550) glow, Hologram Blue (#00F3FF) bubble particles rising. Vibrant, life-giving aura.
> GDD: Oxy-Flora creates oxygen oases, replacing need for mechanical oxygen infrastructure.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

---

## 💗 SECTION D: UI / HUD ASSETS — HYBRID HD

> **Style:** Hybrid HD (referensi: Dave the Diver)
> - **Bars & Panels** → HD Vector, resolusi layar, clean & modern
> - **Icons** → Pixel art (tetap)
> - **Background:** SOLID BRIGHT MAGENTA (#FF00FF) untuk semua asset

> ⚠️ **GDD Design Lock (Section 3.3.1):**
> - **TIDAK ADA HP bar** — Player tidak bisa mati
> - O2 depletes saat keluar kapal/area aman
> - 0% O2 = pingsan, Bot-E rescue ke kasur → "Walk of Shame"
> - ZERO penalties (no item loss, no time skip)

---

### ~~S7-ART-10: ui_hp_bar.png~~ — ❌ CANCELLED (Design Lock: No HP)

---

### S7-ART-10: ui_oxygen_bar.png
> **Style:** HD Vector, BUKAN pixel art. **Generate:** 512x64px.
> **GDD Ref:** Satu-satunya survival meter. Tangki oksigen depletes di planet.

> `A GAME UI OXYGEN TANK BAR. Style: Hi-fi vector, clean modern sci-fi. Smooth gradients allowed. Size: 512x64 pixels.
> SHAPE: Horizontal capsule/rounded rectangle. Slightly cylindrical — like a pressurized tank gauge.
> OUTER FRAME: Thin border in Deep Space (#2B2E3B) with subtle Hologram Blue (#00F3FF) inner glow.
> FILL: Hologram Cyan (#00F3FF) gradient fill, lighter in center. Small floating bubble particle dots (#B0E0FF white-blue) scattered inside the fill area for an oxygen/air feel.
> DANGER ZONE: The leftmost 20% of the bar interior has a subtle warm red (#FF4444) tinted border or hatch marks — indicating the "low oxygen" danger threshold.
> LEFT ICON: A small wind/air icon or O2 molecule symbol in white (#F0F4F8) embedded in the left end.
> Solarpunk aesthetic — clean, futuristic, breathable feel.
> Draw ONLY the bar element, nothing else.
> Background: SOLID BRIGHT MAGENTA (#FF00FF).`

---

### S7-ART-11: Warning Icon — Low O2 — PIXEL ART
> **Style:** Pixel art. **Generate:** 128x128px (1 icon).
> **Note:** HP warning icon CANCELLED (Design Lock: No HP).

> `A single PIXEL ART WARNING ICON. Style: pixel art, bold, chunky pixels, high contrast. Readable at small size. Retro SNES.
> Subject: LOW OXYGEN warning. A pixel art air bubble with an exclamation mark (!) inside or next to it. Hologram Cyan (#00F3FF) bubble, Warm Amber (#FFB347) exclamation mark. Represents "running out of breathable air." Urgent feel.
> Draw ONLY the icon, bold and clear.
> Background: SOLID BRIGHT MAGENTA (#FF00FF). Output: ONE SINGLE static image.`

---

### S7-ART-12: ui_upgrade_panel.png
> **Style:** HD Vector. **Generate:** 576x768px.

> `A GAME UI PANEL BACKGROUND. Size: 576x768 pixels. Style: HI-FI VECTOR (NOT pixel art), Sci-Fi Solarpunk.
> CENTER must be COMPLETELY EMPTY — flat Deep Space (#2B2E3B).
> BORDER: thin rounded frame, Hologram Blue (#00F3FF) with soft glow.
> CORNERS: small copper (#B87333) bracket ornaments.
> TOP EDGE: thin Warm Amber (#FFB347) header band for title — no text.
> Solarpunk feel — organic curves meet industrial precision.
> NO content inside. Just the frame and empty dark interior.
> Background: SOLID BRIGHT MAGENTA (#FF00FF).`

---

## 📋 Generation Checklist

| # | Asset | Method | Status |
|---|-------|--------|--------|
| | **SHIP INTERIOR** | | |
| 1 | Ship Floor Tileset | ✋ Manual Aseprite | ✅ Done |
| 2 | Ship Wall Tileset (15-tile) | ✋ Manual Aseprite | ✅ Done |
| 3 | Locked Cell Overlay | AI Prompt | ⬜ |
| 4 | Room Icons (4) | AI Prompt | ⬜ |
| | **PLANET EXPLORATION** | | |
| 5a-d | Desert Tileset (47-tile core 16) | AI Prompt ×4 | ⬜ |
| 6 | Rock Node (3 states) | AI Prompt | ⬜ |
| 7 | Crystal Node (3 states) | AI Prompt | ⬜ |
| 8 | Alien Plant (3 states) | AI Prompt | ⬜ |
| 9a | Shadow Canyon Overlay | ✋ Manual Aseprite | ⬜ |
| 9b | UV Light Pillar (2 states) | AI Prompt | ⬜ |
| 9c | Oxy-Flora (3 stages) | AI Prompt | ⬜ |
| | **UI / HUD (Hybrid HD)** | | |
| ~~10~~ | ~~HP Bar~~ | — | ❌ No HP |
| 11 | O2 Bar (HD) | AI Prompt | ⬜ |
| 12 | Low O2 Warning Icon | AI Prompt | ⬜ |
| 13 | Upgrade Panel (HD) | AI Prompt | ⬜ |

---

## 🔄 POST-PROCESSING WORKFLOW

1. **Generate** → AI output (magenta background, large canvas)
2. **Crop** → Pisahkan setiap tile dari strip
3. **Resize Down** → Scale ke target size (16x16, 64x8, dll) pakai **Nearest Neighbor**
4. **Remove magenta** → Buat transparent
5. **Assemble tileset** → Susun ke layout yang benar untuk Unity Rule Tile
6. **Import to Unity** → PPU 16, Point filter, No compression

---

## 📁 Output Folder Structure
```
Assets/Art/Sprint7/
├── Ship/
│   ├── ship_floor_A.png
│   ├── ship_floor_B.png
│   ├── ship_wall_tileset.png     (16 tiles assembled into standard layout)
│   ├── ship_locked_cell.png
│   ├── icon_room_bridge.png
│   ├── icon_room_engine.png
│   ├── icon_room_greenhouse.png
│   └── icon_room_storage.png
├── Planet/
│   ├── desert_tileset.png        (47 tiles blob layout)
│   ├── node_rock.png             (3 damage states)
│   ├── node_crystal.png          (3 damage states)
│   ├── node_alien_plant.png      (3 damage states)
│   ├── shadow_canyon_overlay.png
│   ├── uv_light_pillar.png       (OFF + ON states)
│   └── oxy_flora.png             (3 growth stages)
└── UI/
    ├── ui_oxygen_bar.png         (HD 512x64)
    ├── icon_warning_o2.png
    └── ui_upgrade_panel.png      (HD 576x768)
```
