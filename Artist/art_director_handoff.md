# 🎬 Art Director Handoff — Bot Transport Sprites

> **Date:** 2026-02-26
> **Status:** Fase 1 ✅ Complete, Fase 2 Idle ✅ Complete, Fase 2 Move 🔲 In Progress
> **Prompt File:** `Artist/bot_sprite_prompts.md`
> **Rules File:** `Artist/ai_art_generation_rules.md`

---

## 📌 Project Overview

**Game:** Astro-Pioneer (Farming Sim 3D, top-down 3/4 view like Stardew Valley)
**Art Style:** Pixel Art, 64x64px per frame, Sci-Fi Solarpunk
**Asset:** Bot Transport — autonomous robot that carries crates

---

## 🤖 Bot Design Summary

- **Shape:** Compact, cube-shaped (like WALL-E), cute and small
- **Colors:** White (#F0F4F8) body, Orange (#FF8C42) accent panels, Green (#33FF57) glowing eyes
- **Features:** Dark front face panel with 2 green eyes, tank treads (dark grey #4A4A4A), metal bumper, small antenna
- **Carry Style:** Crate balanced on top of robot's head (overlay sprite, terpisah)

---

## ✅ COMPLETED ASSETS

### Fase 1: Reference Sheets
| Asset | File | Status |
|-------|------|--------|
| Bot Reference (8 views) | `Assets/Art/Sprites/Bots/Ref_Bot_E_Sheet.png` | ✅ |
| Crate Overlay (8 views) | `Assets/Art/Sprites/Bots/Ref_Bot_E_Crate_Sheet.png` | ✅ |

### Fase 2: Idle Animations (All 8 Directions)
| Asset | File | Frames |
|-------|------|--------|
| Idle S | `Bot_Anim_Sheet_Idle/Bot_Transport_Idle_S.png` | 4 bob + 2 blink |
| Idle SE | `Bot_Anim_Sheet_Idle/Bot_Transport_Idle_SE.png` | 4 bob + 2 blink |
| Idle E | `Bot_Anim_Sheet_Idle/Bot_Transport_Idle_E.png` | 4 bob + 2 blink |
| Idle NE | `Bot_Anim_Sheet_Idle/Bot_Transport_Idle_NE.png` | 4 bob + 2 twitch |
| Idle N | `Bot_Anim_Sheet_Idle/Bot_Transport_Idle_N.png` | 4 bob + 2 twitch |
| Idle NW | `Bot_Anim_Sheet_Idle/Bot_Transport_Idle_NW.png` | 4 bob + 2 twitch |
| Idle W | `Bot_Anim_Sheet_Idle/Bot_Transport_Idle_W.png` | 4 bob + 2 blink |
| Idle SW | `Bot_Anim_Sheet_Idle/Bot_Transport_Idle_SW.png` | 4 bob + 2 blink |

**Idle Animation Structure:**
- **Bobbing (4 frames):** MIDDLE → UP → MIDDLE → DOWN (body shifts 2-3px, treads stay)
- **Blink (2 frames):** Eyes become thin horizontal lines at UP and DOWN positions
- **Antenna Twitch (2 frames, NE/N/NW only):** Antenna tilts left/right at UP and DOWN positions
- **In Unity:** Blink/twitch triggered randomly during bobbing loop

---

## 🔲 REMAINING WORK

### Fase 2: Move Animations (8 Directions)
- **Prompt:** See `bot_sprite_prompts.md` > ANIMASI 2: MOVE
- **Animation:** 4 poses — body bouncing + tread grooves shifting (rolling)
- **PENTING:** Prompt MOVE perlu di-fix sebelum generate — kata "ANIMATION" harus diganti "SPRITE SHEET" / "POSES" (lihat ai_art_generation_rules.md)
- **Status:** Prompt sudah di-update tapi belum di-test generate

### Carry Animation
- **TIDAK PERLU DI-GENERATE** — pakai Move animation + Crate overlay sprite dari Fase 1B
- **Implementasi di Unity:** 
  1. Buat child GameObject `CrateOverlay` pada bot
  2. Assign crate sprite sesuai arah hadap
  3. Toggle `SetActive(true/false)` saat carry/idle mode

### Fase 3: VFX (2 Sheets)
- Tread Dust (32x32, 4 poses)
- Exhaust Puff (16x16, 4 poses)

---

## 📐 8-Direction View Order

Reference sheet layout: 2 rows × 4 columns

```
ROW 1:  S (front)    SE (front-right)    E (right side)    NE (back-right)
ROW 2:  N (back)     NW (back-left)      W (left side)     SW (front-left)
```

**Eye visibility:**
- ✅ Eyes visible: S, SE, E, W, SW (use BLINK variant)
- ❌ No eyes: NE, N, NW (use ANTENNA TWITCH variant)
- SW = mirror flip of SE

---

## 🎯 KEY DESIGN DECISIONS

### 1. Crate as Overlay (not combined)
- Awalnya coba generate bot+crate dalam satu gambar → GAGAL (crate terlalu kecil, overlap, layout berantakan)
- **Keputusan final:** Generate crate terpisah sebagai overlay sprite
- Crate di-render sebagai child sprite object di Unity

### 2. Separate Blink/Twitch Frames
- Awalnya blink di-include di frame ke-4 bobbing → bobbing jadi cuma 3 frame efektif
- **Keputusan final:** Blink terpisah (2 frame) — generate 4 frame alternating, crop #2 dan #4
- Di Unity: trigger blink secara random selama bobbing loop

### 3. Idle Bobbing Cycle
- **MIDDLE → UP → MIDDLE → DOWN** (bukan neutral → up → neutral → blink)
- DOWN harus EXPLISIT "BELOW reference position" — tanpa ini AI hanya generate neutral sebagai terendah

### 4. Move = Bobbing + Tread Rolling
- Body bouncing sama seperti idle
- Tambahan: tread groove lines bergeser setiap frame (simulasi rolling)

---

## 📁 Asset Location

```
Assets/Art/Sprites/Bots/
├── Ref_Bot_E_Sheet.png            ← Reference (8 static views)
├── Ref_Bot_E_Sheet.aseprite       ← Aseprite source
├── Ref_Bot_E_Crate_Sheet.png      ← Crate overlay reference
├── Ref_Bot_E_Sheet_Carry.aseprite ← Carry source
├── Bot_Anim_Sheet_Idle/           ← All 8 idle sheets ✅
│   ├── Bot_Transport_Idle_S.png
│   ├── Bot_Transport_Idle_SE.png
│   ├── Bot_Transport_Idle_E.png
│   ├── Bot_Transport_Idle_NE.png
│   ├── Bot_Transport_Idle_N.png
│   ├── Bot_Transport_Idle_NW.png
│   ├── Bot_Transport_Idle_W.png
│   └── Bot_Transport_Idle_SW.png
├── Bot_Transport_Idle.png         ← Legacy/combined
├── Bot_Transport_Move.png         ← Legacy/combined
├── Bot_Transport_Carry.png        ← Legacy/combined
└── (various .anim, .controller, .aseprite files)
```

---

## 🔗 Related Files

| File | Purpose |
|------|---------|
| `Artist/bot_sprite_prompts.md` | All prompts for AI generation per fase |
| `Artist/ai_art_generation_rules.md` | Rules & pitfalls learned from experience |
| `Artist/sprint6_prompts.md` | Sprint 6 overall art prompts |
| `Artist/sprint6_art_handoff.md` | Sprint 6 art handoff document |
