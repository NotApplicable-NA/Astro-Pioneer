using UnityEngine;
using AstroPioneer.Managers;
using AstroPioneer.Core;

namespace AstroPioneer.Data
{
    /// <summary>
    /// HoeAction — Destroys structures, utilities, and free-moving entities at the target grid cell.
    /// Migrated from PlayerToolState.TryHoeDestroy() to follow the Strategy Pattern.
    /// </summary>
    [CreateAssetMenu(fileName = "ToolAction_Hoe", menuName = "AstroPioneer/Tools/Hoe Action")]
    public class HoeAction : ToolBehaviour
    {
        public override bool Execute(Vector2Int gridPos, InventoryItem sourceItem, int hotbarSlotIndex)
        {
            if (GridManager.Instance == null) return false;

            ushort structureID = GridManager.Instance.GetStructureAt(gridPos);
            ushort utilityID = GridManager.Instance.GetUtilityAt(gridPos);

            // 1. Try remove structure (crops, machines, fences)
            if (structureID != 0)
            {
                GridManager.Instance.RemoveStructure(gridPos);
                // V25: Fence now in StructureLayer — reevaluate enclosures on structure removal
                if (AstroPioneer.Systems.Husbandry.EnclosureSystem.Instance != null)
                    AstroPioneer.Systems.Husbandry.EnclosureSystem.Instance.ReevaluateEnclosuresAround(gridPos);
                return true;
            }

            // 2. Try remove utility (cables, pipes, fences)
            if (utilityID != 0)
            {
                GridManager.Instance.RemoveUtility(gridPos);
                if (AstroPioneer.Systems.Husbandry.EnclosureSystem.Instance != null)
                    AstroPioneer.Systems.Husbandry.EnclosureSystem.Instance.ReevaluateEnclosuresAround(gridPos);
                return true;
            }

            // 3. Try remove free-moving entities (AgriMech, TransportBot) via Data Layer
            if (ServiceLocator.TryGet<EntityManager>(out var em))
            {
                var entities = em.GetAllEntities();
                // Loop backwards in case of removal
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    var entity = entities[i];
                    // Player ID is typically 2 in our current data-driven setup
                    if (entity == null || entity.EntityTypeID == 2) continue;

                    Vector2Int entityGridPos = EntityManager.GetGridBelowEntity(entity.WorldPosition);
                    if (entityGridPos == gridPos)
                    {
                        if (entity is MonoBehaviour mb && mb != null)
                        {
                            Object.Destroy(mb.gameObject);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
