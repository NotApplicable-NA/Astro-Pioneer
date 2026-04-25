import subprocess

splits = [
    (10, "[Developer] - Mechanics: Planting & Watering Logic", "[Artist] - VFX: Planting & Watering Feedback", "Priority: P0 | Sprint: 3 | Type: VFX | Focus: Visual feedback for crop hydration and seeds."),
    (26, "[Developer] - UI: Crafting Interface Logic", "[Artist] - UI/UX: Crafting Hologram Menu Design", "Design holographic UI for crafting queue, matching 16-Bit Solarpunk guidelines."),
    (28, "[Developer] - System: Trading Post Logic", "[Artist] - Assets: Trading Terminal Machine", "Design the Comm-Terminal / Logistics Capsule machine sprite."),
    (30, "[Developer] - UI: Trading Interface", "[Artist] - UI/UX: Trading Panels Design", "Design holographic UI for Buy/Sell panels and price fluctuations."),
    (32, "[Developer] - System: Ship Upgrade Logic", "[Artist] - Assets: Ship Expansion Visuals", "Design visual upgrades and structural expansions for interior grids."),
    (36, "[Developer] - Mechanic: Rescue Protocol Script", "[Artist] - VFX: Rescue Drone Animation", "Animate drone fetching sleeping player at 0% O2."),
    (37, "[Developer] - Automation: Agri-Mech Logic", "[Artist] - Machinery Assets: Agri-Mech Sprites", "Draw Agri-Mech top-down sprite and multi-directional animations."),
    (38, "[Developer] - Automation: Harvester Logic", "[Artist] - Machinery Assets: Harvester Sprites", "Draw Harvester auto-collector machines and idle/running states."),
    (39, "[Developer] - Core System: Advanced Storage", "[Artist] - Assets: Advanced Storage Units", "Design upgraded large-capacity silos/crates for late game."),
    (40, "[Developer] - UI: Automation Control Panel", "[Artist] - UI/UX: Control Panel Design", "Design diegetic monitor for factory automation management."),
    (41, "[Developer] - Farming: Late Game Crops Logic", "[Artist] - Flora Assets: Late Game Crops", "Draw seeds, growth stages, and items for late-game exotic flora."),
    (46, "[Developer] - System: Tutorial Implementation", "[QA] - Functional Test: Tutorial Flow", "Verify the progression, prompts, and fail-states of the new player UI tutorial."),
    (47, "[Developer] - UI: Settings Logic", "[Artist] - UI/UX: Settings Menu Design", "Design in-theme Settings UI board (Volume, Controls, vsync)."),
    (48, "[Developer] - UI: Pause Menu Implementation", "[Artist] - UI/UX: Pause Menu Design", "Mockup pause screen avoiding generic templates. Strict Solarpunk palette."),
    (49, "[Developer] - System: Additional Planet Scaling", "[Artist] - Environment: Extra Planet Biomes", "Tilesets and palette swaps for the 2 extra extreme environments (heat/cold)."),
    (51, "[Developer] - Core System: Save/Load State", "[QA] - Functional Test: Multi-Planet State Saving", "Stress test loading cross-planet instances with dense bases. Prevent item dupes."),
    (52, "[Developer] - Core: Performance Optimization", "[QA] - System Test: Framerate & Stress", "Measure FPS drops when rendering 500+ crops or 100+ conveyor belts active."),
    (53, "[Developer] - Core: Release Bug Fixing", "[QA] - Regression Test: Full Release Walkthrough", "Final sweep test against all accepted tickets.")
]

rename_only = {
    23: "[Artist] - VFX: Transport Bot Animations",
    25: "[Developer] - Core System: Crafting System Core",
    27: "[Developer] - Automation: Resource Processing",
    29: "[Developer] - Core System: Currency & Trust Tracking",
    31: "[Developer] - Core System: Ship Interior Grid System",
    33: "[Developer] - System: Planet Exploration Framework",
    42: "[Developer] - Data: Advanced Crafting Recipes",
    43: "[Artist] - Audio: SFX System",
    44: "[Artist] - Audio: Music System",
    45: "[Artist] - VFX: Universal Polish Pass",
    50: "[Artist] - Environment: Visual Storytelling Assets",
    54: "[Developer] - Data: Localization Framework",
    55: "[Developer] - Ops: Build & Deployment"
}

def call_gh(*args):
    subprocess.run(["gh"] + list(args), check=True)

for issue_id, dev_title, new_title, new_body in splits:
    print(f"Splitting {issue_id}")
    call_gh("issue", "edit", str(issue_id), "--title", dev_title)
    call_gh("issue", "create", "--title", new_title, "--body", new_body + f"\n\nSplit from issue #{issue_id}")

for issue_id, new_title in rename_only.items():
    print(f"Renaming {issue_id}")
    call_gh("issue", "edit", str(issue_id), "--title", new_title)

print("Split and renaming completed successfully.")
