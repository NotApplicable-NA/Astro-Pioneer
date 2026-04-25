using UnityEngine;
using AstroPioneer.Player;

namespace AstroPioneer.Player
{
    /// <summary>
    /// CameraFollow - Smooth 2D camera that tracks the Player.
    /// Attach to the Main Camera.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Settings")]
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

        private Vector3 currentVelocity = Vector3.zero;

        void Start()
        {
            if (target == null && PlayerToolState.Instance != null)
                target = PlayerToolState.Instance.transform;
        }

        void LateUpdate()
        {
            if (target == null) return;

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        }

        /// <summary>
        /// Assign a new follow target at runtime.
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}
