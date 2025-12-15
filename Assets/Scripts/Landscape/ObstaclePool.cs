using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObstacleType
{
    public string typeName;          // "Rock", "Tree", "Pond"
    public Transform[] variants;     // All variants of this type
    public float weight = 1f;        // Spawn probability (higher = more common)
}

public class ObstaclePool : MonoBehaviour
{
    [Header("Obstacle Types (Set Weights Here)")]
    public ObstacleType[] obstacleTypes;
    // EXAMPLE:
    // Rock weight = 5  (most common)
    // Tree weight = 3  (common)
    // Pond weight = 1  (rare)

    [Header("Pool Settings")]
    public int poolSizePerType = 10;
    public float minDistanceZ = 35f;
    public float laneOffsetX = 5f;
    public int lanes = 5;

    private Transform player;

    // Pool per obstacle TYPE
    private List<List<Transform>> poolsByType = new List<List<Transform>>();

    private List<Transform> activeObstacles = new List<Transform>();
    private float lastSpawnZ = 0f;

    private void Start()
    {
        StartCoroutine(SetPlayer());
        InitializePool();
    }

    private IEnumerator SetPlayer()
    {
        while (FindAnyObjectByType<PlayerView>() == null)
            yield return null;

        player = FindAnyObjectByType<PlayerView>().transform;
    }

    private void InitializePool()
    {
        poolsByType.Clear();

        foreach (var type in obstacleTypes)
        {
            List<Transform> typePool = new List<Transform>();

            for (int i = 0; i < poolSizePerType; i++)
            {
                Transform prefab = type.variants[Random.Range(0, type.variants.Length)];
                Transform obj = Instantiate(prefab, Vector3.one * 9999f, Quaternion.identity);
                obj.gameObject.SetActive(false);
                typePool.Add(obj);
            }

            poolsByType.Add(typePool);
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Keep spawning ahead
        while (lastSpawnZ < player.position.z + 1000f)
        {
            SpawnObstacleRow(lastSpawnZ + minDistanceZ);
            lastSpawnZ += minDistanceZ;
        }

        // Disable obstacles behind player
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (player.position.z - activeObstacles[i].position.z > 20f)
            {
                activeObstacles[i].gameObject.SetActive(false);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    private void SpawnObstacleRow(float zPos)
    {
        int freeSlot = Random.Range(0, lanes);

        List<Vector3> usedPositions = new List<Vector3>();

        for (int i = 0; i < lanes; i++)
        {
            if (i == freeSlot)
                continue;

            int typeIndex = GetWeightedRandomType();
            Transform obstacle = GetPooledObstacle(typeIndex);
            if (obstacle == null)
                continue;

            float xPos = 0f;
            bool valid = false;
            int attempts = 0;

            while (!valid && attempts < 15)
            {
                attempts++;

                // Random X position
                xPos = Random.Range(-20f, 20f);

                // Calculate obstacle width using its child colliders
                float width = GetObstacleWidth(obstacle);

                valid = true;

                // Check against previously placed obstacles in this row
                foreach (var pos in usedPositions)
                {
                    float otherWidth = pos.z; // stored as width in z field

                    if (Mathf.Abs(xPos - pos.x) < (width + otherWidth) * 0.6f)
                    {
                        valid = false;
                        break;
                    }
                }
            }

            if (!valid)
                continue;

            // Store X AND width (in pos.z field)
            usedPositions.Add(new Vector3(xPos, 0, GetObstacleWidth(obstacle)));

            obstacle.position = new Vector3(xPos, 0f, zPos);
            obstacle.rotation = Quaternion.identity;
            obstacle.gameObject.SetActive(true);

            activeObstacles.Add(obstacle);
        }
    }
    private float GetObstacleWidth(Transform obstacle)
    {
        Collider[] colliders = obstacle.GetComponentsInChildren<Collider>();

        if (colliders.Length == 0)
            return 1f; // fallback small width

        Bounds bounds = colliders[0].bounds;

        foreach (var col in colliders)
            bounds.Encapsulate(col.bounds);

        return bounds.size.x / 2f;  // half width ? radius for spacing
    }

    // Weighted random: higher weight = more chance
    private int GetWeightedRandomType()
    {
        float total = 0f;
        foreach (var type in obstacleTypes)
            total += type.weight;

        float rand = Random.value * total;

        for (int i = 0; i < obstacleTypes.Length; i++)
        {
            if (rand < obstacleTypes[i].weight)
                return i;

            rand -= obstacleTypes[i].weight;
        }

        return obstacleTypes.Length - 1;
    }

    private Transform GetPooledObstacle(int typeIndex)
    {
        foreach (var obj in poolsByType[typeIndex])
        {
            if (!obj.gameObject.activeSelf)
                return obj;
        }

        return null; // Pool empty (rare)
    }
    public void ResetObstacles()
    {
        // Disable all active obstacles
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            Transform obj = activeObstacles[i];
            obj.gameObject.SetActive(false);
        }

        activeObstacles.Clear();

        // Reset spawn pointer
        lastSpawnZ = player != null ? player.position.z : 0f;
    }
}