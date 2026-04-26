using UnityEngine;

namespace AstroPioneer.Systems
{
    /// <summary>
    /// Identifies a placed machine instance for Save/Load and pickup logic.
    /// Must remain in its own file so Unity can resolve it on prefabs.
    /// </summary>
    public class MachineIDTag : MonoBehaviour
    {
        public string itemID;
        public Vector2Int originGridPos;
        public Vector2Int dimensions;
        public string uniqueInstanceID;

        /// <summary>
        /// Generates a GUID if one hasn't been assigned yet.
        /// Called on first placement and on load for legacy save data.
        /// </summary>
        public void EnsureUniqueID()
        {
            if (string.IsNullOrEmpty(uniqueInstanceID))
                uniqueInstanceID = System.Guid.NewGuid().ToString();
        }

        public bool Covers(Vector2Int worldPos)
        {
            return worldPos.x >= originGridPos.x && worldPos.x < originGridPos.x + dimensions.x &&
                   worldPos.y >= originGridPos.y && worldPos.y < originGridPos.y + dimensions.y;
        }
    }
}
