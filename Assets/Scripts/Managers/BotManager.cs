using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Machines.Automation;
using AstroPioneer.Core;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// BotManager — V22 Refactored: Population tracker only.
    /// 
    /// Task queuing has been moved to BotStation (Hub-Centric Architecture).
    /// This manager now only tracks the total number of bots in the world
    /// for UI display and debug purposes.
    /// </summary>
    public class BotManager : MonoBehaviour
    {
        public static BotManager Instance { get; private set; }

        private readonly List<TransportBot> allBots = new List<TransportBot>();

        void Awake()
        {
            if (Instance != null) { Destroy(this); return; }
            Instance = this;
            ServiceLocator.Register(this);
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; ServiceLocator.Unregister<BotManager>(); }
        }

        public void RegisterBot(TransportBot bot)
        {
            if (bot != null && !allBots.Contains(bot))
                allBots.Add(bot);
        }

        public void UnregisterBot(TransportBot bot) => allBots.Remove(bot);

        /// <summary>Total bots alive in the world (for UI/debug).</summary>
        public int TotalBotCount
        {
            get
            {
                // Clean nulls lazily
                for (int i = allBots.Count - 1; i >= 0; i--)
                    if (allBots[i] == null) allBots.RemoveAt(i);
                return allBots.Count;
            }
        }

        public IReadOnlyList<TransportBot> GetAllBots() => allBots;
    }
}
