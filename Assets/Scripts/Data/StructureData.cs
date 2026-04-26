using UnityEngine;

namespace AstroPioneer.Data
{
    /// <summary>
    /// V25.1: Pure engine-level grid layer assignment.
    /// Determines ONLY which array inside a Chunk stores this structure's ID.
    /// No semantic meaning — UI/Game Design uses ItemCategory instead.
    /// </summary>
    public enum TargetGridLayer
    {
        FloorLayer,         // Chunk.FloorLayer: iron plates, dirt tiles
        UtilityLayer,       // Chunk.UtilityLayer: cables, pipes (underground infrastructure)
        StructureLayer      // Chunk.StructureLayer: machines, crops, fences, decoratives (above ground)
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

        [Header("Grid Layer (Engine Only)")]
        [Tooltip("V25.1: Determines which data array in the Chunk stores this structure. " +
                 "This is purely an engine concern — use ItemCategory for UI/semantics.")]
        public TargetGridLayer targetLayer = TargetGridLayer.StructureLayer;

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

        [Tooltip("Optional fine-tuning for the visual puppet position. Applied after dimensions centering.")]
        public Vector3 visualOffset = Vector3.zero;

        [Tooltip("If true, this structure blocks Macro placement (other machines/crops).")]
        public bool blocksMacroPlacement = true;

        [Tooltip("V23: If true, bots and entities cannot walk through this structure (e.g. fences, machines). " +
                 "Cables and pipes should have this OFF.")]
        public bool blocksMovement = false;

        [Tooltip("If true, this structure has complex runtime state " +
                 "(inventory, power level, orientation) stored in Chunk.ComplexStates.")]
        public bool hasComplexState = false;

        [Header("Capability Flags")]
        [Tooltip("V25.1: If true, this structure is a crop with growth stages driven by MetadataLayer.")]
        public bool isCrop = false;

        [Tooltip("V25.1: If true, this structure is an interactable machine with runtime behavior.")]
        public bool isMachine = false;

        [Header("Economy")]
        public int sellPrice = 0;
        public int buyPrice = 0;
    }
}
