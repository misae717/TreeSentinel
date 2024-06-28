using UnityEngine;
using TMPro;

public class SignInteraction : MonoBehaviour
{
    [TextArea(2, 5)]  // This allows for easier multi-line input in the Inspector
    public string signText = "Hello, Player!\nWelcome to the game!";
    public float displayDistance = 2f;
    public int textSortingOrder = 100;
    public string sortingLayerName = "UI";
    public Vector3 textOffset = new Vector3(0, 1, 0);
    public float textWidth = 2f;  // Width of the text box
    public float textHeight = 1f;  // Height of the text box
    
    private GameObject player;
    private TextMeshPro textDisplay;
    private bool isDisplaying = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        // Create a new GameObject for the text
        GameObject textObj = new GameObject("SignText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = textOffset;
        
        // Add TextMeshPro component
        textDisplay = textObj.AddComponent<TextMeshPro>();
        textDisplay.text = signText;
        textDisplay.alignment = TextAlignmentOptions.Center;
        textDisplay.fontSize = 3;
        textDisplay.enabled = false;

        // Set up text wrapping
        textDisplay.enableWordWrapping = true;
        textDisplay.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);

        // Ensure the text is rendered on top of everything
        textDisplay.sortingOrder = textSortingOrder;

        // Set the sorting layer of the text's renderer
        Renderer textRenderer = textDisplay.GetComponent<Renderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerName = sortingLayerName;
            textRenderer.sortingOrder = textSortingOrder;
        }
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            
            if (distance <= displayDistance && !isDisplaying)
            {
                ShowText();
            }
            else if (distance > displayDistance && isDisplaying)
            {
                HideText();
            }
        }
    }

    void ShowText()
    {
        textDisplay.enabled = true;
        isDisplaying = true;
    }

    void HideText()
    {
        textDisplay.enabled = false;
        isDisplaying = false;
    }
}