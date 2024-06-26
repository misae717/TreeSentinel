using UnityEngine;
using Pathfinding;

public class SlimeAI : MonoBehaviour
{
    private Path path;
    private int currentWaypoint = 0;
    private Seeker seeker;
    private Rigidbody2D rb;
    private Transform target;

    public float moveSpeed = 3f;
    public float jumpForce = 10f;
    public float minJumpAngle = 75f;
    public float maxJumpAngle = 85f;
    public float nextWaypointDistance = 1f;
    public float groundCheckDistance = 0.1f;
    public float cliffCheckDistance = 1f;
    public float sideCheckDistance = 0.5f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    private bool isGrounded;
    private bool canJump = true;
    private float jumpCooldown = 0.5f;
    private float jumpTimer;
    public float minJumpHeight = 0.5f;

    public float playerProximityThreshold = 1f;
    private float pathUpdateCooldown = 0.5f;
    private float lastPathUpdateTime;

    private bool jumpQueued = false;
    private Vector2 queuedJumpDirection;
    private float lastJumpTime;
    public float jumpTimeout = 1f;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        
        target = GameObject.FindGameObjectWithTag("Player").transform;

        rb.gravityScale = 1;

        InvokeRepeating("UpdatePath", 0f, 0.5f);
    }

    void UpdatePath()
    {
        if (seeker.IsDone() && target != null)
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
            lastPathUpdateTime = Time.time;
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        CheckGrounded();
        
        if (path == null || currentWaypoint >= path.vectorPath.Count)
        {
            if (Time.time - lastPathUpdateTime > pathUpdateCooldown)
            {
                UpdatePath();
            }
            return;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        
        if (Vector2.Distance(rb.position, target.position) < playerProximityThreshold)
        {
            // When close to the player, stop moving and prepare to attack
            rb.velocity = Vector2.zero;
            return;
        }

        if (jumpQueued && isGrounded && canJump)
        {
            TryJump(queuedJumpDirection);
            jumpQueued = false;
        }
        else if (IsCliffAhead(direction) && isGrounded)
        {
            AvoidCliff();
        }
        else if (IsObstacleAhead(direction) && isGrounded)
        {
            HandleObstacle(direction);
        }
        else if (isGrounded)
        {
            Move(direction);
        }

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (jumpTimer > 0)
            jumpTimer -= Time.fixedDeltaTime;
    }

    void Move(Vector2 direction)
    {
        float moveHorizontal = direction.x;
        rb.velocity = new Vector2(moveHorizontal * moveSpeed, rb.velocity.y);

        if (moveHorizontal != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveHorizontal), 1, 1);
        }
    }

    bool ShouldJump(Vector2 direction)
    {
        bool cliffAhead = IsCliffAhead(direction);
        bool obstacleAhead = IsObstacleAhead(direction);

        bool needToJumpUp = false;
        if (currentWaypoint < path.vectorPath.Count - 1)
        {
            float heightDifference = path.vectorPath[currentWaypoint + 1].y - transform.position.y;
            needToJumpUp = heightDifference > minJumpHeight;
        }

        bool isMovingUpOrForward = direction.y >= 0 || Mathf.Abs(direction.x) > Mathf.Abs(direction.y);

        return (cliffAhead || obstacleAhead || needToJumpUp) && isGrounded && canJump && jumpTimer <= 0 && isMovingUpOrForward;
    }

    void TryJump(Vector2 direction)
    {
        if (!canJump || !isGrounded || Time.time - lastJumpTime < jumpTimeout) return;

        rb.velocity = Vector2.zero;
        Vector2 jumpDirection = CalculateJumpDirection(direction);
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
        
        canJump = false;
        jumpTimer = jumpCooldown;
        lastJumpTime = Time.time;
    }

    Vector2 CalculateJumpDirection(Vector2 pathDirection)
    {
        float baseAngle = (pathDirection.x >= 0) ? minJumpAngle : (180 - minJumpAngle);
        float angleAdjustment = Mathf.Lerp(0, maxJumpAngle - minJumpAngle, Mathf.Abs(pathDirection.y));
        float finalAngle = baseAngle + angleAdjustment;
        float radians = finalAngle * Mathf.Deg2Rad;
        Vector2 jumpDir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
        jumpDir.x = Mathf.Sign(pathDirection.x) * Mathf.Abs(jumpDir.x);
        return jumpDir.normalized;
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer) ||
                     Physics2D.Raycast(transform.position + Vector3.right * 0.4f, Vector2.down, groundCheckDistance, groundLayer) ||
                     Physics2D.Raycast(transform.position + Vector3.left * 0.4f, Vector2.down, groundCheckDistance, groundLayer);

        if (isGrounded && rb.velocity.y <= 0)
        {
            canJump = true;
        }
    }

    bool IsCliffAhead(Vector2 direction)
    {
        Vector2 rightCheck = (Vector2)transform.position + Vector2.right * 0.5f + direction * 0.5f;
        Vector2 leftCheck = (Vector2)transform.position + Vector2.left * 0.5f + direction * 0.5f;

        bool rightCliff = !Physics2D.Raycast(rightCheck, Vector2.down, cliffCheckDistance, groundLayer);
        bool leftCliff = !Physics2D.Raycast(leftCheck, Vector2.down, cliffCheckDistance, groundLayer);

        return rightCliff || leftCliff;
    }

    bool IsObstacleAhead(Vector2 direction)
    {
        Vector2 rayStart = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(rayStart, direction, sideCheckDistance, obstacleLayer);
        return hit;
    }

    void HandleObstacle(Vector2 direction)
    {
        if (isGrounded)
        {
            jumpQueued = true;
            queuedJumpDirection = direction;
        }
    }

    void AvoidCliff()
    {
        // Stop movement to avoid falling off the cliff
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(transform.position + Vector3.right * 0.4f, transform.position + Vector3.right * 0.4f + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(transform.position + Vector3.left * 0.4f, transform.position + Vector3.left * 0.4f + Vector3.down * groundCheckDistance);

        if (path != null && currentWaypoint < path.vectorPath.Count)
        {
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - (Vector2)transform.position).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction * sideCheckDistance));
        }

        if (jumpQueued)
        {
            Vector2 jumpDir = CalculateJumpDirection(queuedJumpDirection);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(jumpDir * 2));
        }

        // Visualize player proximity threshold
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, playerProximityThreshold);
    }
}
