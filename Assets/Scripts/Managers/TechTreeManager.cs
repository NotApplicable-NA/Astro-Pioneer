using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Core;

namespace AstroPioneer.Managers
{
    [System.Serializable]
    public class TechNode
    {
        public string techID;
        public string displayName;
        public int cost;
        public bool isUnlocked;
        public List<string> unlocksBlueprintIDs;
        public List<string> prerequisites;
    }

    /// <summary>
    /// Manages Research Points acquired from scanning and unlocking new Blueprints.
    /// </summary>
    public class TechTreeManager : MonoBehaviour
    {
        public static TechTreeManager Instance { get; private set; }

        public int ResearchPoints { get; private set; } = 0;

        [Header("Tech Tree")]
        [SerializeField] private List<TechNode> techNodes = new List<TechNode>();
        
        private HashSet<string> unlockedBlueprints = new HashSet<string>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy()
        {
            if (Instance == this) { Instance = null; ServiceLocator.Unregister<TechTreeManager>(); }
        }

        public void AddResearchPoints(int amount)
        {
            ResearchPoints += amount;
        }

        public bool UnlockTech(string techID)
        {
            TechNode node = null;
            foreach (var candidate in techNodes)
            {
                if (candidate.techID == techID)
                {
                    node = candidate;
                    break;
                }
            }
            if (node == null) return false;

            if (node.isUnlocked) return true;

            // Check prerequisites
            foreach (var req in node.prerequisites)
            {
                TechNode preReqNode = null;
                foreach (var candidate in techNodes)
                {
                    if (candidate.techID == req)
                    {
                        preReqNode = candidate;
                        break;
                    }
                }
                if (preReqNode == null || !preReqNode.isUnlocked)
                {
                    return false;
                }
            }

            if (ResearchPoints >= node.cost)
            {
                ResearchPoints -= node.cost;
                node.isUnlocked = true;
                
                foreach (var bp in node.unlocksBlueprintIDs)
                {
                    unlockedBlueprints.Add(bp);
                }
                return true;
            }
            return false;
        }

        public bool IsBlueprintUnlocked(string blueprintID)
        {
            return unlockedBlueprints.Contains(blueprintID);
        }
    }
}
