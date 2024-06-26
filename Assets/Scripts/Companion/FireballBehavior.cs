using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballBehavior : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public GameObject visual; // Reference to the child GameObject containing the sprite

    private Vector2 direction;

    void Start()
    {
        Destroy(gameObject, 5f); // Destroy the fireball after 5 seconds to avoid clutter
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir;
        RotateVisual();
    }

    void RotateVisual()
    {
        if (visual != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            visual.transform.rotation = Quaternion.Euler(0, 0, angle + 90); // Adjust by -90 degrees
        }
        else
        {
            Debug.LogError("Visual GameObject is not assigned.");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Enemy"))
        {
            // Assuming the enemy has a TakeDamage method
            collision.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
        }
    }
}
