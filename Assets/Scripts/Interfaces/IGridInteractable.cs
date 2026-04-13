using AstroPioneer.Data;

namespace AstroPioneer.Interfaces
{
    /// <summary>
    /// Interface for any world object that can be interacted with via the grid.
    /// Standardizes interaction away from OnMouseDown.
    /// </summary>
    public interface IGridInteractable
    {
        /// <summary>
        /// Triggered when the user clicks the grid cell where this object is located.
        /// </summary>
        /// <param name="heldItem">The item the player is currently holding in their active hotbar slot.</param>
        void Interact(InventoryItem heldItem);
    }
}
