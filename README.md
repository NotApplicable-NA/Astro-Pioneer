# Astro-Pioneer

**Space Farming Simulation Game** - Unity 2022 LTS

Game simulasi manajemen resource (Farming x Automation) untuk PC. Bayangkan *Stardew Valley* bertemu *Factorio* di luar angkasa.

---

## 📋 Project Information

- **Engine:** Unity 2022.3.62f1 (LTS)
- **Platform:** Windows (x64)
- **Visual:** Pixel Art Top-Down 2D (PPU: 32)
- **Input:** Mouse & Keyboard (WASD + Raycast Interaction)

---

## 🚀 Quick Start

### Prerequisites

- Unity 2022.3.62f1 (LTS) atau versi yang compatible
- Visual Studio 2022 atau Rider (untuk C# scripting)

### Setup Scene

#### Method 1: Auto Setup (Recommended)

1. Buka Unity Editor
2. Buka scene yang ingin di-setup (atau buat scene baru)
3. Di menu bar, pilih: **`Astro-Pioneer > Setup Scene > Create All Core Systems`**
4. Sistem akan otomatis membuat:
   - `GridManager` GameObject
   - `MouseInteractionSystem` GameObject
   - `Player` GameObject dengan `PlayerStatus` component

#### Method 2: Manual Setup

1. **Create GridManager:**
   - Buat GameObject baru: `GameObject > Create Empty`
   - Rename menjadi `GridManager`
   - Add Component: `GridManager` (dari `Assets/Scripts/Managers/GridManager.cs`)
   - Atur settings di Inspector (default sudah OK untuk testing)

2. **Create MouseInteractionSystem:**
   - Buat GameObject baru: `GameObject > Create Empty`
   - Rename menjadi `MouseInteractionSystem`
   - Add Component: `MouseInteractionSystem` (dari `Assets/Scripts/Systems/MouseInteractionSystem.cs`)
   - Pastikan ada Camera dengan tag `MainCamera` di scene

3. **Create Player:**
   - Buat GameObject baru: `GameObject > Create Empty`
   - Rename menjadi `Player`
   - Add Component: `PlayerStatus` (dari `Assets/Scripts/Player/PlayerStatus.cs`)
   - Buat child GameObject: `GameObject > Create Empty` → Rename menjadi `ShipSpawnPoint`
   - Drag `ShipSpawnPoint` ke field **Ship Spawn Point** di `PlayerStatus` Inspector

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Data/              # ScriptableObject untuk data game
│   │   ├── CropData.cs
│   │   └── CropDataCreator.cs
│   ├── Managers/          # Singleton managers
│   │   └── GridManager.cs
│   ├── Systems/           # Game systems
│   │   └── MouseInteractionSystem.cs
│   ├── Player/            # Player-related scripts
│   │   └── PlayerStatus.cs
│   └── Editor/            # Editor helper scripts
│       └── SceneSetupHelper.cs
├── Data/                  # ScriptableObject assets
│   └── Crops/            # Crop data assets
└── Prefabs/              # Prefabs untuk scene
    ├── Managers/
    ├── Systems/
    └── Player/
```

---

## 🎮 Core Systems

### 1. Grid System

**GridManager** - Singleton untuk manage grid 2D

- **Playable Area:** 10x10 cells (Starter Hull)
- **Cell Size:** 1 Unity unit = 32 pixels (PPU 32)
- **Features:**
  - World-to-Grid position conversion
  - Grid-to-World position conversion
  - Playable area validation
  - Cell occupation tracking
  - Debug gizmo visualization

**Usage:**
```csharp
Vector2Int gridPos = GridManager.Instance.GetGridPosition(worldPosition);
bool isAvailable = GridManager.Instance.IsPositionAvailable(gridPos);
```

### 2. Mouse Interaction System

**MouseInteractionSystem** - Raycast system untuk mouse-to-grid interaction

- **Features:**
  - Auto-detect Main Camera
  - Support orthographic & perspective camera
  - Grid cell click detection
  - Grid cell hover detection
  - Playable area validation

**Events:**
```csharp
MouseInteractionSystem.OnGridCellClicked += HandleGridClick;
MouseInteractionSystem.OnGridCellHovered += HandleGridHover;
```

### 3. Player Status System

**PlayerStatus** - Manage player stats (Oksigen, Stamina) & Rescue Protocol

- **Features:**
  - Oksigen & Stamina auto-depletion
  - Rescue Protocol (No Game Over):
    - Teleport ke Ship Spawn Point
    - Skip Day
    - Drop 15% items dari inventory
    - Auto-reset status

**Events:**
```csharp
PlayerStatus.OnOxygenChanged += UpdateOxygenUI;
PlayerStatus.OnRescueProtocolTriggered += HandleRescue;
```

### 4. Crop Data System

**CropData** - ScriptableObject untuk data tanaman

- **Current Crops:**
  - **Space Potato (CRP_001):** Growth 120s, Cost 10, Sell 18
  - **Neon Carrot (CRP_002):** Growth 300s, Cost 25, Sell 60

**Create Crop Data:**
- Menu: `Astro-Pioneer > Create Crop Data > [Crop Name]`

---

## 🛠️ Editor Tools

### Scene Setup Helper

Menu: `Astro-Pioneer > Setup Scene`

- **Create All Core Systems** - Auto-setup semua sistem di scene
- **Create GridManager** - Setup GridManager saja
- **Create MouseInteractionSystem** - Setup MouseInteractionSystem saja
- **Create Player** - Setup Player dengan PlayerStatus
- **Create Prefabs Folder Structure** - Buat folder structure untuk prefabs
- **Create Prefab from Selected** - Convert selected GameObject ke Prefab

### Crop Data Creator

Menu: `Astro-Pioneer > Create Crop Data`

- **Space Potato (CRP_001)** - Create Space Potato data
- **Neon Carrot (CRP_002)** - Create Neon Carrot data

---

## 📦 Creating Prefabs

### Method 1: Via Editor Tool

1. Setup GameObject di scene menggunakan **Scene Setup Helper**
2. Select GameObject yang ingin dijadikan Prefab
3. Menu: `Astro-Pioneer > Setup Scene > Create Prefab from Selected`
4. Prefab akan otomatis disimpan di folder yang sesuai

### Method 2: Manual

1. Setup GameObject di scene
2. Drag GameObject dari Hierarchy ke folder `Assets/Prefabs/[Category]/`
3. Prefab akan terbuat otomatis

**Recommended Prefabs:**
- `Assets/Prefabs/Managers/GridManager.prefab`
- `Assets/Prefabs/Systems/MouseInteractionSystem.prefab`
- `Assets/Prefabs/Player/Player.prefab`

---

## 🧪 Testing

### Integration Testing

Fokus ke Integration/Play Mode Tests sesuai arahan PM:

1. **Grid System Test:**
   - Play scene
   - Cek GridManager gizmo di Scene view (hijau = playable area)
   - Test mouse click pada grid (cek Console untuk log)

2. **Mouse Interaction Test:**
   - Play scene
   - Click pada grid cells
   - Verify Console log menunjukkan grid position yang benar

3. **Player Status Test:**
   - Play scene
   - Tunggu oksigen habis (atau set `maxOxygen` ke nilai kecil untuk test cepat)
   - Verify Rescue Protocol triggered:
     - Player teleport ke Ship Spawn Point
     - Day skipped (cek Console log)
     - Items dropped (placeholder - akan di-integrate dengan Inventory)

4. **Crop Data Test:**
   - Create CropData asset via menu
   - Verify data sesuai GDD (Potato: 120s, Carrot: 300s)
   - Test validasi di Inspector (ubah growthTime ke 0, harus warning)

---

## 📝 Development Notes

### Code Standards

- **Namespace:** `AstroPioneer.[Category]` (Data, Managers, Systems, Player)
- **Error Handling:** Gunakan `Debug.LogError` untuk null checks
- **Documentation:** XML comments untuk public methods
- **SOLID Principles:** Separation of Concerns, Single Responsibility

### GDD Compliance

- ✅ No Game Over (Rescue Protocol)
- ✅ Grid 10x10 playable area (Starter Hull)
- ✅ Crop data sesuai Appendix B (Potato 120s, Carrot 300s)
- ✅ Mouse-to-Grid Raycast implementation

### Known TODOs

- [ ] Integrate Inventory System dengan Rescue Protocol (drop 15% items)
- [ ] Integrate Day/Night System dengan Skip Day
- [ ] Create actual Prefabs (saat ini masih setup di scene)
- [ ] Add UI integration (Oxygen/Stamina bars)

---

## 🤝 Contributing

### Branch Strategy

- `main` - Production-ready code
- `develop` - Development branch
- `feature/[feature-name]` - Feature branches

### Commit Messages

Format: `[Category] Description`

Examples:
- `[Grid] Fix grid position calculation`
- `[Player] Implement Rescue Protocol`
- `[Data] Add CropData validation`

---

## 📚 References

- **GDD v2.1** - Single Source of Truth untuk game logic & balancing
- **Unity 2022 LTS Documentation** - https://docs.unity3d.com/2022.3/Documentation/Manual/

---

## 📧 Contact

Untuk pertanyaan teknis, hubungi Engineering Team.

---

**Status:** Sprint 1 - Foundation ✅ COMPLETED


