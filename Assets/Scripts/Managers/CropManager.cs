using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Data;
using AstroPioneer.Systems;
using AstroPioneer.Core;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// CropManager — Static Data-Oriented Manager for Crops.
    /// Modifies grid data via GridManager. It does NOT spawn GameObjects.
    /// Visuals are handled automatically by ChunkRenderer based on structure/metadata IDs.
    /// </summary>
    public class CropManager : MonoBehaviour
    {
        public static CropManager Instance { get; private set; }
        
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        void Start()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayChanged += HandleDayChanged;
                
            if (ServiceLocator.TryGet<ChunkManager>(out var cm))
                cm.OnChunkLoadedEvent += HandleChunkLoaded;
        }

        void OnDestroy()
        {
            if (Instance == this) 
            {
                Instance = null;
                ServiceLocator.Unregister<CropManager>(); 
            }
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnDayChanged -= HandleDayChanged;
                
            if (ServiceLocator.TryGet<ChunkManager>(out var cm))
                cm.OnChunkLoadedEvent -= HandleChunkLoaded;
        }

        private void HandleChunkLoaded(Chunk chunk)
        {
            if (TimeManager.Instance == null) return;
            
            int currentDay = TimeManager.Instance.DaysPassed;
            int daysMissed = currentDay - chunk.LastSimulatedDay;

            if (daysMissed > 0)
            {
                bool anyChange = false;
                for (int x = 0; x < GameConstants.CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < GameConstants.CHUNK_SIZE; y++)
                    {
                        ushort id = chunk.StructureLayer.Get(x, y);
                        if (id > 0 && StructureRegistry.Instance != null && StructureRegistry.Instance.IsCrop(id))
                        {
                            byte meta = chunk.MetadataLayer.Get(x, y);
                            bool isWatered = (meta & GameConstants.META_WATERED_FLAG) != 0;
                            
                            // For offline simulation, we only process 1 day of growth because 
                            // the watered status is consumed after 1 day.
                            if (isWatered)
                            {
                                byte stage = (byte)(meta & GameConstants.META_GROWTH_MASK);
                                byte daysAccumulated = (byte)((meta >> 4) & 0x07);
                                
                                var data = StructureRegistry.Instance.Get(id) as CropStructureData;
                                if (data != null && stage < 3)
                                {
                                    daysAccumulated++;
                                    if (daysAccumulated >= data.daysToGrowPerStage[stage])
                                    {
                                        stage++;
                                        daysAccumulated = 0;
                                    }
                                }
                                
                                byte newMeta = (byte)((stage & 0x0F) | ((daysAccumulated & 0x07) << 4));
                                chunk.MetadataLayer.Set(x, y, newMeta);
                                anyChange = true;
                            }
                        }
                    }
                }
                
                // Update chunk's last simulated day so it doesn't catch up again today
                chunk.LastSimulatedDay = currentDay;
                if (anyChange) chunk.IsDirty = true;
            }
        }

        private void HandleDayChanged(int currentDay)
        {
            if (!ServiceLocator.TryGet<ChunkManager>(out var cm)) return;

            // Iterate over all active chunks and update crops
            foreach (var kvp in cm.ActiveChunks)
            {
                var chunk = kvp.Value;
                for (int x = 0; x < GameConstants.CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < GameConstants.CHUNK_SIZE; y++)
                    {
                        ushort id = chunk.StructureLayer.Get(x, y);
                        // Use category-based check instead of hardcoded ID range
                        if (id > 0 && StructureRegistry.Instance != null && StructureRegistry.Instance.IsCrop(id))
                        {
                            byte meta = chunk.MetadataLayer.Get(x, y);
                            bool isWatered = (meta & GameConstants.META_WATERED_FLAG) != 0;
                            
                            // If watered, advance growth stage
                            if (isWatered)
                            {
                                byte stage = (byte)(meta & GameConstants.META_GROWTH_MASK);
                                byte daysAccumulated = (byte)((meta >> 4) & 0x07); // bits 4-6 store days in stage
                                
                                var data = StructureRegistry.Instance.Get(id) as CropStructureData;
                                if (data != null && stage < 3)
                                {
                                    daysAccumulated++;
                                    
                                    // Check if current stage growth is complete
                                    if (daysAccumulated >= data.daysToGrowPerStage[stage])
                                    {
                                        stage++;
                                        daysAccumulated = 0;
                                    }
                                }
                                
                                // Pack new metadata: [Stage 0-3] | [Days 4-6]
                                // Note: META_WATERED_FLAG (bit 7) is cleared automatically here
                                byte newMeta = (byte)((stage & 0x0F) | ((daysAccumulated & 0x07) << 4));
                                chunk.MetadataLayer.Set(x, y, newMeta);
                                chunk.IsDirty = true;
                                
                                // Trigger visual update
                                if (ServiceLocator.TryGet<ChunkRenderer>(out var renderer))
                                    renderer.UpdateVisualMetadata((int)chunk.Coord.WorldOriginX + x, (int)chunk.Coord.WorldOriginY + y);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Plants a crop by setting its structureID in the Grid.
        /// </summary>
        public bool PlantCrop(Vector2Int gridPos, CropStructureData cropData)
        {
            if (GridManager.Instance == null) return false;
            
            if (cropData == null)
            {
                Debug.LogWarning($"[CropManager] Failed to plant at {gridPos}: CropStructureData is null!");
                return false;
            }

            ushort structureID = StructureRegistry.Instance.GetID(cropData);

            if (structureID == GameConstants.STRUCTURE_EMPTY)
            {
                Debug.LogWarning($"[CropManager] Failed to plant {cropData.displayName}: StructureID is 0! Make sure it is added to StructureRegistry.");
                return false;
            }

            if (GridManager.Instance.TryPlaceStructure(gridPos, structureID))
            {
                // Stage 0, Unwatered
                GridManager.Instance.SetMetadataAt(gridPos, 0); 
                return true;
            }

            Debug.LogWarning($"[CropManager] Failed to plant {cropData.displayName} at {gridPos}: Grid position already occupied or chunk not loaded.");
            return false;
        }

        /// <summary>
        /// Reads crop structure ID at a position. Returns 0 if none.
        /// </summary>
        public ushort GetCropAt(Vector2Int gridPos)
        {
            if (GridManager.Instance == null) return 0;
            ushort id = GridManager.Instance.GetStructureAt(gridPos);
            if (id > 0 && StructureRegistry.Instance != null && StructureRegistry.Instance.IsCrop(id)) return id;
            return 0;
        }

        /// <summary>
        /// Removes a crop from the grid layer.
        /// </summary>
        public void RemoveCrop(Vector2Int gridPos)
        {
            if (GridManager.Instance != null)
            {
                ushort id = GetCropAt(gridPos);
                if (id != 0) GridManager.Instance.RemoveStructure(gridPos);
            }
        }
        
        public void WaterCropAt(Vector2Int gridPos)
        {
            if (GridManager.Instance == null) return;
            ushort id = GetCropAt(gridPos);
            if (id == 0) return; // Not a crop

            byte meta = GridManager.Instance.GetMetadataAt(gridPos);
            meta = (byte)(meta | GameConstants.META_WATERED_FLAG);
            GridManager.Instance.SetMetadataAt(gridPos, meta);
            
            // Visual update
            if (ServiceLocator.TryGet<ChunkRenderer>(out var renderer))
                renderer.UpdateVisualMetadata(gridPos.x, gridPos.y);
        }

        public bool TryHarvestCropAt(Vector2Int gridPos, out CropStructureData cropData)
        {
            cropData = null;
            if (GridManager.Instance == null) return false;
            
            ushort id = GetCropAt(gridPos);
            if (id == 0) return false;
            
            byte meta = GridManager.Instance.GetMetadataAt(gridPos);
            byte stage = (byte)(meta & GameConstants.META_GROWTH_MASK);
            
            if (stage >= 3)
            {
                // Harvest successful
                if (StructureRegistry.Instance != null)
                {
                    StructureData data = StructureRegistry.Instance.Get(id);
                    if (data is CropStructureData cropStr)
                    {
                        cropData = cropStr;
                    }
                }
                
                RemoveCrop(gridPos); // Plant is consumed on harvest
                return true;
            }
            return false;
        }
    }
}
