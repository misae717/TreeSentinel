using UnityEngine;
using Cinemachine;
using System.Collections;
using TreeController;

public class CutsceneManager : MonoBehaviour
{
    public PlayerController playerController;
    public CompanionBehavior companion;
    public CinemachineVirtualCamera mainCamera;
    public CinemachineVirtualCamera cutsceneCamera;
    public float zoomDuration = 1.5f;
    public float dialogueDuration = 5f;

    private bool cutsceneTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!cutsceneTriggered && collision.CompareTag("Player"))
        {
            cutsceneTriggered = true;
            StartCoroutine(StartCutscene());
        }
    }

    private IEnumerator StartCutscene()
    {
        // Disable player control
        playerController.ToggleCutsceneMode(true);

        // Switch to cutscene camera
        mainCamera.Priority = 0;
        cutsceneCamera.Priority = 10;

        // Wait for camera transition
        yield return new WaitForSeconds(zoomDuration);

        // Start dialogue
        // TODO: Implement your dialogue system here
        Debug.Log("Starting dialogue between player and companion");

        // Wait for dialogue duration
        yield return new WaitForSeconds(dialogueDuration);

        // Start companion's cutscene sequence
        companion.TriggerCutscene();

        // Wait for companion's animation to finish (floating + attaching)
        yield return new WaitForSeconds(companion.floatDuration + companion.attachDuration + 2f); // Added 2 seconds for dialogue wait in companion script

        // Switch back to main camera
        cutsceneCamera.Priority = 0;
        mainCamera.Priority = 10;

        // Re-enable player control
        playerController.ToggleCutsceneMode(false);

        // Cutscene completed
        Debug.Log("Cutscene completed");
    }
}