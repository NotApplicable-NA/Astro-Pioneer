# Astro-Pioneer QA Testing Plan

## 🚨 Functional Regression (User Experience)
Validasi fitur lama untuk memastikan pengalaman pemain tetap konsisten setelah update arsitektur.

### [ ] **#56** - [P1] TICKET-056: Power Management System
  - [✅] Verify power grid connects across chunk boundaries (Visual feedback)
  - [✅] Test generator output and battery storage persistence
  - [???] Ensure Solar Panels react to TimeManager day/night cycles (Feature pending)

### [ ] **#35** - [P1] TICKET-035: Oxygen/HP System
  - [✅] Verify O2 depletes correctly during exploration
  - [??] Test O2 regeneration near Oxy-Flora or Base (Feature pending)
  - [✅] Check if Fatigue triggers correctly when Sleep is skipped

### [ ] **#34** - [P1] TICKET-034: Resource Gathering
  - [??] Test mining/logging tools on world objects (Feature pending)
  - [??] Verify resource drops and inventory addition (Feature pending)
  - [??] Ensure mined objects do not respawn incorrectly (Feature pending)

### [✅] **#24** - [P2] TICKET-024: Storage Management UI
  - [✅] Open/close storage UI without freezing
  - [??] Test drag-and-drop between inventory and storage (Feature pending)
  - [✅] Verify storage contents persist after Save/Load

### [✅] **#20** - [P1] TICKET-020: Small Storage Bin
  - [✅] Test placing Small Storage Bin on macro-grid
  - [✅] Verify capacity limits

### [ ] **#19** - [P1] TICKET-019: Water Pump Machine
  - [??] Verify Water Pump extracts water if placed on water source (Feature pending)
  - [??] Test pipe micro-grid connectivity (Feature pending)

### [ ] **#18** - [P1] TICKET-018: Hotbar System
  - [??] Verify scroll wheel cycles hotbar slots (Feature pending)
  - [✅] Test equipping tools/seeds from hotbar

### [✅] **#14** - [P1] TICKET-014: Time-Based Events
  - [✅] Verify TimeManager triggers daily events (crop growth, respawns)
  - [✅] Test fast-forwarding time (sleep)

### [✅] **#12** - [P2] TICKET-012: Crop Visual Feedback
  - [✅] Verify ChunkRenderer updates crop sprite upon growth stage change
  - [✅] Ensure correct sprites are assigned via StructureRegistry

### [✅] **#11** - [P0] TICKET-011: Harvesting System
  - [✅] Test harvesting fully grown crops
  - [✅] Verify grid tile resets to 'Empty' after harvest
  - [✅] Test tool usage (Scythe/Hands)

### [ ] **#6** - [P2] TICKET-006: Sprinkler VFX Animation
  - [??] Verify sprinkler particles trigger at correct daily time (Feature pending)
  - [??] Test grid hydration logic (Feature pending)

### [ ] **#5** - [P1] TICKET-005: Basic Sprinkler Machine
  - [??] Test placing sprinkler (Feature pending)
  - [??] Verify 3x3 hydration radius applies to crops (Feature pending)

---

## 🏗️ Active QA Tasks (New Architecture Implementation)
Testing fungsionalitas dan stabilitas sistem baru dari sudut pandang end-user.

### [ ] **#86** - [QA] - Regression Test: Full Release Walkthrough
  - [ ] Run end-to-end gameplay loop without softlocks
  - [ ] Verify all core mechanics interact smoothly

### [ ] **#85** - [QA] - System Test: Framerate & Stress
  - [ ] Spawn 1000+ crops and measure FPS drop
  - [ ] Test chunk loading/unloading memory leaks

### [ ] **#84** - [QA] - Functional Test: Multi-Planet State Saving
  - [ ] Save game on Planet A, warp to Planet B, return to Planet A
  - [ ] Verify all chunk data remains intact

### [ ] **#80** - [QA] - Functional Test: Tutorial Flow
  - [ ] Verify tutorial popups trigger at correct sequence
  - [ ] Test edge cases (player does sequence out of order)

### [ ] **#68** - [QA] - Functional Test: Economy & Meta Progression
  - [ ] Verify credits addition/deduction
  - [ ] Test Trust level progression via Shipping Bin

### [ ] **#67** - [QA] - Functional Test: Animal Husbandry AI & Enclosures
  - [ ] Verify animals stay inside closed enclosures
  - [ ] Test AI pathfinding logic inside fences

### [ ] **#66** - [QA] - Functional Test: Dual Grid Building & Automation Flow
  - [ ] Test overlapping micro-grid (pipes) and macro-grid (machines)
  - [ ] Ensure no collision bugs

### [ ] **#65** - [QA] - Functional Test: O2 & Energy Logistics Engine
  - [ ] Test O2 range extenders
  - [ ] Verify power nodes distribute energy properly
