using UnityEngine;

namespace AstroPioneer.Data
{
    /// <summary>
    /// Defines a ship room type with size, cost, and bonuses.
    /// </summary>
    [CreateAssetMenu(fileName = "NewShipRoom", menuName = "AstroPioneer/Ship/Room")]
    public class ShipRoom : ScriptableObject
    {
        [Header("Identity")]
        public string roomID;
        public string displayName;
        [TextArea] public string description;
        public Sprite roomIcon;

        [Header("Size")]
        [Tooltip("Room size in grid cells")]
        public Vector2Int size = new Vector2Int(3, 3);

        [Header("Cost")]
        [Tooltip("Credits to unlock this room")]
        public int unlockCost = 500;
        [Tooltip("Ship upgrade tier required (0 = always available)")]
        public int requiredTier = 0;

        [Header("Bonuses")]
        [Tooltip("Crop growth speed multiplier (Greenhouse only)")]
        public float cropGrowthMultiplier = 1f;
        [Tooltip("Power generation bonus (Engine room only)")]
        public float powerBonus = 0f;
        [Tooltip("Extra storage slots (Storage room only)")]
        public int bonusStorageSlots = 0;
    }
}
