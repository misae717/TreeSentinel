using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class DestroyOnEnterTilemap : MonoBehaviour
{
    public string targetTilemapTag = "DestructionZone";
    public float destroyDelay = 3f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object is a Tilemap with the specified tag
        Tilemap tilemap = collision.GetComponent<Tilemap>();
        if (tilemap != null && tilemap.CompareTag(targetTilemapTag))
        {
            // Start the coroutine to destroy the object after the specified delay
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(destroyDelay);

        // Destroy the game object
        Destroy(gameObject);
    }
}