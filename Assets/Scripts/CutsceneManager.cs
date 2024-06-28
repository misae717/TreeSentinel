using UnityEngine;
using System.Collections;
using TreeController;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;

    public void StartCutscene()
    {
        if (playerController != null)
        {
            playerController.ToggleCutsceneMode(true);
        }
        else
        {
            Debug.LogError("PlayerController reference is missing in CutsceneManager");
        }
        
    }

    public void EndCutscene()
    {
        if (playerController != null)
        {
            playerController.ToggleCutsceneMode(false);
        }
        else
        {
            Debug.LogError("PlayerController reference is missing in CutsceneManager");
        }
        
    }
}