using UnityEngine;
using TMPro;
using System.Collections;
using TreeController;

public class CompanionBehavior : MonoBehaviour
{
    public Transform player;
    public float radius = 2.0f;
    public float speed = 2.0f;
    public float floatRange = 0.2f;
    public float floatSpeed = 1.0f;
    public float followSpeed = 3.0f;

    public float floatHeight = 0.02f;
    public float floatDuration = 0.5f;
    public float rotationDuration = 1f;
    public float attachDuration = 1f;

    [Header("Dialogue")]
    [TextArea(2, 5)]
    public string dialogueMessage = "Hello!\nI'm your new companion.";
    public float dialogueDuration = 3f;
    public Vector3 textOffset = new Vector3(0, 0.5f, 0);
    public float textWidth = 2f;
    public float textHeight = 1f;
    public int textSortingOrder = 100;
    public string sortingLayerName = "UI";

    [Header("Root Growth")]
    public RootGrowthMechanic rootGrowthMechanic;
    public float rootGrowthCooldown = 5f;
    public float rootGrowthMaxDistance = 5f;

    private float angle;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer playerSpriteRenderer;
    private TextMeshPro dialogueText;
    private Vector3 initialPosition;
    private float lastRootGrowthTime;

    private enum CompanionState
    {
        OnGround,
        Floating,
        Standing,
        Attaching,
        Following
    }

    private CompanionState currentState = CompanionState.OnGround;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerSpriteRenderer = player.Find("Visual/Effects/Sprite").GetComponent<SpriteRenderer>();
        initialPosition = transform.position;

        if (rootGrowthMechanic == null) Debug.LogError("Root Growth Mechanic is not assigned");

        CreateDialogueText();
        lastRootGrowthTime = -rootGrowthCooldown; // Allow immediate root growth at start
    }

    void Update()
    {
        switch (currentState)
        {
            case CompanionState.OnGround:
            case CompanionState.Floating:
            case CompanionState.Standing:
                // These states are handled by coroutines
                break;
            case CompanionState.Attaching:
                // Handled by AttachToPlayer coroutine
                break;
            case CompanionState.Following:
                FollowPlayer();
                HandleRootGrowth();
                break;
        }
    }

    void LateUpdate()
    {
        if (dialogueText != null && dialogueText.enabled)
        {
            dialogueText.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }
    }

    private void CreateDialogueText()
    {
        GameObject textObj = new GameObject("DialogueText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = textOffset;
        
        dialogueText = textObj.AddComponent<TextMeshPro>();
        dialogueText.text = dialogueMessage;
        dialogueText.alignment = TextAlignmentOptions.Center;
        dialogueText.fontSize = 3;
        dialogueText.enabled = false;
        
        dialogueText.enableWordWrapping = true;
        dialogueText.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
        
        dialogueText.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        
        dialogueText.sortingOrder = textSortingOrder;
        
        Renderer textRenderer = dialogueText.GetComponent<Renderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingLayerName = sortingLayerName;
            textRenderer.sortingOrder = textSortingOrder;
        }
    }

    public void StartCutscene()
    {
        StartCoroutine(CutsceneSequence());
    }

    private IEnumerator CutsceneSequence()
    {
        yield return StartCoroutine(StartFloating());
        yield return StartCoroutine(StandUp());
        FlipToFacePlayer();
        yield return StartCoroutine(ShowDialogue());
        yield return StartCoroutine(AttachToPlayer());
        currentState = CompanionState.Following;
    }

    private IEnumerator StartFloating()
    {
        currentState = CompanionState.Floating;
        Vector3 startPos = initialPosition;
        Vector3 endPos = startPos + Vector3.up * floatHeight;
        float elapsedTime = 0f;

        while (elapsedTime < floatDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / floatDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
    }

    private IEnumerator StandUp()
    {
        currentState = CompanionState.Standing;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.identity;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
    }

    private void FlipToFacePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        spriteRenderer.flipX = directionToPlayer.x < 0;
    }

    private IEnumerator ShowDialogue()
    {
        dialogueText.enabled = true;

        yield return new WaitForSeconds(dialogueDuration);

        dialogueText.enabled = false;
    }

    private IEnumerator AttachToPlayer()
    {
        currentState = CompanionState.Attaching;
        Vector3 startPos = transform.position;
        Vector3 endPos = player.position + Vector3.up * radius;
        float elapsedTime = 0f;

        while (elapsedTime < attachDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsedTime / attachDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.SetParent(player);
        transform.localPosition = new Vector3(0, radius, 0);
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    private void FollowPlayer()
    {
        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float y = radius + Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.localPosition = new Vector3(x, y, 0);

        int playerOrderInLayer = playerSpriteRenderer.sortingOrder;
        if (Mathf.Sin(angle) > 0)
        {
            spriteRenderer.sortingOrder = playerOrderInLayer + 1;
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.sortingOrder = playerOrderInLayer - 1;
            spriteRenderer.flipX = false;
        }
    }

    private void HandleRootGrowth()
    {
        if ((Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(0)) && Time.time >= lastRootGrowthTime + rootGrowthCooldown)
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 companionPosition = transform.position;
            float distanceToMouse = Vector2.Distance(mousePosition, companionPosition);

            if (distanceToMouse <= rootGrowthMaxDistance)
            {
                if (rootGrowthMechanic.StartGrowth(mousePosition))
                {
                    lastRootGrowthTime = Time.time;
                }
            }
        }
        else if (Input.GetKeyUp(KeyCode.R) || Input.GetMouseButtonUp(0))
        {
            rootGrowthMechanic.StopGrowth();
        }
    }
}