using UnityEngine;

namespace AstroPioneer.Data
{
    /// <summary>
    /// ToolBehaviour — Abstract ScriptableObject base for all tool actions.
    /// Uses the Strategy Pattern: each tool's behaviour is a separate asset,
    /// dragged into InventoryItem.toolAction in the Inspector.
    /// Adding a new tool requires ZERO code changes in PlayerToolState.
    /// </summary>
    public abstract class ToolBehaviour : ScriptableObject
    {
        [Header("Tool Config")]
        [Tooltip("Human-readable name for this action (e.g. 'Destroy', 'Water', 'Chop').")]
        public string actionName;

        [Tooltip("If true, one unit of the item is consumed on each successful use.")]
        public bool consumesItem = false;

        /// <summary>
        /// Execute the tool's action at the given grid position.
        /// </summary>
        /// <param name="gridPos">The grid cell the player clicked.</param>
        /// <param name="sourceItem">The InventoryItem being used.</param>
        /// <param name="hotbarSlotIndex">The hotbar slot index (for inventory removal).</param>
        /// <returns>True if the action was successful.</returns>
        public abstract bool Execute(Vector2Int gridPos, InventoryItem sourceItem, int hotbarSlotIndex);
    }
}
