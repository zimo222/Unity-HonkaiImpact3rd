using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    #region Components
    public Animator anim { get; private set; }
    public Rigidbody rb { get; private set; }
    public EntityFX fx { get; private set; }
    #endregion

    [Header("Knockback info")]
    [SerializeField] protected Vector3 knockbackDirection;
    [SerializeField] protected float knockbackDuration;
    protected bool isKnocked;

    [Header("Collision info")]
    public Transform attackCheck;
    public float attackCheckRadius;
    [SerializeField] protected Transform groundCheck;
    [SerializeField] protected float groundCheckDistance = 0.3f;
    [SerializeField] protected LayerMask whatIsGround;

    [Header("Movement")]
    [SerializeField] protected float MoveSpeed = 5f;
    [SerializeField] protected float rotationSpeed = 10f;

    public Vector3 facingDir { get; private set; } = Vector3.forward;
    protected Quaternion targetRotation;

    protected virtual void Awake() { }

    protected virtual void Start()
    {
        fx = GetComponentInChildren<EntityFX>();
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update() { }

    protected virtual void FixedUpdate()
    {
        HandleRotation();
    }

    public virtual void Damage()
    {
        if (fx != null)
            fx.StartCoroutine("FlashFX");
        StartCoroutine("HitKnockback");
        Debug.Log(gameObject.name + " was damaged");
    }

    protected virtual IEnumerator HitKnockback()
    {
        isKnocked = true;

        // 3D knockback - can be direction based
        Vector3 knockbackForce = new Vector3(
            knockbackDirection.x * -Mathf.Sign(facingDir.x),
            knockbackDirection.y,
            knockbackDirection.z * -Mathf.Sign(facingDir.z)
        );
        rb.velocity = knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);
        isKnocked = false;
    }

    #region Movement
    public void SetZeroVelocity()
    {
        if (isKnocked) return;
        rb.velocity = Vector3.zero;
    }

    public void SetVelocity(Vector3 direction, float speed, bool shouldUpdateFacingDir = true)
    {
        if (isKnocked) return;

        Vector3 moveDirection = new Vector3(direction.x, 0, direction.z).normalized;
        rb.velocity = new Vector3(moveDirection.x * speed, rb.velocity.y, moveDirection.z * speed);

        // Update facing direction if moving
        if (moveDirection.magnitude > 0.1f && shouldUpdateFacingDir)
        {
            facingDir = moveDirection;
        }
    }

    public void SetVerticalVelocity(float yVelocity)
    {
        if (isKnocked) return;
        rb.velocity = new Vector3(rb.velocity.x, yVelocity, rb.velocity.z);
    }

    protected virtual void HandleRotation()
    {
        if (facingDir.magnitude > 0.1f)
        {
            targetRotation = Quaternion.LookRotation(facingDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }
    #endregion

    #region Collision
    public virtual bool IsGroundDetected()
    {
        return Physics.CheckBox(groundCheck.position,
                                groundCheck.localScale / 2, // half extent
                                groundCheck.rotation,
                                whatIsGround);
    }

    public virtual bool IsWallDetected(Vector3 direction, float distance = 0.5f)
    {
        return Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, distance, whatIsGround);
    }

    public virtual bool IsObstacleInFront(float distance = 1f)
    {
        Vector3 checkPosition = transform.position + Vector3.up * 0.5f;
        return Physics.Raycast(checkPosition, transform.forward, distance, whatIsGround);
    }

    protected virtual void OnDrawGizmos()
    {
        if (groundCheck != null)
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);

        if (attackCheck != null)
            Gizmos.DrawWireSphere(attackCheck.position, attackCheckRadius);
    }
    #endregion

    public void SetFacingDirection(Vector3 newDirection)
    {
        // 确保方向不为零向量
        if (newDirection.magnitude > 0.1f)
        {
            facingDir = newDirection;

            // 可选：如果需要立即更新旋转
            if (facingDir.magnitude > 0.1f)
            {
                targetRotation = Quaternion.LookRotation(facingDir);
            }
        }
    }
}