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

            Debug.Log("=== URP 2D Renderer Verification ===");

            // Check Camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var cameraData = mainCamera.GetUniversalAdditionalCameraData();
                if (cameraData != null)
                {
                    Debug.Log($"✅ Camera has URP Additional Data component");
                    Debug.Log($"   - Renderer Index: {cameraData.antialiasing}");
                    Debug.Log($"   - Camera Type: {mainCamera.orthographic} (Orthographic: {mainCamera.orthographic})");
                }
                else
                {
                    Debug.LogError("❌ Camera missing URP Additional Data component!");
                }
            }
            else
            {
                Debug.LogError("❌ Main Camera not found!");
            }

            // Check Render Pipeline
            var pipeline = UniversalRenderPipeline.asset;
            if (pipeline != null)
            {
                Debug.Log($"✅ URP Pipeline Asset assigned: {pipeline.name}");
            }
            else
            {
                Debug.LogError("❌ URP Pipeline Asset not assigned!");
            }

            // Check Quality Settings
            var qualitySettings = QualitySettings.renderPipeline;
            if (qualitySettings != null)
            {
                Debug.Log($"✅ Quality Settings using: {qualitySettings.name}");
            }

            Debug.Log("=== Verification Complete ===");
        }

        private void OnDrawGizmos()
        {
            // Visual indicator URP is active
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
