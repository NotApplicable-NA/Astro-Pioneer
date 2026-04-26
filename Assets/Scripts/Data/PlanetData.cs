using UnityEngine;
using System.Collections.Generic;

namespace AstroPioneer.Data
{
    public enum BiomeType
    {
        Desert,
        Tundra,
        Volcanic,
        Lush
    }

    /// <summary>
    /// Defines a planet's properties: biome, resources, and exploration parameters.
    /// GDD NOTE: No hazards/damage zones. Challenges are logistics-based (Shadow Canyons, light puzzles).
    /// </summary>
    [CreateAssetMenu(fileName = "NewPlanet", menuName = "AstroPioneer/Exploration/Planet")]
    public class PlanetData : ScriptableObject
    {
        [Header("Identity")]
        public string planetID;
        public string displayName;
        [TextArea] public string description;
        public Sprite planetIcon;

        [Header("Biome")]
        public BiomeType biome;
        public string sceneName; // The Unity scene to load

        [Header("Oxygen")]
        [Tooltip("Oxygen drain rate per second (higher = harder)")]
        public float oxygenDrainRate = 1f;

        [Header("Resources")]
        [Tooltip("Resource nodes to spawn on this planet")]
        public List<ResourceSpawnEntry> resourceNodes = new List<ResourceSpawnEntry>();

        [Header("Difficulty")]
        [Range(1, 5)]
        public int difficultyLevel = 1;
        [Tooltip("Trust points required to unlock this planet (0 = always)")]
        public int trustRequired = 0;

        [Header("Shadow Canyons")]
        [Tooltip("Whether this planet has Shadow Canyon zones (no sunlight areas)")]
        public bool hasShadowCanyons = false;
    }

    [System.Serializable]
    public class ResourceSpawnEntry
    {
        public InventoryItem resource;
        [Tooltip("Min quantity dropped per node")]
        public int minDrop = 1;
        [Tooltip("Max quantity dropped per node")]
        public int maxDrop = 3;
        [Tooltip("Relative spawn weight (higher = more common)")]
        public float spawnWeight = 1f;
        [Tooltip("Hits required to harvest")]
        public int hitPoints = 3;
    }
}
