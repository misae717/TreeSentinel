using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RootGrowthMechanic : MonoBehaviour
{
    public float growthSpeed = 5f;
    public float maxLength = 10f;
    public float helixRadius = 0.5f;
    public float helixFrequency = 2f;
    public int pointsPerUnit = 5;
    public float colliderRadius = 0.05f;
    public LayerMask obstacleLayer;
    public Gradient rootColorGradient;
    public float raycastDistance = 0.5f;
    public int rootSortingOrder = 10;
    public string rootSortingLayerName = "Default";

    private LineRenderer[] lineRenderers;
    private List<Vector3>[] pointsArrays;
    private bool isGrowing = false;
    private float currentLength = 0f;
    private List<GameObject> colliders = new List<GameObject>();

    private void Awake()
    {
        InitializeLineRenderers();
        pointsArrays = new List<Vector3>[2] { new List<Vector3>(), new List<Vector3>() };
        SetupRootColorGradient();
    }

    private void InitializeLineRenderers()
    {
        lineRenderers = new LineRenderer[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject lineObj = new GameObject($"LineRenderer_{i}");
            lineObj.transform.SetParent(transform);
            lineRenderers[i] = lineObj.AddComponent<LineRenderer>();
            lineRenderers[i].startWidth = 0.1f;
            lineRenderers[i].endWidth = 0.05f;
            lineRenderers[i].material = new Material(Shader.Find("Sprites/Default"));
            lineRenderers[i].positionCount = 0;
            
            // Set sorting order and layer
            lineRenderers[i].sortingOrder = rootSortingOrder;
            lineRenderers[i].sortingLayerName = rootSortingLayerName;
        }
    }

    private void SetupRootColorGradient()
    {
        if (rootColorGradient == null)
        {
            rootColorGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[3];
            colorKeys[0] = new GradientColorKey(new Color(0.545f, 0.27f, 0.075f), 0.0f);
            colorKeys[1] = new GradientColorKey(new Color(0.333f, 0.420f, 0.184f), 0.5f);
            colorKeys[2] = new GradientColorKey(new Color(0.420f, 0.557f, 0.137f), 1.0f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);

            rootColorGradient.SetKeys(colorKeys, alphaKeys);
        }
    }

    public bool StartGrowth(Vector2 startPosition)
    {
        if (!isGrowing && IsValidStartPosition(startPosition))
        {
            transform.position = startPosition;
            pointsArrays[0].Clear();
            pointsArrays[1].Clear();
            pointsArrays[0].Add(Vector3.zero);
            pointsArrays[1].Add(Vector3.zero);
            currentLength = 0f;
            isGrowing = true;
            StartCoroutine(GrowRoots());
            return true;
        }
        return false;
    }

    private bool IsValidStartPosition(Vector2 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, raycastDistance, obstacleLayer);
        return hit.collider != null;
    }

    private IEnumerator GrowRoots()
    {
        while (isGrowing && currentLength < maxLength)
        {
            currentLength += growthSpeed * Time.deltaTime;
            UpdateRootShape();
            yield return null;
        }
        isGrowing = false;
    }

    private void UpdateRootShape()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        Vector3 growthDirection = (mousePosition - transform.position).normalized;

        int totalPoints = Mathf.Max(2, Mathf.CeilToInt(currentLength * pointsPerUnit));

        for (int i = 0; i < 2; i++)
        {
            pointsArrays[i].Clear();
            for (int j = 0; j < totalPoints; j++)
            {
                float t = j / (float)(totalPoints - 1);
                float angle = t * currentLength * helixFrequency + (i * Mathf.PI); // Offset second helix by PI
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * helixRadius,
                    Mathf.Sin(angle) * helixRadius,
                    0
                );
                Vector3 position = transform.position + growthDirection * (t * currentLength) + offset;
                pointsArrays[i].Add(position);
            }

            lineRenderers[i].positionCount = totalPoints;
            lineRenderers[i].SetPositions(pointsArrays[i].ToArray());
            lineRenderers[i].colorGradient = rootColorGradient;
        }

        UpdateColliders();
    }

    private void UpdateColliders()
    {
        int desiredColliderCount = Mathf.Max(1, Mathf.CeilToInt(currentLength / 0.5f));

        // Remove excess colliders
        while (colliders.Count > desiredColliderCount)
        {
            Destroy(colliders[colliders.Count - 1]);
            colliders.RemoveAt(colliders.Count - 1);
        }

        // Add or update colliders
        for (int i = 0; i < desiredColliderCount; i++)
        {
            if (i >= colliders.Count)
            {
                GameObject colliderObj = new GameObject($"RootCollider_{i}");
                colliderObj.transform.SetParent(transform);
                CircleCollider2D circleCollider = colliderObj.AddComponent<CircleCollider2D>();
                circleCollider.isTrigger = true;
                colliders.Add(colliderObj);
            }

            Vector3 position;
            if (pointsArrays[0].Count > 1)
            {
                float t = i / (float)(desiredColliderCount - 1);
                position = Vector3.Lerp(pointsArrays[0][0], pointsArrays[0][pointsArrays[0].Count - 1], t);
            }
            else
            {
                position = transform.position; // Default to the root's origin if not enough points
            }

            if (!float.IsNaN(position.x) && !float.IsNaN(position.y) && !float.IsNaN(position.z))
            {
                colliders[i].transform.position = position;
                colliders[i].GetComponent<CircleCollider2D>().radius = colliderRadius;
            }
            else
            {
                Debug.LogWarning($"Invalid position calculated for collider {i}: {position}");
            }
        }
    }

    public void StopGrowth()
    {
        isGrowing = false;
    }

    public void DestroyRoot()
    {
        foreach (var collider in colliders)
        {
            Destroy(collider);
        }
        colliders.Clear();
        foreach (var points in pointsArrays)
        {
            points.Clear();
        }
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.positionCount = 0;
        }
    }
}