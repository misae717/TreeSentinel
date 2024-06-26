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
    public float jumpForce = 7f;
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
    public float extraJumpHeight = 0.2f;

    public float backawayDistance = 1f;
    public float backawaySpeed = 2f;
    private bool isBackingAway = false;
    private Vector2 backawayDirection;

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
            seeker.StartPath(rb.position, target.position, OnPathComplete);
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
            return;

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        
        if (isBackingAway)
        {
            BackAway();
        }
        else if (IsCliffAhead(direction))
        {
            if (ShouldJump(direction))
            {
                TryJump(direction);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y); // Stop horizontal movement
            }
        }
        else if (IsObstacleAhead(direction))
        {
            HandleObstacle(direction);
        }
        else
        {
            Move(direction);
        }

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
            currentWaypoint++;

        // Update jump timer
        if (jumpTimer > 0)
            jumpTimer -= Time.fixedDeltaTime;
    }

    void Move(Vector2 direction)
    {
        float moveHorizontal = direction.x;
        
        // Apply horizontal movement
        rb.velocity = new Vector2(moveHorizontal * moveSpeed, rb.velocity.y);

        // Flip sprite if necessary
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
        if (!canJump) return;

        Vector2 jumpDirection = CalculateJumpDirection(direction);
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
        
        canJump = false;
        jumpTimer = jumpCooldown;
        Debug.Log("Jump executed in direction: " + jumpDirection);
    }

    Vector2 CalculateJumpDirection(Vector2 pathDirection)
    {
        float pathAngle = Mathf.Atan2(pathDirection.y, pathDirection.x);
        float jumpAngle = pathAngle + extraJumpHeight;
        return new Vector2(Mathf.Cos(jumpAngle), Mathf.Sin(jumpAngle)).normalized;
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        if (isGrounded && rb.velocity.y <= 0)
        {
            canJump = true;
        }
    }

    bool IsCliffAhead(Vector2 direction)
    {
        Vector2 rayStart = (Vector2)transform.position + direction * 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, cliffCheckDistance, groundLayer);
        Debug.DrawRay(rayStart, Vector2.down * cliffCheckDistance, hit ? Color.green : Color.red);
        return !hit;
    }

    bool IsObstacleAhead(Vector2 direction)
    {
        Vector2 rayStart = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(rayStart, direction, sideCheckDistance, obstacleLayer);
        Debug.DrawRay(rayStart, direction * sideCheckDistance, hit ? Color.red : Color.green);
        return hit;
    }

    void HandleObstacle(Vector2 direction)
    {
        if (!isBackingAway)
        {
            // Start backing away
            isBackingAway = true;
            backawayDirection = -direction; // Back away in the opposite direction of the obstacle
            return;
        }

        if (ShouldJump(direction))
        {
            isBackingAway = false;
            TryJump(direction);
        }
    }

    void BackAway()
    {
        rb.velocity = backawayDirection * backawaySpeed;
        
        // Check if we've backed away enough
        if (!IsObstacleAhead(-backawayDirection))
        {
            isBackingAway = false;
        }
    }

    void OnDrawGizmos()
    {
        // Visualize ground check
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);

        // Visualize cliff checks
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.right * 0.5f, transform.position + Vector3.right * 0.5f + Vector3.down * cliffCheckDistance);
        Gizmos.DrawLine(transform.position + Vector3.left * 0.5f, transform.position + Vector3.left * 0.5f + Vector3.down * cliffCheckDistance);

        // Visualize side obstacle checks
        if (path != null && currentWaypoint < path.vectorPath.Count)
        {
            Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - (Vector2)transform.position).normalized;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(direction * sideCheckDistance));
            
            // Draw opposite side check
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(-direction * sideCheckDistance));
        }

        // Visualize backaway direction when active
        if (isBackingAway)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)(backawayDirection * backawayDistance));
        }
    }
}