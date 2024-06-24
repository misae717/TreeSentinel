using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionBehavior : MonoBehaviour
{
    public Transform player; // Reference to the player's transform
    public float radius = 2.0f; // Radius of the circular path
    public float speed = 2.0f; // Speed of the rotation
    public float floatRange = 0.2f; // Range of the Y-axis float movement
    public float floatSpeed = 1.0f; // Speed of the Y-axis float movement

    private float angle;
    private float originalY;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer playerSpriteRenderer;

    void Start()
    {
        // Store the original Y position
        originalY = transform.localPosition.y;
        // Get the SpriteRenderer component of the companion
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Get the SpriteRenderer component of the player's Sprite GameObject
        playerSpriteRenderer = player.Find("Visual/Effects/Sprite").GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Calculate the position of the companion
        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float y = originalY + Mathf.Sin(Time.time * floatSpeed) * floatRange;
        transform.localPosition = new Vector3(x, y, 0);

        // Get the player's order in layer
        int playerOrderInLayer = playerSpriteRenderer.sortingOrder;

        // Swap the order in layer based on the angle
        if (Mathf.Sin(angle) > 0)
        {
            spriteRenderer.sortingOrder = playerOrderInLayer + 1; // In front of the player
        }
        else
        {
            spriteRenderer.sortingOrder = playerOrderInLayer - 1; // Behind the player
        }
    }
}

