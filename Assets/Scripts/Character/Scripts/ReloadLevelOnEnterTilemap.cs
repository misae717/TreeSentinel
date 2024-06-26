using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using System.Collections;

public class ReloadLevelOnEnterTilemap : MonoBehaviour
{
    public string targetTilemapTag = "DestructionZone";
    public float reloadDelay = 3f;
    public bool isPlayer = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the collided object is a Tilemap with the specified tag
        Tilemap tilemap = collision.GetComponent<Tilemap>();
        if (tilemap != null && tilemap.CompareTag(targetTilemapTag))
        {
            if (isPlayer)
            {
                // Start the coroutine to reload the level after the specified delay
                StartCoroutine(ReloadLevelAfterDelay());
            }
            else
            {
                // For non-player objects, destroy them as before
                StartCoroutine(DestroyAfterDelay());
            }
        }
    }

    private IEnumerator ReloadLevelAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(reloadDelay);

        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator DestroyAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(reloadDelay);

        // Destroy the game object
        Destroy(gameObject);
    }
}