using UnityEngine;
using System;

namespace AstroPioneer.Player
{
    /// <summary>
    /// System untuk manage player status (Oksigen, Stamina, dll).
    /// Implementasi Rescue Protocol sesuai GDD v2.1: Tidak Game Over, teleport ke ship, skip day, drop items
    /// </summary>
    public class PlayerStatus : MonoBehaviour
    {
        [Header("Oxygen Settings")]
        [Tooltip("Maksimal oksigen (dalam detik atau unit)")]
        [SerializeField] private float maxOxygen = 100f;
        
        [Tooltip("Rate pengurangan oksigen per detik")]
        [SerializeField] private float oxygenDepletionRate = 1f;
        
        [Tooltip("Apakah oksigen berkurang otomatis?")]
        [SerializeField] private bool autoDepleteOxygen = true;
        
        [Header("Stamina Settings")]
        [Tooltip("Maksimal stamina")]
        [SerializeField] private float maxStamina = 100f;
        
        [Tooltip("Rate pengurangan stamina per detik")]
        [SerializeField] private float staminaDepletionRate = 0.5f;
        
        [Header("Rescue Protocol")]
        [Tooltip("Spawn point di Ship (untuk teleport saat rescue)")]
        [SerializeField] private Transform shipSpawnPoint;
        
        [Tooltip("Delay sebelum teleport (dalam detik)")]
        [SerializeField] private float rescueTeleportDelay = 1f;
        
        [Header("Debug")]
        [Tooltip("Tampilkan status di console")]
        [SerializeField] private bool showDebugLogs = true;
        
        // Current values
        private float currentOxygen;
        private float currentStamina;
        
        // State
        private bool isRescueTriggered = false;
        
        // Events
        public event Action<float, float> OnOxygenChanged; // (current, max)
        public event Action<float, float> OnStaminaChanged; // (current, max)
        public event Action OnRescueProtocolTriggered;
        public event Action OnDaySkipped;
        
        // Properties
        public float CurrentOxygen => currentOxygen;
        public float MaxOxygen => maxOxygen;
        public float OxygenPercentage => maxOxygen > 0 ? currentOxygen / maxOxygen : 0f;
        
        public float CurrentStamina => currentStamina;
        public float MaxStamina => maxStamina;
        public float StaminaPercentage => maxStamina > 0 ? currentStamina / maxStamina : 0f;
        
        public bool IsAlive => currentOxygen > 0f && currentStamina > 0f;
        
        private void Awake()
        {
            // Initialize values
            currentOxygen = maxOxygen;
            currentStamina = maxStamina;
            
            // Validasi spawn point
            if (shipSpawnPoint == null)
            {
                Debug.LogWarning("[PlayerStatus] Ship Spawn Point tidak di-assign! Rescue Protocol mungkin tidak berfungsi.", this);
            }
        }
        
        private void Start()
        {
            // Trigger initial events
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        private void Update()
        {
            if (isRescueTriggered)
                return; // Jangan update jika sedang rescue
            
            // Auto deplete oxygen
            if (autoDepleteOxygen && currentOxygen > 0f)
            {
                DepleteOxygen(oxygenDepletionRate * Time.deltaTime);
            }
            
            // Auto deplete stamina (jika diperlukan)
            if (currentStamina > 0f)
            {
                DepleteStamina(staminaDepletionRate * Time.deltaTime);
            }
            
            // Check rescue condition
            if (currentOxygen <= 0f || currentStamina <= 0f)
            {
                TriggerRescueProtocol();
            }
        }
        
        /// <summary>
        /// Kurangi oksigen
        /// </summary>
        public void DepleteOxygen(float amount)
        {
            if (amount <= 0f) return;
            
            currentOxygen = Mathf.Max(0f, currentOxygen - amount);
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
            
            if (showDebugLogs && currentOxygen <= 0f)
            {
                Debug.Log("[PlayerStatus] Oksigen habis! Rescue Protocol akan di-trigger.");
            }
        }
        
        /// <summary>
        /// Tambah oksigen (untuk item/upgrade)
        /// </summary>
        public void AddOxygen(float amount)
        {
            if (amount <= 0f) return;
            
            currentOxygen = Mathf.Min(maxOxygen, currentOxygen + amount);
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
        }
        
        /// <summary>
        /// Kurangi stamina
        /// </summary>
        public void DepleteStamina(float amount)
        {
            if (amount <= 0f) return;
            
            currentStamina = Mathf.Max(0f, currentStamina - amount);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            
            if (showDebugLogs && currentStamina <= 0f)
            {
                Debug.Log("[PlayerStatus] Stamina habis! Rescue Protocol akan di-trigger.");
            }
        }
        
        /// <summary>
        /// Tambah stamina
        /// </summary>
        public void AddStamina(float amount)
        {
            if (amount <= 0f) return;
            
            currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        /// <summary>
        /// Trigger Rescue Protocol sesuai GDD v2.1:
        /// - TIDAK Game Over
        /// - Teleport ke Ship
        /// - Skip Day
        /// - Drop Items (akan di-handle oleh Inventory System nanti)
        /// </summary>
        public void TriggerRescueProtocol()
        {
            if (isRescueTriggered)
                return; // Prevent multiple triggers
            
            isRescueTriggered = true;
            
            if (showDebugLogs)
            {
                Debug.Log("[PlayerStatus] === RESCUE PROTOCOL TRIGGERED ===");
            }
            
            // Trigger event
            OnRescueProtocolTriggered?.Invoke();
            
            // Teleport ke ship setelah delay
            Invoke(nameof(TeleportToShip), rescueTeleportDelay);
        }
        
        /// <summary>
        /// Teleport player ke ship spawn point
        /// </summary>
        private void TeleportToShip()
        {
            if (shipSpawnPoint != null)
            {
                transform.position = shipSpawnPoint.position;
                
                if (showDebugLogs)
                {
                    Debug.Log($"[PlayerStatus] Player teleported to ship at {shipSpawnPoint.position}");
                }
            }
            else
            {
                Debug.LogWarning("[PlayerStatus] Ship Spawn Point tidak ada! Player tidak di-teleport.");
            }
            
            // Skip day
            SkipDay();
            
            // Drop items (akan di-handle oleh Inventory Manager nanti)
            DropItems();
            
            // Reset status setelah rescue
            ResetAfterRescue();
        }
        
        /// <summary>
        /// Skip day (akan di-integrate dengan Day/Night system nanti)
        /// </summary>
        private void SkipDay()
        {
            OnDaySkipped?.Invoke();
            
            if (showDebugLogs)
            {
                Debug.Log("[PlayerStatus] Day skipped (Rescue Protocol)");
            }
        }
        
        /// <summary>
        /// Drop items dari inventory sesuai Rescue Protocol.
        /// Logic: Drop 15% dari setiap stack item (sesuai keputusan PM)
        /// </summary>
        private void DropItems()
        {
            // TODO: Integrate dengan Inventory Manager
            // Logic: int amountToDrop = Mathf.CeilToInt(currentStackCount * 0.15f);
            // 
            // Pseudocode untuk implementasi nanti:
            // foreach (item in inventory.items)
            // {
            //     int currentStackCount = item.stackCount;
            //     int amountToDrop = Mathf.CeilToInt(currentStackCount * 0.15f);
            //     if (amountToDrop > 0)
            //     {
            //         inventory.RemoveItem(item.itemID, amountToDrop);
            //         // Spawn dropped item di world position (optional)
            //     }
            // }
            
            if (showDebugLogs)
            {
                Debug.Log("[PlayerStatus] Items dropped (Rescue Protocol) - 15% dari setiap stack. TODO: Integrate with Inventory System");
            }
        }
        
        /// <summary>
        /// Helper method untuk calculate drop amount (15% dari stack).
        /// Bisa dipanggil oleh Inventory Manager saat integrasi.
        /// </summary>
        /// <param name="stackCount">Jumlah item dalam stack</param>
        /// <returns>Jumlah item yang harus di-drop (rounded up)</returns>
        public static int CalculateDropAmount(int stackCount)
        {
            if (stackCount <= 0) return 0;
            return Mathf.CeilToInt(stackCount * 0.15f);
        }
        
        /// <summary>
        /// Reset status setelah rescue protocol selesai
        /// </summary>
        private void ResetAfterRescue()
        {
            // Reset oksigen dan stamina ke max
            currentOxygen = maxOxygen;
            currentStamina = maxStamina;
            
            OnOxygenChanged?.Invoke(currentOxygen, maxOxygen);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
            
            // Reset flag
            isRescueTriggered = false;
            
            if (showDebugLogs)
            {
                Debug.Log("[PlayerStatus] Status reset after rescue. Ready to continue.");
            }
        }
        
        /// <summary>
        /// Set spawn point untuk rescue protocol (bisa di-call dari external)
        /// </summary>
        public void SetShipSpawnPoint(Transform spawnPoint)
        {
            shipSpawnPoint = spawnPoint;
        }
    }
}

