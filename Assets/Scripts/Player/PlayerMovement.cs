using UnityEngine;

namespace RoyalFlush.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        
        private Rigidbody2D rb;
        private Animator animator;
        private Vector2 movement;
        private bool isFacingRight = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            // Get input
            movement.x = Input.GetAxisRaw("Horizontal");
            
            // Update Animation
            animator.SetFloat("Speed", Mathf.Abs(movement.x));

            // Flip character
            if (movement.x > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (movement.x < 0 && isFacingRight)
            {
                Flip();
            }
        }

        private void FixedUpdate()
        {
            // Apply movement
            rb.velocity = new Vector2(movement.x * moveSpeed, rb.velocity.y);
        }

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
