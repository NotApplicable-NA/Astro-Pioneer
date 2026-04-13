using UnityEngine;
using System.Collections.Generic;

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
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddResearchPoints(int amount)
        {
            ResearchPoints += amount;
        }

        public bool UnlockTech(string techID)
        {
            TechNode node = techNodes.Find(n => n.techID == techID);
            if (node == null) return false;

            if (node.isUnlocked) return true;

            // Check prerequisites
            foreach (var req in node.prerequisites)
            {
                TechNode preReqNode = techNodes.Find(n => n.techID == req);
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
