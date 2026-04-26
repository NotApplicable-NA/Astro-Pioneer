# 🤖 Bot Transport — Sprite Generation Prompts

> **Resolusi:** 64x64px per frame
> **Animasi:** 3 (Idle, Move, Carry)
> **Frame per animasi:** 4
> **Arah:** 8 (S, SE, E, NE, N, NW, W, SW) — SEMUA di-generate
> **Metode:** 3-fase (Reference Sheet → Animation Frames → VFX Terpisah)

---

## 🎨 Desain Bot (Referensi untuk SEMUA prompt)

> **DESAIN BOT:** A small, compact autonomous transport robot (like WALL-E). BOXY body — WHITE (#F0F4F8) with ORANGE (#FF8C42) accent panels on sides and top stripe. TWO large GREEN (#33FF57) glowing eyes on a dark front face panel. Dark grey (#4A4A4A) TANK TREADS on both sides. Small metal bumper/guard rail on front. Small antenna nub on top.
> **CARRY STYLE:** When carrying cargo, the crate is BALANCED ON TOP OF THE ROBOT'S HEAD — sitting directly on the flat roof. Cute and charming, like a character balancing a box on its head.
> The robot is roughly CUBE-shaped and COMPACT — cute, not vehicle-like. Sci-Fi Solarpunk style — modern, clean, cute.

---

## ⚙️ Workflow 2-Fase

### Fase 1: Generate Reference Sheet (PERTAMA)
1. Generate **1 gambar** berisi 8 arah statis
2. Review & approve desain
3. Crop masing-masing arah → simpan sebagai reference individual

### Fase 2: Generate Animation (per arah, PAKAI reference)
1. Upload **reference image** dari Fase 1 (1 arah)
2. Prompt AI untuk membuat 4 frame animasi dengan desain yang **EXACT SAME**
3. Ulangi untuk semua 8 arah × 3 animasi
4. **TIDAK ADA** dust/particle di sprite — bot harus bersih!

### Fase 3: Generate VFX Terpisah (PAKAI reference bot)
1. Upload **reference bot** + prompt VFX dust
2. Generate dust/particle sprite sheets yang cocok dengan skala bot
3. Implementasi di Unity sebagai ParticleSystem atau separate sprite layer

---

# 📌 FASE 1: Reference Sheet (Statis, 8 Arah)

## 1A. Reference Sheet — Tanpa Crate

> `A PIXEL ART CHARACTER TURNAROUND SHEET showing 8 static views of the SAME small robot, arranged in 2 rows of 4. Camera is OVERHEAD looking slightly down (top-down 3/4 view like Stardew Valley). This is NOT isometric.
> ROBOT DESIGN: A small, compact, CUBE-SHAPED autonomous transport robot (like WALL-E). WHITE (#F0F4F8) boxy body with ORANGE (#FF8C42) accent panels on sides and a stripe on top. TWO large GREEN (#33FF57) glowing eyes on a dark front face panel. Dark grey (#4A4A4A) TANK TREADS on both sides. Metal BUMPER/GUARD RAIL on front. Small antenna on top. The robot is compact and cute, NOT a vehicle/truck. 64x64 pixel art, detailed shading.
> IMPORTANT: The robot has a clearly different FRONT (dark face with green eyes + bumper) and BACK (plain white/grey panel, no eyes). Diagonal views MUST show the correct face.
> The 8 views in reading order (left to right, top to bottom):
> ROW 1 — front-facing views then turning clockwise:
> 1st — SOUTH (facing toward viewer): You see the FRONT FACE — dark panel with BOTH green eyes looking at you. Bumper at bottom. Both treads visible.
> 2nd — SOUTH-EAST (body rotated 45° RIGHT): You STILL see the FRONT face with eyes — but turned to the right. The DARK FACE PANEL is angled toward bottom-right. Right eye is larger/closer. You see the RIGHT side of the body + right tread. Orange panel on the LEFT side is visible.
> 3rd — EAST (facing right, 90°): Side profile. Robot faces RIGHT. ONE green eye visible on the right edge. Full right tread visible. Bumper points right.
> 4th — NORTH-EAST (body rotated 135°, mostly AWAY): You see the BACK of the robot — plain white/grey panel, NO EYES VISIBLE. The back is angled toward top-right. You see the RIGHT side of the body. This is DIFFERENT from SE — there are NO EYES because the face points away.
> ROW 2 — continuing clockwise:
> 5th — NORTH (facing fully AWAY, 180°): You see ONLY the BACK — plain panel. NO EYES at all. Both treads visible. Antenna on top.
> 6th — NORTH-WEST (body rotated 225°, mostly AWAY): You see the BACK — plain panel, NO EYES. Back angled toward top-left. LEFT side of body visible. This is the MIRROR of NE.
> 7th — WEST (facing left, 270°): Side profile facing LEFT. ONE green eye visible on the left edge. Full left tread. Bumper points left. This is the MIRROR of EAST.
> 8th — SOUTH-WEST (body rotated 315°): THIS IS THE MIRROR/FLIP OF VIEW #2 (SE). The robot faces toward BOTTOM-LEFT. You see the FRONT face with GREEN EYES — but the face is turned to the LEFT (not right). The LEFT EYE is larger because it is closer to the viewer. The LEFT TREAD is the dominant/visible tread. The orange accent panel on the RIGHT side of the body is visible. The bumper points toward the LEFT. Everything is the LEFT-side version of view #2.
> KEY: Views 1,2,8 show the FRONT FACE with eyes. Views 4,5,6 show the BACK with NO eyes. Views 3,7 show side profiles.
> All 8 = SAME robot, SAME size. Each view 64x64 pixels. Background: SOLID WHITE.`

## 1B. Reference Sheet — Crate Only (Overlay Sprite)

> `[UPLOAD Fase 1A image as rotation reference] A PIXEL ART SPRITE SHEET showing EXACTLY 8 views of a BROWN WOODEN CRATE — arranged in 2 rows of 4, matching the SAME rotation angles as the robot in the reference image. This crate will be overlaid ON TOP of the robot sprite.
> CRATE DESIGN: A brown wooden crate (#8B6914) with darker (#5C4033) horizontal plank lines and visible wood grain. Simple box shape. The crate is about the same width as each robot in the reference.
> The crate must follow the EXACT same rotation/facing direction as each corresponding robot view: S, SE, E, NE (row 1), N, NW, W, SW (row 2). Use the reference image to match each angle precisely.
> Each crate view: 64x64 pixel art. Background: SOLID WHITE. Crate ONLY — no robot, no ground, no shadows.`

---

# 📌 FASE 2: Animation Frames (Per Arah, Pakai Reference)

> **INSTRUKSI:** Untuk setiap prompt di bawah, **UPLOAD gambar reference** (hasil crop dari Fase 1) sebagai input image.

---

## 🔵 ANIMASI 1: IDLE

### 1A. Idle Bobbing (4 frames)

> Gerakan naik-turun halus. Setiap frame HARUS berbeda posisi vertikal.

**Template prompt:**
> `[UPLOAD reference image] Generate a PIXEL ART SPRITE SHEET showing 4 POSES of this robot in a horizontal row (4 images side by side in ONE image). Use the same robot design, colors, and viewing angle as the reference.
> Smooth bobbing up and down. Pose 1: MIDDLE position (same as reference). Pose 2: robot body moved UP by 2-3 pixels ABOVE reference position. Pose 3: back to MIDDLE position. Pose 4: robot body moved DOWN by 2-3 pixels BELOW reference position — the body sinks lower than where it normally sits. Pose 4 must be the LOWEST point, clearly lower than poses 1 and 3. The treads stay at the same level, only the body shifts.
> 64x64 pixel art per pose. Background: SOLID WHITE. Output: ONE SINGLE static image, NOT a video.`

### 1B. Idle Blink (2 frames, terpisah)

> Eyes menjadi garis tipis horizontal. Generate terpisah dari bobbing.

**Template prompt (untuk arah dengan MATA — S, SE, E, W, SW):**
> `[UPLOAD reference image] Generate a PIXEL ART SPRITE SHEET showing 4 POSES of this robot in a horizontal row (4 images side by side in ONE image). Use the same robot design, colors, and viewing angle as the reference.
> Pose 1: robot in UP position (shifted up 2-3px), eyes OPEN (normal green circles).
> Pose 2: robot in UP position (shifted up 2-3px), eyes BLINKING (eyes become thin horizontal lines).
> Pose 3: robot in DOWN position (shifted down 2-3px), eyes OPEN (normal green circles).
> Pose 4: robot in DOWN position (shifted down 2-3px), eyes BLINKING (eyes become thin horizontal lines).
> 64x64 pixel art per pose. Background: SOLID WHITE. Output: ONE SINGLE static image, NOT a video.`
> **CROP:** Ambil frame 2 dan 4 saja untuk blink sprite.

**Template prompt (untuk arah TANPA MATA — NE, N, NW):**
> `[UPLOAD reference image] Generate a PIXEL ART SPRITE SHEET showing 4 POSES of this robot in a horizontal row (4 images side by side in ONE image). Use the same robot design, colors, and viewing angle as the reference.
> Pose 1: robot in UP position (shifted up 2-3px), antenna STRAIGHT (normal).
> Pose 2: robot in UP position (shifted up 2-3px), antenna TILTED LEFT.
> Pose 3: robot in DOWN position (shifted down 2-3px), antenna STRAIGHT (normal).
> Pose 4: robot in DOWN position (shifted down 2-3px), antenna TILTED RIGHT.
> 64x64 pixel art per pose. Background: SOLID WHITE. Output: ONE SINGLE static image, NOT a video.`
> **CROP:** Ambil frame 2 dan 4 saja untuk twitch sprite.

### Per-arah notes:
| Direction | Upload Reference | Blink note |
|-----------|-----------------|------------|
| **Idle_S** | Crop #1 dari sheet | Both eyes blink |
| **Idle_SE** | Crop #2 dari sheet | Both eyes blink (right bigger) |
| **Idle_E** | Crop #3 dari sheet | One eye blinks |
| **Idle_NE** | Crop #4 dari sheet | No eyes — blink: antenna twitches instead |
| **Idle_N** | Crop #5 dari sheet | No eyes — blink: antenna twitches instead |
| **Idle_NW** | Crop #6 dari sheet | No eyes — blink: antenna twitches instead |
| **Idle_W** | Crop #7 dari sheet | One eye blinks |
| **Idle_SW** | Crop #8 dari sheet | Both eyes blink (left bigger) |

---

## 🟢 ANIMASI 2: MOVE (Driving)

> Tread bergerak (groove shifts), body sedikit bouncing.

**1. Template prompt (untuk Front / Back - S, N):**
> `[UPLOAD reference image] Generate a PIXEL ART SPRITE SHEET showing 4 POSES of this robot in a horizontal row (4 static images side by side in ONE single image file).
> The robot is DRIVING FORWARD. Use the same robot design, colors, and 2D viewing angle as the reference.
> CRITICAL: Strictly 2D pixel art style. DO NOT make the robot look 3D, volumetric, or like a 3D model. DO NOT rotate the robot's angle.
> Pose 1: MIDDLE position. The horizontal groove lines on the tank treads are at their starting position.
> Pose 2: Body shifted UP 2px. The tread groove lines shift DOWN by 1-2 pixels.
> Pose 3: MIDDLE position. The tread groove lines shift DOWN further.
> Pose 4: Body shifted DOWN 2px. The tread groove lines shift DOWN again.
> RULES:
> 1. The TREADS NEVER CHANGE HEIGHT — only the body bobs up and down.
> 2. The 4 frames must NOT be completely identical (the body must bob, and the tread lines must scroll).
> 3. Each pose is a distinct 64x64 pixel art frame. NO dust or particles.
> Background: SOLID WHITE. Output: ONE SINGLE static PNG/JPG.`

**2. Template prompt (untuk Diagonal - SE, NE, SW, NW):**
> `[UPLOAD reference image] Generate a PIXEL ART SPRITE SHEET showing 4 POSES of this robot in a horizontal row (4 static images side by side in ONE single image file).
> The robot is DRIVING FORWARD. Use the same robot design, colors, and 2D viewing angle as the reference.
> CRITICAL: Strictly 2D pixel art style. DO NOT make the robot look 3D or rotate its angle. All 4 poses must face the exact same diagonal angle.
> Pose 1: MIDDLE position. The DIAGONAL groove lines on the visible front edge of the tank treads are at their starting position.
> Pose 2: Body shifted UP 2px. The DIAGONAL groove lines shift DOWN-AND-BACK slightly along the perspective angle.
> Pose 3: MIDDLE position. The diagonal tread groove lines shift further along the angle.
> Pose 4: Body shifted DOWN 2px. The diagonal groove lines shift again along the angle.
> RULES:
> 1. In a diagonal view, the treads face an angle. The dark lines on the tread must scroll DIAGONALLY along the surface of the track to show rolling. The inner wheels/gears should also rotate slightly if visible.
> 2. The TREADS NEVER CHANGE HEIGHT — only the body bobs up and down.
> 3. The 4 frames must NOT be identical. 
> Background: SOLID WHITE. Output: ONE SINGLE static PNG/JPG.`

**3. Template prompt KHUSUS SIDE VIEW (untuk arah E dan W):**
> `[UPLOAD reference image] Generate a PIXEL ART SPRITE SHEET showing 4 POSES of this robot in a horizontal row (4 static images side by side in ONE single image file).
> The robot is DRIVING FORWARD. Use the same robot design, colors, and 2D viewing angle as the reference. Important: You are looking at the SIDE PROFILE of the robot and its tank treads.
> CRITICAL: Strictly 2D pixel art style. DO NOT make the robot look 3D or rotate its angle. All 4 poses must face the side.
> Pose 1: MIDDLE position. The tank tread GEARS/WHEELS (inside the treads) are in their starting rotation. The VERTICAL tread lines on the outer edge are at their start.
> Pose 2: Body shifted UP 2px. The gear details inside the treads ROTATE slightly. The VERTICAL tread lines shift backwards (towards the rear of the robot) by 1-2 pixels.
> Pose 3: MIDDLE position. The gear details rotate further. The vertical tread lines shift further backward (rolling).
> Pose 4: Body shifted DOWN 2px. The gear details rotate again. The vertical tread lines shift backwards again.
> RULES:
> 1. For a SIDE view, we must see the treads rolling! Animate the VERTICAL grooves along the outside of the dark track moving backwards like a conveyor belt, AND the inner wheels/gears spinning.
> 2. The TREADS NEVER CHANGE HEIGHT from the ground — only the white body bobs up and down.
> 3. The 4 frames must NOT be identical. 
> Background: SOLID WHITE. Output: ONE SINGLE static PNG/JPG.`

### Per-arah notes:
| Direction | Upload Reference |
|-----------|------------------|
| **Move_S** | Crop #1 |
| **Move_SE** | Crop #2 |
| **Move_E** | Crop #3 |
| **Move_NE** | Crop #4 |
| **Move_N** | Crop #5 |
| **Move_NW** | Crop #6 |
| **Move_W** | Crop #7 |
| **Move_SW** | Crop #8 |

---

## 🟠 ANIMASI 3: CARRY (Driving + Crate Overlay)

> **PENDEKATAN BARU:** Carry TIDAK perlu di-generate terpisah.
> Carry animation = **Move animation** + **Crate sprite overlay** dari Fase 1B.
> Di Unity, saat bot dalam mode carry, aktifkan child sprite object crate di atas bot.
> Crate sprite ikut bobbing movement dari parent bot secara otomatis.
>
> **Jadi TIDAK perlu generate 8×4 frame terpisah untuk Carry!**

### Implementasi di Unity:
| Step | Detail |
|------|--------|
| 1 | Buat child GameObject `CrateOverlay` pada bot |
| 2 | Assign crate sprite (dari Fase 1B crop) sesuai arah |
| 3 | Position offset: di atas kepala robot |
| 4 | Toggle `SetActive(true/false)` saat carry/idle |


---

# 📌 FASE 3: VFX Terpisah (Pakai Reference Bot)

> Generate setelah Fase 2 selesai. VFX akan di-overlay di Unity sebagai ParticleSystem atau sprite terpisah.

## 3A. Tread Dust VFX

> `[UPLOAD reference image of bot from south view] Generate a PIXEL ART SPRITE SHEET of a small DUST CLOUD animation — 4 frames in a horizontal row. The dust should match the scale of the robot in the reference image, appearing at the bottom near where the tank treads meet the ground. Small grey-brown (#A89880) puff of dust that expands and fades.
> Frame 1: tiny dust burst appears. Frame 2: dust expands outward. Frame 3: dust spreads and becomes transparent. Frame 4: dust almost fully dissipated.
> 32x32 pixel art per frame. Background: SOLID WHITE. Dust particles only.`

## 3B. Exhaust Puff VFX

> `[UPLOAD reference image of bot from north view] Generate a PIXEL ART SPRITE SHEET of a small EXHAUST PUFF animation — 4 frames in a horizontal row. A tiny grey (#B0B0B0) smoke puff coming from the exhaust vent on the back of the robot. Subtle and small.
> Frame 1: tiny puff appears. Frame 2: puff rises and expands. Frame 3: puff fades. Frame 4: nearly invisible.
> 16x16 pixel art per frame. Background: SOLID WHITE. Smoke only.`

---

## 📋 Generation Checklist

| # | Asset | Type | Status |
|---|-------|------|--------|
| | **FASE 1: REFERENCE** | | |
| 0a | Reference Sheet (tanpa crate) | 8 static views | ✅ |
| 0b | Crate Overlay Sheet (crate saja) | 8 static views | ✅ |
| | **FASE 2: IDLE** | | |
| 1 | `Bot_Idle_S` | 4 bob + 2 blink | ✅ |
| 2 | `Bot_Idle_SE` | 4 bob + 2 blink | ✅ |
| 3 | `Bot_Idle_E` | 4 bob + 2 blink | ✅ |
| 4 | `Bot_Idle_NE` | 4 bob + 2 twitch | ✅ |
| 5 | `Bot_Idle_N` | 4 bob + 2 twitch | ✅ |
| 6 | `Bot_Idle_NW` | 4 bob + 2 twitch | ✅ |
| 7 | `Bot_Idle_W` | 4 bob + 2 blink | ✅ |
| 8 | `Bot_Idle_SW` | 4 bob + 2 blink | ✅ |
| | **FASE 2: MOVE** | | |
| 9 | `Bot_Move_S` | 4 frames | ⬜ |
| 10 | `Bot_Move_SE` | 4 frames | ⬜ |
| 11 | `Bot_Move_E` | 4 frames | ⬜ |
| 12 | `Bot_Move_NE` | 4 frames | ⬜ |
| 13 | `Bot_Move_N` | 4 frames | ⬜ |
| 14 | `Bot_Move_NW` | 4 frames | ⬜ |
| 15 | `Bot_Move_W` | 4 frames | ⬜ |
| 16 | `Bot_Move_SW` | 4 frames | ⬜ |
| | **CARRY** | *Overlay: Move + Crate sprite* | ✅ No generation needed |
| | **FASE 3: VFX** | | |
| 17 | `VFX_Tread_Dust` | 4 frames | ⬜ |
| 18 | `VFX_Exhaust_Puff` | 4 frames | ⬜ |
| | **TOTAL** | **2 refs + 16 anim sheets + 2 VFX = 72 frames** | |

---

## 📁 Output Folder Structure
```
Assets/Art/Sprites/Bots/
├── Reference/
│   ├── Bot_Ref_Sheet.png           (8 static views)
│   ├── Crate_Overlay_Sheet.png     (8 crate-only views)
│   ├── Bot_Ref_S.png               (cropped)
│   ├── Bot_Ref_SE.png              (cropped)
│   └── ...                         (dst)
├── CrateOverlay/
│   ├── Crate_S.png                 (static overlay)
│   ├── Crate_SE.png
│   └── ...                         (8 directions)
├── Idle/
│   ├── Bot_Idle_S.png      (4 frames)
│   ├── Bot_Idle_SE.png     (4 frames)
│   └── ...                 (8 directions)
├── Move/
│   ├── Bot_Move_S.png      (4 frames)
│   ├── Bot_Move_SE.png     (4 frames)
│   └── ...                 (8 directions)
└── VFX/
    ├── VFX_Tread_Dust.png  (4 frames)
    └── VFX_Exhaust_Puff.png (4 frames)
```
