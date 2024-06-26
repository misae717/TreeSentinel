using UnityEngine;
using UnityEngine.AI;

public class SlimeController : MonoBehaviour
{
    public float health = 100f;
    public float moveSpeed = 3f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform player;
    private float lastAttackTime;

    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (health <= 0)
        {
            Die();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && Time.time - lastAttackTime > attackCooldown)
        {
            Attack();
        }
        else
        {
            Move();
        }
    }

    private void Move()
    {
        agent.SetDestination(player.position);
        animator.SetBool("IsMoving", true);
    }

    private void Attack()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
        // Implement attack logic here
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        animator.SetTrigger("Hurt");
    }

    private void Die()
    {
        animator.SetTrigger("Death");
        // Implement death logic (e.g., disable components, spawn loot, etc.)
    }
}