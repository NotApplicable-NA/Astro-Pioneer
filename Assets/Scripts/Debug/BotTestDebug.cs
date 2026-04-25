using UnityEngine;
using AstroPioneer.Managers;

namespace AstroPioneer.DebugTools
{
    /// <summary>
    /// BotTestDebug — V22 DEPRECATED.
    /// Task queuing is now handled by BotStation (Hub-Centric Architecture).
    /// This debug tool is kept for backward compatibility but does nothing.
    /// </summary>
    public class BotTestDebug : MonoBehaviour
    {
        [Header("References")]
        public Transform pickupTarget;
        public Transform dropoffTarget;

        [ContextMenu("Test Transport Task")]
        public void TestTransport()
        {
            Debug.LogWarning("[BotTestDebug] DEPRECATED: Task queuing is now handled by BotStation. Place a BotStation near machines instead.");
        }
    }
}
