using UnityEngine;
using AstroPioneer.Managers;

namespace AstroPioneer.DebugTools
{
    public class BotTestDebug : MonoBehaviour
    {
        [Header("References")]
        public Transform pickupTarget;
        public Transform dropoffTarget;

        [Header("Actions")]
        [Tooltip("Click the context menu (3 dots) or use this bool to trigger")]
        public bool triggerTransport = false;

        void Update()
        {
            if (triggerTransport)
            {
                triggerTransport = false;
                TestTransport();
            }
        }

        [ContextMenu("Test Transport Task")]
        public void TestTransport()
        {
            if (BotManager.Instance != null && pickupTarget != null && dropoffTarget != null)
            {
                BotManager.Instance.RequestTransport(pickupTarget.position, dropoffTarget.position);
            }
            else
            {
            }
        }
    }
}
