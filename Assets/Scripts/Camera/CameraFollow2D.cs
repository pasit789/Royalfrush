using System.Collections;
using UnityEngine;
using RoyalFlush.Player;

namespace RoyalFlush.CameraSystem
{
    public class CameraFollow2D : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

        [Header("Level Bounds (Optional)")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;
        [SerializeField] private float minY = -5f;
        [SerializeField] private float maxY = 5f;

        [Header("Camera Shake")]
        [SerializeField] private float defaultShakeDuration = 0.3f;
        [SerializeField] private float defaultShakeIntensity = 0.15f;

        [Header("Cinematic Zoom")]
        [SerializeField] private float defaultOrthoSize = 5f;
        [SerializeField] private float dialogueZoomSize = 4f;
        [SerializeField] private float zoomSmoothSpeed = 3f;

        private Vector3 velocity = Vector3.zero;
        private Camera cam;

        // Shake state
        private float shakeTimeRemaining = 0f;
        private float shakeIntensity = 0f;

        // Focus state
        private Transform focusTarget;
        private bool isFocusingOnTarget = false;
        private float targetOrthoSize;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                defaultOrthoSize = cam.orthographicSize;
            }
            targetOrthoSize = defaultOrthoSize;
        }

        private void Start()
        {
            // Auto-find player target if not assigned
            if (target == null && PlayerMovement.Instance != null)
            {
                target = PlayerMovement.Instance.transform;
            }
        }

        private void LateUpdate()
        {
            // Resolve target
            Transform activeTarget = isFocusingOnTarget && focusTarget != null ? focusTarget : target;

            if (activeTarget == null)
            {
                if (PlayerMovement.Instance != null)
                {
                    target = PlayerMovement.Instance.transform;
                    activeTarget = target;
                }
                else
                {
                    return;
                }
            }

            // Calculate desired position
            Vector3 desiredPosition;
            if (isFocusingOnTarget && focusTarget != null)
            {
                // When focusing during dialogue, look at the midpoint between player and NPC
                if (target != null)
                {
                    Vector3 midpoint = (target.position + focusTarget.position) * 0.5f;
                    desiredPosition = midpoint + offset;
                }
                else
                {
                    desiredPosition = focusTarget.position + offset;
                }
            }
            else
            {
                desiredPosition = activeTarget.position + offset;
            }

            // Apply bounds clamping
            if (useBounds)
            {
                desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
                desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
            }

            // Always lock Z axis for 2D orthographic camera
            desiredPosition.z = offset.z;

            // Smooth damping for cinematic camera movement
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

            // Apply camera shake offset
            if (shakeTimeRemaining > 0f)
            {
                Vector2 shakeOffset = Random.insideUnitCircle * shakeIntensity;
                transform.position += new Vector3(shakeOffset.x, shakeOffset.y, 0f);
                shakeTimeRemaining -= Time.deltaTime;

                // Diminish shake over time
                shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, Time.deltaTime * 5f);
            }

            // Smooth zoom
            if (cam != null && cam.orthographic)
            {
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, Time.deltaTime * zoomSmoothSpeed);
            }
        }

        // ==================== PUBLIC API ====================

        /// <summary>
        /// Trigger a camera shake effect (e.g. for dramatic moments).
        /// </summary>
        public void Shake(float duration = -1f, float intensity = -1f)
        {
            shakeTimeRemaining = duration > 0f ? duration : defaultShakeDuration;
            shakeIntensity = intensity > 0f ? intensity : defaultShakeIntensity;
        }

        /// <summary>
        /// Focus camera on a specific target (used during dialogue to frame both characters).
        /// </summary>
        public void FocusOn(Transform npcTransform)
        {
            if (npcTransform == null) return;
            focusTarget = npcTransform;
            isFocusingOnTarget = true;
            targetOrthoSize = dialogueZoomSize;
        }

        /// <summary>
        /// Release focus and return to following the player.
        /// </summary>
        public void ReleaseFocus()
        {
            focusTarget = null;
            isFocusingOnTarget = false;
            targetOrthoSize = defaultOrthoSize;
        }

        /// <summary>
        /// Set custom zoom level (e.g. for cutscenes).
        /// </summary>
        public void SetZoom(float orthoSize)
        {
            targetOrthoSize = orthoSize;
        }

        /// <summary>
        /// Reset zoom to default.
        /// </summary>
        public void ResetZoom()
        {
            targetOrthoSize = defaultOrthoSize;
        }

        /// <summary>
        /// Snap camera to target immediately (no smooth lerp). Useful after scene load.
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null && PlayerMovement.Instance != null)
            {
                target = PlayerMovement.Instance.transform;
            }

            if (target != null)
            {
                Vector3 snapPos = target.position + offset;
                snapPos.z = offset.z;
                if (useBounds)
                {
                    snapPos.x = Mathf.Clamp(snapPos.x, minX, maxX);
                    snapPos.y = Mathf.Clamp(snapPos.y, minY, maxY);
                }
                transform.position = snapPos;
                velocity = Vector3.zero;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!useBounds) return;

            Gizmos.color = Color.cyan;
            Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
            Vector3 size = new Vector3(Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY), 0.1f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
