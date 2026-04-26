using UnityEngine;
using System;
using AstroPioneer.Core;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// CurrencyManager — Tracks Credits (spending currency) and Trust Points (progression gate).
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [Header("Starting Values")]
        [SerializeField] private int startingCredits = 500;
        [SerializeField] private int startingTrust = 0;

        [Header("Runtime")]
        [SerializeField] private int currentCredits;
        [SerializeField] private int currentTrust;

        public event Action<int> OnCreditsChanged;
        public event Action<int> OnTrustChanged;

        public int CurrentCredits => currentCredits;
        public int CurrentTrust => currentTrust;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            currentCredits = startingCredits;
            currentTrust = startingTrust;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ─── Credits ───

        public void LoadCredits(int amount)
        {
            currentCredits = amount;
            OnCreditsChanged?.Invoke(currentCredits);
        }

        public void AddCredits(int amount)
        {
            if (amount <= 0) return;
            currentCredits += amount;
            OnCreditsChanged?.Invoke(currentCredits);
        }

        public bool TrySpendCredits(int amount)
        {
            if (amount <= 0 || currentCredits < amount) return false;
            currentCredits -= amount;
            OnCreditsChanged?.Invoke(currentCredits);
            return true;
        }

        // ─── Trust ───

        public void AddTrust(int amount)
        {
            if (amount <= 0) return;
            currentTrust += amount;
            OnTrustChanged?.Invoke(currentTrust);
        }

        public bool HasTrust(int amount) => currentTrust >= amount;

        public bool TrySpendTrust(int amount)
        {
            if (amount <= 0 || currentTrust < amount) return false;
            currentTrust -= amount;
            OnTrustChanged?.Invoke(currentTrust);
            return true;
        }
    }
}
