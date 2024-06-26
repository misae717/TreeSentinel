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
    public float followSpeed = 3.0f; // Speed at which the companion follows the player

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
        // Update the position of the companion relative to the player
        Vector3 targetPosition = player.position;

        // Calculate the circular motion around the player
        angle += speed * Time.deltaTime;
        float x = Mathf.Cos(angle) * radius;
        float y = originalY + Mathf.Sin(Time.time * floatSpeed) * floatRange;
        Vector3 offset = new Vector3(x, y, 0);

        // Smoothly follow the player with some lag
        transform.position = Vector3.Lerp(transform.position, targetPosition + offset, followSpeed * Time.deltaTime);

        // Get the player's order in layer
        int playerOrderInLayer = playerSpriteRenderer.sortingOrder;

        // Swap the order in layer based on the angle
        if (Mathf.Sin(angle) > 0)
        {
            spriteRenderer.sortingOrder = playerOrderInLayer + 1; // In front of the player
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.sortingOrder = playerOrderInLayer - 1; // Behind the player
            spriteRenderer.flipX = false;
        }
    }
}


