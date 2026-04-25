using UnityEngine;

namespace AstroPioneer.Data
{
    /// <summary>
    /// Categories for structures placed on the grid.
    /// Determines which GridLayer inside a Chunk they belong to.
    /// </summary>
    public enum StructureCategory
    {
        Floor,          // FloorLayer: iron plates, dirt tiles
        Utility,        // UtilityLayer: cables, pipes, fences
        Crop,           // StructureLayer: plantable crops
        Machine,        // StructureLayer: sprinklers, harvesters, pumps
        Decorative      // StructureLayer: trees, rocks, decor
    }

    /// <summary>
    /// StructureData — Data-driven base definition for all grid entities.
    /// The ID is automatically derived from its index in the StructureRegistry.
    /// </summary>
    [CreateAssetMenu(fileName = "NewStructure", menuName = "AstroPioneer/Data/Structure Data")]
    public class StructureData : ScriptableObject
    {
        [Header("Identity")]
        public string displayName;

        [Header("Classification")]
        public StructureCategory category;

        [Header("Visuals")]
        [Tooltip("Sprites for each state/growth stage. Index 0 = default.")]
        public Sprite[] sprites;

        [Tooltip("Prefab spawned by ChunkRenderer from ObjectPool. " +
                 "For simple structures (crops), can be a basic SpriteRenderer. " +
                 "For complex machines, the full prefab with components.")]
        public GameObject visualPrefab;

        [Header("Grid Rules")]
        [Tooltip("Grid dimensions (e.g. 2x2 for AgriMech, 1x1 for crops).")]
        public Vector2Int dimensions = Vector2Int.one;

        [Tooltip("If true, this structure blocks Macro placement (other machines/crops).")]
        public bool blocksMacroPlacement = true;

        [Tooltip("V23: If true, bots and entities cannot walk through this structure (e.g. fences, machines). " +
                 "Cables and pipes should have this OFF.")]
        public bool blocksMovement = false;

        [Tooltip("If true, this structure has complex runtime state " +
                 "(inventory, power level, orientation) stored in Chunk.ComplexStates.")]
        public bool hasComplexState = false;

        [Header("Economy")]
        public int sellPrice = 0;
        public int buyPrice = 0;
    }
}
