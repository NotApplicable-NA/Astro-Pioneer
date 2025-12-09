using UnityEngine;

namespace AstroPioneer.Data
{
    /// <summary>
    /// ScriptableObject untuk menyimpan data tanaman (Crop).
    /// Mengacu pada Appendix B: Data Balancing dari GDD v2.1
    /// </summary>
    [CreateAssetMenu(fileName = "NewCropData", menuName = "Astro-Pioneer/Crop Data", order = 1)]
    public class CropData : ScriptableObject
    {
        [Header("Crop Identification")]
        [Tooltip("ID unik untuk crop (contoh: CRP_001, CRP_002)")]
        public string cropID;
        
        [Tooltip("Nama display untuk crop")]
        public string cropName;
        
        [Header("Growth Settings")]
        [Tooltip("Waktu pertumbuhan dalam detik")]
        public float growthTimeSeconds;
        
        [Header("Economy")]
        [Tooltip("Harga beli bibit (dalam Credits)")]
        public int seedCost;
        
        [Tooltip("Harga jual hasil panen (dalam Credits)")]
        public int sellPrice;
        
        [Header("Visual")]
        [Tooltip("Sprite untuk seed stage")]
        public Sprite seedSprite;
        
        [Tooltip("Sprite untuk sprout stage")]
        public Sprite sproutSprite;
        
        [Tooltip("Sprite untuk harvest stage")]
        public Sprite harvestSprite;
        
        /// <summary>
        /// Validasi data crop saat di-assign di Inspector
        /// </summary>
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(cropID))
            {
                Debug.LogWarning($"[CropData] {cropName}: CropID tidak boleh kosong!", this);
            }
            
            if (growthTimeSeconds <= 0)
            {
                Debug.LogError($"[CropData] {cropName}: Growth Time harus lebih dari 0!", this);
            }
            
            if (seedCost < 0)
            {
                Debug.LogWarning($"[CropData] {cropName}: Seed Cost tidak boleh negatif!", this);
            }
            
            if (sellPrice < 0)
            {
                Debug.LogWarning($"[CropData] {cropName}: Sell Price tidak boleh negatif!", this);
            }
        }
    }
}


