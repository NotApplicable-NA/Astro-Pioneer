# Sprint 6: Art Prompts — Economy & Trading

**Created by:** Art Director  
**Date:** 2026-02-24 (Revised)  
**Purpose:** Prompt-only handoff. User generates assets using these prompts.

---

## ⚠️ STYLE DEFINITION — Paste at Start of EVERY Prompt

> **Apa itu Solarpunk dalam konteks ini:**
> Solarpunk = Futuristic Space Station aesthetic. Clean, optimistic technology. Nature influence is expressed through MATERIAL CHOICE (copper, brass, warm amber tones, organic rounded shapes) and ENERGY GLOW (bioluminescent green, warm amber). 
>
> **BUKAN berarti ada daun, tanaman, lumut, atau tanaman merambat yang tumbuh di mesin.** Tidak ada elemen botanical literal kecuali secara eksplisit diminta.

Masukkan blok style ini di awal SETIAP prompt:

```
Style: PIXEL ART, Sci-Fi Solarpunk. Solarpunk here means CLEAN FUTURISTIC TECH with warm organic MATERIALS (copper, brass, rounded shapes) and bioluminescent green energy glow. DO NOT add any leaves, vines, moss, or plants growing on the object. The machine should look like clean laboratory equipment, not overgrown ruins.
```

Untuk **UI/HUD assets (Hi-Fi Vector)**:
```
Style: HI-FI VECTOR (NOT pixel art), Sci-Fi Solarpunk. Clean futuristic interface with warm copper/amber tones and holographic blue accents. NO leaves, vines, or botanical elements.
```

---

## 📏 Rotation Rules

| Type | Rotations | Directions |
|------|-----------|------------|
| **Placeable (Static)** | 4 images | SE, SW, NW, NE |
| **Placeable (Moving)** | 8 images | SE, S, SW, W, NW, N, NE, E |
| **Item Icons / UI / HUD** | 1 image | Front-facing (no rotation) |

---

## 🍲 Batch 1: Item Icons (32x32px — No Rotation)

### 1. Icon_AstroStew.png
> `A single item icon sprite. Size: 32x32 pixels. Style: PIXEL ART, Sci-Fi Solarpunk. Solarpunk here means CLEAN FUTURISTIC TECH with warm organic MATERIALS (copper, brass, rounded shapes) and bioluminescent green energy glow. DO NOT add any leaves, vines, moss, or plants on the object.
> Subject: A cooked "Astro Stew" dish. A sleek copper bowl with a translucent blue-tinted rim. Filled with chunky purple-orange stew. Small steam wisps rising from the top. Warm amber glow (#FFB347).
> Background: SOLID WHITE (#FFFFFF).`

### 2. Icon_NeonSalad.png
> `A single item icon sprite. Size: 32x32 pixels. Style: PIXEL ART, Sci-Fi Solarpunk. Solarpunk here means CLEAN FUTURISTIC TECH with warm organic MATERIALS (copper, brass, rounded shapes) and bioluminescent green energy glow. DO NOT add any leaves, vines, moss, or plants on the object.
> Subject: A "Neon Salad" dish. A clear glass bio-dome bowl filled with neon-bright veggies in cyan and magenta tones. Small holographic particles above. Fresh and vibrant.
> Background: SOLID WHITE (#FFFFFF).`

### 3. Icon_BioFuelCell.png
> `A single item icon sprite. Size: 32x32 pixels. Style: PIXEL ART, Sci-Fi Solarpunk. Solarpunk here means CLEAN FUTURISTIC TECH with warm organic MATERIALS (copper, brass, rounded shapes) and bioluminescent green energy glow. DO NOT add any leaves, vines, moss, or plants on the object.
> Subject: A "Bio Fuel Cell". A small cylindrical energy cell. Body is dark metal (#2B2E3B) with amber (#FFB347) and green (#99e550) bioluminescent liquid visible through a glass window. Fine circuit lines etched on the surface. Looks like a small battery or capacitor.
> Background: SOLID WHITE (#FFFFFF).`

### 4. Icon_NutrientPack.png
> `A single item icon sprite. Size: 32x32 pixels. Style: PIXEL ART, Sci-Fi Solarpunk. Solarpunk here means CLEAN FUTURISTIC TECH with warm organic MATERIALS (copper, brass, rounded shapes) and bioluminescent green energy glow. DO NOT add any leaves, vines, moss, or plants on the object.
> Subject: A "Nutrient Pack" meal ration. A compact, vacuum-sealed foil pouch (silver/white). A small holographic nutrition label glows on the front (Hologram Blue #00F3FF). Clean, medical-grade packaging. Like a space MRE.
> Background: SOLID WHITE (#FFFFFF).`

### 5. Icon_SprinklerMk2.png
> `A single item icon sprite. Size: 32x32 pixels. Style: PIXEL ART, Sci-Fi Solarpunk. Solarpunk here means CLEAN FUTURISTIC TECH with warm organic MATERIALS (copper, brass, rounded shapes) and bioluminescent green energy glow. DO NOT add any leaves, vines, moss, or plants on the object.
> Subject: An "Upgraded Sprinkler Mk2". A sleek sprinkler head with copper (#B87333) body and glowing energy rings (Bio-Lume Green #99e550). More advanced than a basic sprinkler. Tiny antenna and circuit lines on the body. Looks like high-tech irrigation equipment.
> Background: SOLID WHITE (#FFFFFF).`

---

## 🏭 Batch 2: Machine Sprites (Placeable — 4 Rotations Each)

> **FORMAT:** 1x4 horizontal strip (turntable-style). Setelah generate, crop jadi 4 file.
> **PERSPEKTIF:** Top-down ¾ view (seperti Stardew Valley), BUKAN isometric.
> **ORIENTASI:** S, W, N, E (cardinal), BUKAN SE, SW, NW, NE (diagonal).
> **KUNCI:** Setiap sisi punya fitur visual UNIK agar AI terpaksa menggambar berbeda.

### 6. Machine_ProcessingStation (64x32px — occupies 2x1 grid tiles)

**Sprite sheet — 1 prompt, 4 views:**
> `A PIXEL ART SPRITE SHEET showing 4 FRONT-FACING views of the SAME machine in a horizontal row, left to right — like character sprites in an RPG (Stardew Valley style). Camera is OVERHEAD looking slightly down. Each view shows ONE face HEAD-ON (not diagonal). This is NOT isometric.
> Style: PIXEL ART with visible pixel grid. S/N views are 64x32px (wide, landscape). W/E views are 32x32px (square, showing the narrow side). Detailed and readable — use shading, highlights, and small interior details. NO anti-aliasing. Sci-Fi Solarpunk, MODERN, SLEEK — WHITE/SILVER body, copper accents only. NO leaves/vines/plants.
> Subject: A "Bio-Processing Station" — a WIDE workbench that is TWICE as wide as it is deep (2:1 ratio). White (#F0F4F8) body. Copper (#B87333) trim. Two glass tubes with green liquid (#99e550) on top.
> IMPORTANT — Each of the 4 faces has a UNIQUE feature. Do NOT repeat or swap features between views:
> - FRONT face: a WIDE CYAN TOUCHSCREEN (#00F3FF glow) — a glowing rectangle with visible data readout lines and small text inside the screen
> - BACK face: a THIN GREEN LED BAR (#99e550) — a horizontal strip of small glowing dots
> - LEFT face: a ROUND COPPER EXHAUST PORT — a dark circle with copper ring and inner shadow/depth
> - RIGHT face: a TALL GREEN GLASS CAPSULE — a vertical oval filled with green liquid and small bubbles inside
> The 4 sprites left to right:
> 1st — SOUTH (64x32, wide): Front face visible. BIG WIDE CYAN TOUCHSCREEN fills most of the face. Left edge: exhaust port peeking. Right edge: glass capsule peeking. TWO glass tubes visible on top (side by side). This is a WIDE landscape sprite.
> 2nd — WEST (32x32, square): Left face visible. ROUND COPPER EXHAUST PORT in center. Machine faces LEFT. Only ONE glass tube visible on top. This sprite is NARROWER because you see the short side of the machine.
> 3rd — NORTH (64x32, wide): Back face visible. THIN GREEN LED BAR in center. Left edge: glass capsule peeking. Right edge: exhaust port peeking. TWO glass tubes visible on top (side by side). Same wide shape as South.
> 4th — EAST (32x32, square): Right face visible. TALL GREEN GLASS CAPSULE in center. Machine faces RIGHT. Only ONE glass tube visible on top. Same narrow shape as West.
> All 4 = same machine, same size. Background: SOLID WHITE.`

---

### 7. Machine_TradingPost (32x64px per view)

**Sprite sheet — 1 prompt, 4 views:**
> `A PIXEL ART SPRITE SHEET showing 4 FRONT-FACING views of the SAME machine in a horizontal row, left to right. Each view shows the machine from a DIFFERENT cardinal direction — like character sprites in an RPG (Stardew Valley style). Camera is OVERHEAD looking slightly down so you can see the TOP surface of the machine. Each view shows ONE face of the machine HEAD-ON (not at a diagonal angle). This is NOT isometric — no diamond shapes.
> Style: VERY LOW RESOLUTION PIXEL ART like a 16-bit SNES game (32x64 pixels per view). Visible chunky square pixels. NO anti-aliasing, NO smooth gradients. Sci-Fi Solarpunk, MODERN, SLEEK — WHITE and SILVER body, copper accents only. NOT steampunk. NO leaves/vines/plants.
> Subject: A "Trading Post" kiosk — a tall, sleek terminal. White (#F0F4F8) body, rounded corners. Copper (#B87333) trim only. Keep VERY SIMPLE and BLOCKY — only 32x64 pixels, use flat colors. Small antenna on top. Grey (#78909C) base.
> The machine has 4 DISTINCT faces:
> - FRONT: a large cyan holographic menu display (#00F3FF glow)
> - BACK: a flat brushed metal panel with rectangular maintenance hatch
> - LEFT side: a narrow secondary screen showing amber text (#FFB347)
> - RIGHT side: a vertical green LED strip (#99e550) and rounded air vent
> The 4 sprites left to right:
> 1st — SOUTH (facing toward viewer): you see the FRONT face (menu display) head-on. Left edge = LEFT side (amber screen), Right edge = RIGHT side (LED strip). Top of machine visible.
> 2nd — WEST (facing left): you see the LEFT face (amber screen) head-on. The machine faces LEFT.
> 3rd — NORTH (facing away): you see the BACK face (maintenance hatch) head-on. Edges are mirrored from South view.
> 4th — EAST (facing right): you see the RIGHT face (LED strip + vent) head-on. The machine faces RIGHT.
> All 4 same machine, same size, same proportions. Background: SOLID WHITE.`

---

## 🖥️ Batch 3: UI Panels (Hi-Fi Vector — No Rotation)

> ⚠️ **PENTING:** Panel ini digunakan sebagai **background** untuk menu dalam game.
> Bagian TENGAH harus **KOSONG dan BERSIH** agar text/button bisa di-overlay di atasnya.
> Dekorasi HANYA boleh di **border dan corner**. Harus bisa dipotong 9-slice.

### 8. UI_CraftingPanel_BG.png (256x320px)
> `A GAME UI PANEL BACKGROUND designed for 9-slice stretching. Size: 256x320 pixels. Style: HI-FI VECTOR (NOT pixel art), Sci-Fi Solarpunk.
> This panel will have text, buttons, and item slots placed ON TOP of it in-game, so the CENTER AREA must be COMPLETELY EMPTY and CLEAN — just a flat dark background with no graphics, no patterns, no wireframes.
> - CENTER: flat solid Deep Space color (#2B2E3B), completely clean and empty
> - BORDER: a thin rounded-corner frame in Hologram Blue (#00F3FF) with subtle glow
> - CORNERS: small copper (#B87333) bracket ornaments at each corner (like sci-fi corner braces)
> - EDGES: very subtle, faint circuit-line decoration running along the border only
> The panel should look like a futuristic popup window. NO content, NO icons, NO text inside the panel. Just the frame and the empty dark interior.`

### 9. UI_TradingPanel_BG.png (256x320px)
> `A GAME UI PANEL BACKGROUND designed for 9-slice stretching. Size: 256x320 pixels. Style: HI-FI VECTOR (NOT pixel art), Sci-Fi Solarpunk.
> This panel will have text, prices, and item listings placed ON TOP of it in-game, so the CENTER AREA must be COMPLETELY EMPTY and CLEAN.
> - CENTER: flat solid Dark Terminal color (#1A1D2E), completely clean and empty
> - BORDER: a thin rounded-corner frame in Warm Amber (#FFB347) with subtle glow
> - CORNERS: small copper (#B87333) bracket ornaments at each corner
> - TOP EDGE: a thin amber (#FFB347) header bar area (20px tall) for title text — just a subtle color band, no text
> The panel should look like a futuristic shop terminal screen. NO content, NO icons, NO text inside the panel. Just the frame and the empty dark interior.`

---

## ⭐ Batch 4: HUD Icon (Hi-Fi — No Rotation)

### 10. Icon_TrustPoints.png (16x16px)
> `A single small game HUD icon on a transparent background. Size: 16x16 pixels. Style: HI-FI VECTOR, clean and simple. Must be legible at VERY SMALL SIZE.
> Subject: A "Trust Points" icon — a simple 5-pointed STAR shape, filled with a holographic blue-to-cyan gradient (#00F3FF to #66FFFF). Thin copper (#B87333) outline. One single clean shape, NO fine details (it is only 16x16 pixels).
> Background: TRANSPARENT (or SOLID WHITE #FFFFFF for easy removal).`

---

## 📋 Generation Checklist

| # | Asset | Rotations | Status |
|---|-------|-----------|--------|
| 1 | `Icon_AstroStew.png` | 1 | ✅ |
| 2 | `Icon_NeonSalad.png` | 1 | ✅ |
| 3 | `Icon_BioFuelCell.png` | 1 | ✅ |
| 4 | `Icon_NutrientPack.png` | 1 | ✅ |
| 5 | `Icon_SprinklerMk2.png` | 1 | ✅ |
| 6 | `Machine_ProcessingStation` | 4 (S/W/N/E) | ✅ |
| 7 | `Machine_TradingPost` | 4 (S/W/N/E) | ✅ |
| 8 | `UI_CraftingPanel_BG.png` | 1 | ✅ |
| 9 | `UI_TradingPanel_BG.png` | 1 | ✅ |
| 10 | `Icon_TrustPoints.png` | 1 | ✅ |
| | **Total Files** | **18** | |
