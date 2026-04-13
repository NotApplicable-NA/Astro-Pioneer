using UnityEngine;

namespace AstroPioneer.Machines
{
    public enum FacingDirection
    {
        South = 0,
        West = 1,
        North = 2,
        East = 3
    }

    /// <summary>
    /// Handles 4-directional sprite switching for machines.
    /// Assign 4 sprites (S, W, N, E) in Inspector.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class MachineDirection : MonoBehaviour
    {
        [Header("Directional Sprites (S, W, N, E)")]
        [SerializeField] private Sprite spriteS;
        [SerializeField] private Sprite spriteW;
        [SerializeField] private Sprite spriteN;
        [SerializeField] private Sprite spriteE;

        [Header("Current Direction")]
        [SerializeField] private FacingDirection currentDirection = FacingDirection.South;

        private SpriteRenderer sr;

        public FacingDirection CurrentDirection => currentDirection;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            ApplyDirection();
        }

        /// <summary>
        /// Set direction and update sprite. Called by placement system or player input.
        /// </summary>
        public void SetDirection(FacingDirection dir)
        {
            currentDirection = dir;
            ApplyDirection();
        }

        /// <summary>
        /// Rotate 90° clockwise: S → W → N → E → S
        /// </summary>
        public void RotateClockwise()
        {
            currentDirection = (FacingDirection)(((int)currentDirection + 1) % 4);
            ApplyDirection();
        }

        /// <summary>
        /// Rotate 90° counter-clockwise: S → E → N → W → S
        /// </summary>
        public void RotateCounterClockwise()
        {
            currentDirection = (FacingDirection)(((int)currentDirection + 3) % 4);
            ApplyDirection();
        }

        private void ApplyDirection()
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (sr == null) return;

            switch (currentDirection)
            {
                case FacingDirection.South: sr.sprite = spriteS; break;
                case FacingDirection.West:  sr.sprite = spriteW; break;
                case FacingDirection.North: sr.sprite = spriteN; break;
                case FacingDirection.East:  sr.sprite = spriteE; break;
            }
        }

        // Editor: preview direction changes instantly
#if UNITY_EDITOR
        void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null) ApplyDirection();
            };
        }
#endif
    }
}
