import json

with open('all_issues_utf8.json', 'r', encoding='utf-8-sig') as f:
    issues = json.load(f)

md = '# Astro-Pioneer Issue Audit & Testing Plan (Developer Focus)\n\n'

# Define custom test points for critical tickets based on recent refactor
test_points = {
    # CLOSED (REGRESSION)
    56: ["Verify power grid connects across chunk boundaries", "Test generator output and battery storage persistence", "Ensure Solar Panels react to TimeManager day/night cycles"],
    35: ["Verify O2 depletes correctly during exploration", "Test O2 regeneration near Oxy-Flora or Base", "Check if Fatigue triggers correctly when Sleep is skipped"],
    34: ["Test mining/logging tools on world objects", "Verify resource drops and inventory addition", "Ensure mined objects do not respawn incorrectly upon chunk load"],
    24: ["Open/close storage UI without freezing", "Test drag-and-drop between inventory and storage", "Verify storage contents persist after Save/Load"],
    22: ["Verify bot uses A* pathfinding on the new Chunk grid", "Ensure bot stops or recalculates if player places structure in its path", "Test bot behavior across chunk borders"],
    20: ["Test placing Small Storage Bin on macro-grid", "Verify capacity limits"],
    19: ["Verify Water Pump extracts water if placed on water source", "Test pipe micro-grid connectivity"],
    18: ["Verify scroll wheel cycles hotbar slots", "Test equipping tools/seeds from hotbar"],
    14: ["Verify TimeManager triggers daily events (crop growth, respawns)", "Test fast-forwarding time (sleep)"],
    12: ["Verify ChunkRenderer updates crop sprite upon growth stage change", "Ensure correct sprites are assigned via StructureRegistry"],
    11: ["Test harvesting fully grown crops", "Verify grid tile resets to 'Empty' after harvest", "Test tool usage (Scythe/Hands)"],
    9: ["Test planting Neon Carrot seeds", "Verify 3-day growth cycle"],
    8: ["Test planting Space Potato seeds", "Verify 2-day growth cycle"],
    7: ["Verify CropManager communicates with GridManager", "Test object pooling (Spawn/Despawn) when moving between chunks", "Verify structure data is saved to binary files"],
    6: ["Verify sprinkler particles trigger at correct daily time", "Test grid hydration logic"],
    5: ["Test placing sprinkler", "Verify 3x3 hydration radius applies to crops"],
    2: ["Verify global 2D light syncs with TimeManager (dark at night, bright at day)"],

    # OPEN (IMPLEMENTATION / QA)
    86: ["Run end-to-end gameplay loop without softlocks", "Verify all core mechanics interact smoothly"],
    85: ["Spawn 1000+ crops and measure FPS drop", "Test chunk loading/unloading memory leaks"],
    84: ["Save game on Planet A, warp to Planet B, return to Planet A", "Verify all chunk data remains intact"],
    80: ["Verify tutorial popups trigger at correct sequence", "Test edge cases (player does sequence out of order)"],
    68: ["Verify credits addition/deduction", "Test Trust level progression via Shipping Bin"],
    67: ["Verify animals stay inside closed enclosures", "Test AI pathfinding logic inside fences"],
    66: ["Test overlapping micro-grid (pipes) and macro-grid (machines)", "Ensure no collision bugs"],
    65: ["Test O2 range extenders", "Verify power nodes distribute energy properly"],
    64: ["Test Composter converting bio-mass to fertilizer", "Verify input/output ratios"],
    63: ["Test placing fences, paths, and large structures", "Verify structure bounds checking"],
    62: ["Test fog of war clears upon player movement", "Verify chunk boundary edge cases"],
    61: ["Test unlocking new items with Research Data", "Verify blueprints appear in crafting menu"],
    60: ["Test endgame trigger conditions", "Verify pollution/ecological variables"],
    59: ["Test Enclosure detection logic (closed-loop fence check)", "Verify fauna breeding/happiness"],
    57: ["Test UV Light Pillar functionality in dark zones", "Ensure normal crops don't grow without UV light"],
    55: ["Test building standalone executable", "Verify no Unity Editor dependencies in scripts"],
    54: ["Test switching languages at runtime", "Verify all UI text updates dynamically"],
    53: ["Fix all existing null reference errors", "Review Unity crash logs"],
    52: ["Optimize ChunkRenderer Object Pool", "Reduce GC allocations in Update loops"],
    51: ["Test writing binary chunk data", "Test reading and deserializing chunk data", "Handle corrupted save file recovery"],
    49: ["Test procedural generation parameters for new planets", "Verify biome data loading"],
    48: ["Ensure game pauses (Time.timeScale = 0) when menu opens", "Test resume functionality"],
    47: ["Test volume sliders (Music/SFX)", "Save settings to PlayerPrefs"],
    46: ["Implement step-by-step UI guides", "Add highlighting to important UI elements"],
    42: ["Implement late-game recipes (Quantum Core, etc)", "Verify crafting time balancing"],
    41: ["Test multi-harvest crops (e.g., Flux Berry)", "Test trellis logic (Solar-Vine)"],
    40: ["Implement UI to toggle machines on/off", "Show power consumption stats"],
    39: ["Implement linked storage networks", "Test auto-sorting algorithms"],
    38: ["Test Harvester machine picking up mature crops automatically", "Verify deposit into adjacent storage"],
    37: ["Test Agri-Mech planting, watering, and harvesting", "Verify fuel/battery consumption"],
    36: ["Implement drone rescue when O2 hits 0", "Teleport player to bed, reset O2"],
    33: ["Implement resource node generation on new planets", "Test landing sequence"],
    32: ["Implement upgrading ship modules (O2 capacity, Engine)", "Deduct resources on upgrade"],
    31: ["Separate interior ship grid from exterior planet grid", "Ensure saves don't overlap"],
    30: ["Implement buy/sell logic in trading menu", "Verify dynamic pricing"],
    29: ["Implement database for Credits and Trust points", "Bind to UI top bar"],
    28: ["Implement Trading Post item acceptance logic", "Trigger daily shipment event"],
    27: ["Implement Smelter/Assembler conversion logic", "Test input queue and output slot"],
    26: ["Implement UI layout for Crafting", "Show required vs available materials"],
    25: ["Implement base crafting manager", "Handle inventory item consumption"],
    21: ["Implement Transport Bot inventory", "Set A->B logic for moving items between chests"],
    16: ["Implement inventory array logic", "Enforce stack limits (e.g., 99 per slot)"],
    15: ["Implement fatigue debuff (slower movement, fast O2 drain)", "Reset fatigue upon sleeping"],
    13: ["Calculate sun position based on game time", "Trigger OnDayStart and OnNightStart events"],
    10: ["Check inventory for seed/water", "Apply to target grid via MouseInteractionSystem"]
}

md += '## 🚨 High Priority Regression (Refactor Impact)\n'
md += 'Testing fitur lama (Closed) yang wajib divalidasi karena berpotensi rusak setelah arsitektur `ChunkManager` dan `ServiceLocator` diubah.\n\n'

for i in issues:
    if i['state'] == 'CLOSED' and ('Artist' not in i['title']) and ('Asset' not in i['title']) and ('Migrate' not in i['title']):
        md += f"### [ ] **#{i['number']}** - {i['title']}\n"
        points = test_points.get(i['number'], ["Test basic functionality", "Verify no NullReferenceException"])
        for p in points:
            md += f"  - [ ] {p}\n"
        md += "\n"

md += '---\n## 🏗️ Core Open Mechanics & QA (New Architecture Implementation)\n'
md += 'Tugas Developer & QA yang masih aktif. Harus diimplementasikan dan ditesting menggunakan standar arsitektur sistem Grid & Chunk yang baru.\n\n'

for i in issues:
    if i['state'] == 'OPEN' and ('Artist' not in i['title']) and ('UI/UX' not in i['title'] and 'VFX' not in i['title']):
        md += f"### [ ] **#{i['number']}** - {i['title']}\n"
        points = test_points.get(i['number'], ["Implement feature logic", "Integrate with ServiceLocator", "Test edge cases"])
        for p in points:
            md += f"  - [ ] {p}\n"
        md += "\n"

with open('full_testing_audit.md', 'w', encoding='utf-8-sig') as f:
    f.write(md)
