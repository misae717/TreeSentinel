using UnityEngine;

public class SlimeHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("Death");
        // Disable the slime's ability to interact
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SlimeAI>().enabled = false;
        this.enabled = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Assuming the player has a script named PlayerHealth
            collision.gameObject.GetComponent<PlayerHealth>().TakeDamage(10);
        }
    }
}
