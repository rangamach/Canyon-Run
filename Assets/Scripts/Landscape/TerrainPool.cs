using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPool : MonoBehaviour
{
    [Header("Terrain Prefabs")]
    [SerializeField] private Transform[] terrainPrefabs;

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 20;      // Make sure this is enough for active + preload tiles
    [SerializeField] private float tileLength = 395f;
    [SerializeField] private int preloadTiles = 10;

    [Header("References")]
    private Transform player;

    private Queue<Transform> pool = new Queue<Transform>();
    private List<Transform> activeTiles = new List<Transform>();

    private float nextSpawnZ = 0f;

    private void Start()
    {
        StartCoroutine(SetPlayer());

        // Create pool
        for (int i = 0; i < poolSize; i++)
        {
            Transform prefab = terrainPrefabs[Random.Range(0, terrainPrefabs.Length)];
            Transform tile = Instantiate(prefab, Vector3.one * 9999f, Quaternion.identity);
            tile.gameObject.SetActive(false);
            pool.Enqueue(tile);
        }

        // Preload initial tiles
        for (int i = 0; i < preloadTiles; i++)
            SpawnTile();
    }

    private IEnumerator SetPlayer()
    {
        while (FindAnyObjectByType<PlayerView>() == null)
            yield return null;

        player = FindAnyObjectByType<PlayerView>().transform;
    }

    private void Update()
    {
        if (player == null) return;

        // Spawn tile ahead if player approaches the last one
        if (activeTiles.Count == 0 || player.position.z > activeTiles[activeTiles.Count - 1].position.z - tileLength)
        {
            SpawnTile();
        }
        // Disable tiles behind player
        DisableOldTiles();
    }

    private void SpawnTile()
    {
        if (pool.Count == 0) return;

        Transform tile = pool.Dequeue();

        tile.position = new Vector3(0f, 0f, nextSpawnZ);
        tile.gameObject.SetActive(true);

        activeTiles.Add(tile);
        nextSpawnZ += tileLength;
    }

    private void DisableOldTiles()
    {
        for (int i = activeTiles.Count - 1; i >= 0; i--)
        {
            Transform tile = activeTiles[i];
            if (player.position.z - tile.position.z > tileLength * 2f)
            {
                tile.gameObject.SetActive(false);
                pool.Enqueue(tile);
                activeTiles.RemoveAt(i);
            }
        }
    }
}