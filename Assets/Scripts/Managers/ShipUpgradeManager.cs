using UnityEngine;
using System;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Managers;
using AstroPioneer.Systems.Ship;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// Manages ship upgrade tiers, room purchases, and expansion.
    /// </summary>
    public class ShipUpgradeManager : MonoBehaviour
    {
        public static ShipUpgradeManager Instance { get; private set; }

        [Header("Upgrade Tiers")]
        [SerializeField] private int currentTier = 1;
        [SerializeField] private int maxTier = 3;

        [Header("Tier Costs (Credits to upgrade)")]
        [SerializeField] private int[] tierCosts = { 0, 1000, 3000 }; // Tier1=free, Tier2=1000, Tier3=3000

        [Header("Tier Grid Expansion (extra cells per tier)")]
        [SerializeField] private Vector2Int[] tierExpansions = {
            new Vector2Int(6, 6),   // Tier 1: 6x6 starter
            new Vector2Int(10, 10), // Tier 2: expands to 10x10
            new Vector2Int(14, 14)  // Tier 3: expands to 14x14
        };

        [Header("Available Rooms")]
        [SerializeField] private List<ShipRoom> availableRooms = new List<ShipRoom>();

        // Track which rooms have been purchased
        private HashSet<string> purchasedRoomIDs = new HashSet<string>();

        // Events
        public event Action<int> OnTierUpgraded;
        public event Action<ShipRoom> OnRoomPurchased;

        // Properties
        public int CurrentTier => currentTier;
        public int MaxTier => maxTier;
        public IReadOnlyList<ShipRoom> AvailableRooms => availableRooms;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── Tier Upgrades ───────────────────────────

        /// <summary>
        /// Get cost for the next tier upgrade.
        /// </summary>
        public int GetNextTierCost()
        {
            if (currentTier >= maxTier) return -1; // Already max
            return tierCosts[currentTier]; // Index = next tier - 1
        }

        /// <summary>
        /// Check if player can upgrade to next tier.
        /// </summary>
        public bool CanUpgradeTier()
        {
            if (currentTier >= maxTier) return false;
            int cost = GetNextTierCost();
            return CurrencyManager.Instance != null && CurrencyManager.Instance.CurrentCredits >= cost;
        }

        /// <summary>
        /// Upgrade ship to next tier. Spends credits and expands grid.
        /// </summary>
        public bool UpgradeTier()
        {
            if (!CanUpgradeTier())
            {
                return false;
            }

            int cost = GetNextTierCost();
            if (!CurrencyManager.Instance.TrySpendCredits(cost))
                return false;

            currentTier++;

            // Expand grid (handled by ShipGrid based on tier)
            if (ShipGrid.Instance != null)
            {
                ExpandGridToTier(currentTier);
            }

            OnTierUpgraded?.Invoke(currentTier);
            return true;
        }

        private void ExpandGridToTier(int tier)
        {
            if (tier <= 0 || tier > tierExpansions.Length) return;

            var expansion = tierExpansions[tier - 1];
            var grid = ShipGrid.Instance;
            var maxSize = grid.MaxGridSize;
            var center = new Vector2Int(maxSize.x / 2, maxSize.y / 2);
            var halfExp = new Vector2Int(expansion.x / 2, expansion.y / 2);

            // Unlock cells in the expanded area
            for (int x = center.x - halfExp.x; x < center.x + halfExp.x; x++)
            {
                for (int y = center.y - halfExp.y; y < center.y + halfExp.y; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (grid.IsWithinBounds(pos) && grid.GetCellState(pos) == ShipCellState.Locked)
                    {
                        // Directly unlock via grid cell
                        var cell = grid.GetCell(pos);
                        if (cell != null) cell.state = ShipCellState.Empty;
                    }
                }
            }
        }

        // ── Room Purchases ──────────────────────────

        /// <summary>
        /// Get rooms available for purchase at current tier.
        /// </summary>
        public List<ShipRoom> GetPurchasableRooms()
        {
            var result = new List<ShipRoom>();
            foreach (var room in availableRooms)
            {
                if (room == null) continue;
                if (purchasedRoomIDs.Contains(room.roomID)) continue;
                if (room.requiredTier > currentTier) continue;
                result.Add(room);
            }
            return result;
        }

        /// <summary>
        /// Check if a room has been purchased.
        /// </summary>
        public bool IsRoomPurchased(ShipRoom room)
        {
            return room != null && purchasedRoomIDs.Contains(room.roomID);
        }

        /// <summary>
        /// Purchase and place a room on the ship grid.
        /// </summary>
        public bool PurchaseRoom(ShipRoom room, Vector2Int origin)
        {
            if (room == null) return false;

            if (purchasedRoomIDs.Contains(room.roomID))
            {
                return false;
            }

            if (room.requiredTier > currentTier)
            {
                return false;
            }

            // Check credits
            if (CurrencyManager.Instance == null || !CurrencyManager.Instance.TrySpendCredits(room.unlockCost))
            {
                return false;
            }

            // Unlock room on grid
            if (ShipGrid.Instance == null || !ShipGrid.Instance.UnlockRoom(room, origin))
            {
                // Rollback credits
                CurrencyManager.Instance.AddCredits(room.unlockCost);
                return false;
            }

            purchasedRoomIDs.Add(room.roomID);
            OnRoomPurchased?.Invoke(room);
            return true;
        }
    }
}
