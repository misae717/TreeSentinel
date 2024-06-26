using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndpoint : MonoBehaviour
{
    public string nextLevelName = "NextLevel";  // Name of the next scene to load
    public float delayBeforeLoading = 1f;  // Optional delay before loading the next level
    public GameObject victoryEffect;  // Optional particle effect or other GameObject to spawn on victory

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            EndLevel();
        }
    }

    private void EndLevel()
    {


        // Load the next level after a delay
        Invoke("LoadNextLevel", delayBeforeLoading);
    }

    private void LoadNextLevel()
    {
        SceneManager.LoadScene(nextLevelName);
    }
}