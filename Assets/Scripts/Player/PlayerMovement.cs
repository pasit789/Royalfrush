using UnityEngine;

namespace RoyalFlush.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerMovement : MonoBehaviour
    {
        public static PlayerMovement Instance { get; private set; }

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 8f;
        [SerializeField] private float deceleration = 12f;
        
        private Rigidbody2D rb;
        private Animator animator;
        private float currentSpeed;
        private float targetSpeed;
        private bool isFacingRight = true;
        private bool canMove = true;

        // Cached animator parameter hashes for performance
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            
            // Freeze rotation so character doesn't tip over
            rb.freezeRotation = true;
        }

        /// <summary>
        /// Enable or disable player movement. Called by DialogueManager, SceneTransitionManager, etc.
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            canMove = enabled;
            if (!enabled)
            {
                currentSpeed = 0f;
                targetSpeed = 0f;
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                }
                UpdateAnimator(0f);
            }
        }

        /// <summary>
        /// Teleport the player to a specific world position (used by SceneTransitionManager).
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            transform.position = position;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            currentSpeed = 0f;
            UpdateAnimator(0f);
        }

        private void Update()
        {
            if (!canMove)
            {
                UpdateAnimator(0f);
                return;
            }

            // Get input
            float inputX = Input.GetAxisRaw("Horizontal");
            targetSpeed = inputX * moveSpeed;
            
            // Smooth acceleration / deceleration
            if (Mathf.Abs(inputX) > 0.01f)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
            }

            // Update Animation
            UpdateAnimator(Mathf.Abs(currentSpeed) / moveSpeed);

            // Flip character based on input direction
            if (inputX > 0.01f && !isFacingRight)
            {
                Flip();
            }
            else if (inputX < -0.01f && isFacingRight)
            {
                Flip();
            }
        }

        private void FixedUpdate()
        {
            if (!canMove) return;

            // Apply smoothed movement
            rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
        }

        private void UpdateAnimator(float normalizedSpeed)
        {
            if (animator == null) return;
            
            animator.SetFloat(SpeedHash, normalizedSpeed);
            
            // Optional: set IsMoving bool if the animator has it
            // This won't error if the parameter doesn't exist in the controller
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
