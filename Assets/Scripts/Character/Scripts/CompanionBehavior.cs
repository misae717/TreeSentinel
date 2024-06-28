using UnityEngine;
using System.Collections;

public class CompanionBehavior : MonoBehaviour
{
    public Transform player;
    public float radius = 2.0f;
    public float speed = 2.0f;
    public float floatRange = 0.2f;
    public float floatSpeed = 1.0f;
    public float followSpeed = 3.0f;
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;

    public float floatHeight = 1.5f; // Height to float during cutscene
    public float floatDuration = 2f; // Duration of floating animation
    public float attachDuration = 1f; // Duration of attaching animation

    private float angle;
    private float originalY;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer playerSpriteRenderer;
    private float nextFireTime;

    private enum CompanionState
    {
        OnGround,
        Floating,
        Attaching,
        Following
    }

    private CompanionState currentState = CompanionState.OnGround;

    void Start()
    {
        originalY = transform.localPosition.y;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerSpriteRenderer = player.Find("Visual/Effects/Sprite").GetComponent<SpriteRenderer>();

        if (fireballPrefab == null) Debug.LogError("Fireball Prefab is not assigned");
        if (firePoint == null) Debug.LogError("Fire Point is not assigned");
    }

    void Update()
    {
        switch (currentState)
        {
            case CompanionState.OnGround:
                // Do nothing, waiting for cutscene to start
                break;
            case CompanionState.Floating:
                // Handled by StartFloating coroutine
                break;
            case CompanionState.Attaching:
                // Handled by AttachToPlayer coroutine
                break;
            case CompanionState.Following:
                FollowPlayer();
                HandleFiring();
                break;
        }
    }

    public void StartCutscene()
    {
        StartCoroutine(CutsceneSequence());
    }

    private IEnumerator CutsceneSequence()
    {
        yield return StartCoroutine(StartFloating());
        // Here you would trigger dialogue or other cutscene elements
        yield return new WaitForSeconds(2f); // Wait for dialogue
        yield return StartCoroutine(AttachToPlayer());
        currentState = CompanionState.Following;
    }

    private IEnumerator StartFloating()
    {
        currentState = CompanionState.Floating;
        Vector3 startPos = transform.position;
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

        transform.position = endPos;
    }

    private void FollowPlayer()
    {
        Vector3 targetPosition = player.position;
        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float y = originalY + Mathf.Sin(Time.time * floatSpeed) * floatRange;
        Vector3 offset = new Vector3(x, y, 0);

        transform.position = Vector3.Lerp(transform.position, targetPosition + offset, followSpeed * Time.deltaTime);

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

    private void HandleFiring()
    {
        if (Input.GetMouseButton(0) && Time.time > nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Fire()
    {
        if (fireballPrefab == null || firePoint == null)
        {
            Debug.LogError("Fireball Prefab or Fire Point is not assigned");
            return;
        }

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePosition - firePoint.position).normalized;

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        fireball.GetComponent<FireballBehavior>().SetDirection(direction);
    }
}