using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace AstroPioneer.Editor
{
    /// <summary>
    /// Editor helper untuk verify URP 2D Renderer configuration
    /// </summary>
    public class URPVerificationHelper : MonoBehaviour
    {
        [Header("URP Verification")]
        [SerializeField] private bool showDebugInfo = true;

        private void Start()
        {
            VerifyURPConfiguration();
        }

        private void VerifyURPConfiguration()
        {
            if (!showDebugInfo) return;

            // Check Camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var cameraData = mainCamera.GetUniversalAdditionalCameraData();
                if (cameraData != null)
                {
                }
                else
                {
                }
            }
            else
            {
            }

            // Check Render Pipeline
            var pipeline = UniversalRenderPipeline.asset;
            if (pipeline != null)
            {
            }
            else
            {
            }

            // Check Quality Settings
            var qualitySettings = QualitySettings.renderPipeline;
            if (qualitySettings != null)
            {
            }
        }

        private void OnDrawGizmos()
        {
            // Visual indicator URP is active
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
