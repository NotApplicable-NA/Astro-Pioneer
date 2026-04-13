using UnityEngine;
using System.Collections.Generic;
using AstroPioneer.Machines.Automation;

namespace AstroPioneer.Managers
{
    /// <summary>
    /// BotManager — Task queue manager that assigns transport tasks to available bots.
    /// </summary>
    public class BotManager : MonoBehaviour
    {
        public static BotManager Instance { get; private set; }

        private readonly List<TransportBot> activeBots = new List<TransportBot>();
        private readonly Queue<(Vector3 source, Vector3 target)> taskQueue = new Queue<(Vector3, Vector3)>();

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update() => AssignTasks();

        public void RegisterBot(TransportBot bot)
        {
            if (!activeBots.Contains(bot))
                activeBots.Add(bot);
        }

        public void UnregisterBot(TransportBot bot) => activeBots.Remove(bot);

        public void RequestTransport(Vector3 sourcePos, Vector3 targetPos)
        {
            taskQueue.Enqueue((sourcePos, targetPos));
        }

        private void AssignTasks()
        {
            if (taskQueue.Count == 0 || activeBots.Count == 0) return;

            for (int i = activeBots.Count - 1; i >= 0; i--)
            {
                if (taskQueue.Count == 0) break;

                var bot = activeBots[i];
                if (bot == null)
                {
                    activeBots.RemoveAt(i);
                    continue;
                }

                if (bot.IsAvailable)
                {
                    var task = taskQueue.Dequeue();
                    bot.AssignTask(task.source, task.target);
                }
            }
        }
    }
}
