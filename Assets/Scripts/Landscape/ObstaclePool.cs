


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObstacleType
{
    public string typeName;       // "Rock", "Tree", "Pond"
    public Transform[] variants;  // Prefab variants
}

public class ObstaclePool : MonoBehaviour
{
    [Header("Obstacle Types")]
    public ObstacleType[] obstacleTypes;

    [Header("Pool Settings")]
    public int poolSizePerType = 10;
    public float minDistanceZ = 35f;   // Minimum distance between obstacles
    public float laneOffsetX = 5f;     // Distance between lanes
    public int lanes = 5;

    [Header("References")]
    private Transform player;

    private List<Transform> pool = new List<Transform>();
    private List<Transform> activeObstacles = new List<Transform>();

    private float lastSpawnZ = 0f;

    private void Start()
    {
        StartCoroutine(SetPlayer());

        // Initialize pool
        foreach (var type in obstacleTypes)
        {
            for (int i = 0; i < poolSizePerType; i++)
            {
                Transform variant = type.variants[Random.Range(0, type.variants.Length)];
                Transform obs = Instantiate(variant, Vector3.one * 9999f, Quaternion.identity);
                obs.gameObject.SetActive(false);
                pool.Add(obs);
            }
        }
    }
    private IEnumerator SetPlayer()
    {
        while (FindAnyObjectByType<PlayerView>() == null)
        {
            yield return null;
        }

        this.player = FindAnyObjectByType<PlayerView>().transform;
    }

    private void Update()
    {
        if (player == null) return;

        // Spawn obstacles ahead
        while (lastSpawnZ < player.position.z + 100f)
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
        // Pick one lane to leave free
        int freeLane = Random.Range(0, lanes);

        for (int lane = 0; lane < lanes; lane++)
        {
            if (lane == freeLane) continue; // skip free lane

            Transform obstacle = GetPooledObstacle();
            if (obstacle == null) return;

            float xPos = (lane - 1) * laneOffsetX; // assuming 3 lanes: -5,0,5
            obstacle.position = new Vector3(xPos, 0f, zPos);
            obstacle.rotation = Quaternion.identity;
            obstacle.gameObject.SetActive(true);
            activeObstacles.Add(obstacle);
        }
    }

    private Transform GetPooledObstacle()
    {
        foreach (var obs in pool)
        {
            if (!obs.gameObject.activeInHierarchy)
                return obs;
        }
        return null; // all in use
    }
}
