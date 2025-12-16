# CONTEXT: Project Manager - Astro-Pioneer

**Document Purpose:** Transfer PM persona context ke model berikutnya  
**Last Updated:** 2025-12-14 20:39  
**Sprint:** Sprint 2 (Wrapping Up) → Sprint 3 (Ready)

---

## 🎭 PERAN SAYA

**Role:** Project Manager (PM)  
**Persona:** Relaxed, Technical, Direct-to-Action, Empathetic yet Disciplined  
**Bahasa:** Indonesia (komunikasi), English (code/technical specs)  
**Batasan:** 
- HANYA sebagai PM, tidak mengutak-atik code langsung
- TIDAK berwenang mengubah GDD tanpa Game Designer approval
- Memberikan arahan, koordinasi, dan manajemen scope

---

## 🚀 PROJECT OVERVIEW

**Game:** Astro-Pioneer  
**Genre:** Premium PC Space Farming Sim (Steam)  
**Engine:** Unity 2022 LTS  
**Render Pipeline:** Universal Render Pipeline (URP) 2D Renderer (MANDATORY)  
**Visual Style:** High-Fidelity Pixel Art "Solarpunk" (Sage Green #9DC183, Cream #F5F5DC, Teal #008080)  
**UI Style:** Hybrid HD (GDD 2.2 updated - Dave the Diver reference)  
**Core Loop:** Farm → Harvest → Trade → Upgrade Ship → Explore Planets  
**PPU:** Strictly 32 Pixels Per Unit, Filter Mode: Point  
**Fail State:** NO GAME OVER - "Rescue Protocol" (Teleport + Skip Day + Drop 15% Items)

**Repository:** https://github.com/NotApplicable-NA/Astro-Pioneer  
**Local Path:** `c:\Nabil\Projects\Farming Sim 3D\Astro-Pioneer`

---

## 👥 TIM VIRTUAL

| Role | Tugas | Notes |
|------|-------|-------|
| **Game Designer** | GDD, feature design | PM perlu memo untuk pivot |
| **Game Artist** | Art direction, prompts | TIDAK generate image langsung |
| **Game Artist Generator** | Generate image dari prompt Artist | Executor only |
| **Game Developer** | Code implementation | Menggunakan Antigravity editor |
| **QA** | Testing | - |
| **Market Analyst** | Market research, strategic input | Data sudah divalidasi stakeholder |

**PENTING:** 
- Game Artist HANYA buat prompt, TIDAK generate image (akan error)
- Game Artist Generator yang execute image generation

---

## ✅ SPRINT 1 PHASE 2 - COMPLETE

### Issues Status (All Closed):

| Issue | Title | Status | Assigned |
|-------|-------|--------|----------|
| #1 | Migrate to URP 2D | ✅ CLOSED (verified - no work needed) | Developer |
| #2 | Setup 2D Global Lighting | ✅ CLOSED (Cream #F5F5DC) | Developer |
| #3 | Golden Spike Asset | ✅ CLOSED (Normal Map validated) | Artist → Generator → Developer |
| #4 | Hybrid HD UI | 🔄 RE-OPENED for Sprint 2 (Option A: Full Overhaul) | Artist → Developer |

### Key Achievements:
- URP 2D Renderer sudah configured dari awal project
- 2D Global Lighting dengan Cream color applied
- Machine_Harvester sprite + Normal Map working (validated dengan Spot Light 2D)
- Visual pipeline complete: URP + Lighting + Normal Map

### Assets Created:
- `Assets/Art/Sprites/Machines/Machine_Harvester.png` (402 KB)
- `Assets/Art/Sprites/Machines/Machine_Harvester_Normal.png` (384 KB)
- `Assets/Scripts/Editor/URPVerificationHelper.cs` (verification tool)

---

## ✅ RESOLVED DECISIONS

### Automation Feature Timeline - APPROVED

**GDD 2.2 Update:** Automation dipindah ke Early-Game
- Tier 1 (Early): Basic Sprinkler - auto-water 4 tiles
- Tier 2 (Mid): Transport Bot
- Tier 3 (Late): Agri-Androids

**Status:** ✅ APPROVED by Game Designer, reflected in GDD 2.2

---

## 📊 MARKET ANALYST MEMO SUMMARY

4 Points yang diterima:

1. **Trust Visual Impact** ✅ ACCEPTED
   - Trust harus punya visual impact (koloni transformation)
   - Tidak perlu pivot, masuk backlog

2. **Automation Earlier** ⚠️ PENDING GAME DESIGNER
   - Perlu approval untuk ubah timeline

3. **Visual Contrast (Cold Tech vs Warm Nature)** ✅ ACCEPTED
   - Sudah aligned dengan current art direction

4. **Rescue Protocol** ✅ VALIDATED
   - GDD sudah benar, no change needed

---

## 📋 SPRINT 2 - IN PROGRESS

### Issues Status:

| Issue | Title | Art | Dev | Status |
|-------|-------|-----|-----|--------|
| #4 | Hybrid HD UI | ✅ | ⏳ | 🔄 OPEN (Dev pending) |
| #5 | Basic Sprinkler Machine | ✅ | ✅ | ✅ CLOSED |
| #6 | Sprinkler VFX Animation | ✅ | ⏳ | 🔄 OPEN (Dev pending) |

### Assets Delivered Sprint 2:
- `Assets/Art/Sprites/Machines/Machine_Sprinkler.png`
- `Assets/Art/Sprites/Machines/Machine_Sprinkler_Normal.png`
- `Assets/Scripts/Machines/Sprinkler.cs`
- `Assets/Art/Sprites/UI/Panels/Panel_Main.png`
- `Assets/Art/Sprites/UI/Panels/Panel_Secondary.png`
- `Assets/Art/Sprites/UI/Buttons/Button_Primary.png`
- `Assets/Art/Sprites/UI/Buttons/Button_Secondary.png`
- `Assets/Art/Sprites/VFX/VFX_Sprinkler_Water.png` (1.2 MB, 8-frame sprite sheet)

### Key Decisions Made:
- **UI Scope:** Option A (Full overhaul - panels, buttons, HUD)
- **Font:** Inter (modern, clean)
- **Art Style:** Consistent Solarpunk theme
- **Machine_Harvester:** Reclassified as Tier 3 (Late Game)

### Pending Sprint 2 Tasks:
1. Developer → TICKET-004 integration (9-slice configuration)
2. Developer → TICKET-006 integration (slice VFX sprite sheet, create animation)

### ✅ FULL PRODUCT BACKLOG CREATED (Dec 14, 2025)

**Total Issues:** 55 (includes #1-6)
**New Issues Created:** 49 (#7-#55)
**Timeline:** 20-25 weeks (5-6 months) to launch

#### Sprint Breakdown:
- **Sprint 3:** Core Farming Loop (6 tickets) - #7-#12
- **Sprint 4:** Time & Inventory (6 tickets) - #13-#18
- **Sprint 5:** Tier 2 Automation (6 tickets) - #19-#24
- **Sprint 6:** Economy & Trading (6 tickets) - #25-#30
- **Sprint 7:** Exploration (6 tickets) - #31-#36
- **Sprint 8:** Tier 3 + Late Game (6 tickets) - #37-#42
- **Sprint 9:** Polish & Content (8 tickets) - #43-#50
- **Sprint 10:** Launch Prep (5 tickets) - #51-#55

**All issues posted to:** https://github.com/NotApplicable-NA/Astro-Pioneer/issues

**Note:** Issues created without labels (labels don't exist in repo yet). Priority denoted in title ([P0], [P1], [P2]).

### Next Sprint Planning:
When Sprint 2 complete, begin Sprint 3 planning:
1. Review TICKET-007 (Crop System Foundation)
2. Assign to Developer
3. Create art tasks for TICKET-008 & TICKET-009 (crops)

---

## 🔧 TECHNICAL CONSTRAINTS

### Unity Project:
- PPU: 32 (STRICT)
- Filter Mode: Point (no filter)
- Compression: None untuk sprites
- Normal Map: Link via Sprite Editor → Secondary Textures (TIDAK bisa via code)

### Browser Automation:
- GitHub comments: Gunakan Ctrl+Enter untuk submit
- Element indices berubah-ubah (dynamic DOM)
- Kadang perlu manual copy-paste jika browser automation gagal

### AI Model Constraints:
- Developer persona TIDAK bisa akses Unity Editor, hanya edit files
- User harus handle Unity Editor tasks (linking Normal Map, visual validation)

---

## 📁 IMPORTANT FILE LOCATIONS

**GitHub Issues Templates:**
- `.github/ISSUES/TICKET-001-URP-Migration.md`
- `.github/ISSUES/TICKET-002-2D-Lighting.md`
- `.github/ISSUES/TICKET-003-Golden-Spike.md`
- `.github/ISSUES/TICKET-004-Hybrid-UI.md`

**Art Assets:**
- `Assets/Art/Sprites/UI/` - Diegetic Hologram UI (existing)
- `Assets/Art/Sprites/Machines/` - Machine sprites
- `Assets/Art/Sprites/Crops/SpacePotato/` - Crop sprites
- `Assets/Art/Sprites/Environment/` - Environment tiles

**Scripts:**
- `Assets/Scripts/Editor/URPVerificationHelper.cs` - URP validation
- `Assets/Scripts/Editor/SceneSetupHelper.cs` - Scene setup

---

## 📝 WORKFLOW YANG SUDAH ESTABLISHED

### Issue Creation Flow:
1. PM create issue di GitHub
2. PM add assignment comment
3. Forward prompt ke tim yang assigned
4. Tim execute dan report back
5. PM verify deliverables
6. PM approve dan close issue

### Art Asset Flow:
1. PM assign ke Game Artist
2. Game Artist buat prompt + art direction
3. Game Artist forward ke Game Artist Generator
4. Generator create assets
5. Developer integrate ke Unity
6. User validate di Unity Editor (jika perlu)
7. PM approve

### GDD Change Flow:
1. Input dari Market Analyst atau tim lain
2. PM evaluate apakah perlu pivot
3. Jika ya → PM buat memo ke Game Designer
4. Game Designer decide dan update GDD
5. PM proceed dengan updated direction

---

## 🎯 NEXT ACTIONS (When Resuming)

1. **Tunggu Game Designer response** untuk Automation timeline
2. **Start Sprint 2 Planning** setelah decision clear
3. **Create Sprint 2 issues** di GitHub
4. **Assign ke tim** sesuai priority

---

## 💡 LESSONS LEARNED

1. **Check existing config first** - URP sudah configured, saving migration work
2. **Artist doesn't generate** - Only creates prompts, Generator executes
3. **Unity Editor tasks need User** - AI cannot control Unity directly
4. **Close issues only when truly done** - Integration phase counts as part of ticket
5. **PM tidak ubah GDD** - Harus memo ke Game Designer untuk pivot

---
