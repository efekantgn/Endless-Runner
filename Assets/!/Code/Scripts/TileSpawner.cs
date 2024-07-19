using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileSpawner : MonoBehaviour
{

    [SerializeField] private int tileStartCount = 10;
    [SerializeField] private int minStraightTiles = 3;
    [SerializeField] private int maxStrightTiles = 15;


    [SerializeField] private GameObject startingTile;
    [SerializeField] private List<GameObject> turnTiles;
    [SerializeField] private List<GameObject> obstacles;

    private Vector3 currentTileLocation = Vector3.zero;
    private Vector3 currentTileDirection = Vector3.forward;
    private GameObject prevTile = null;

    private List<GameObject> currentTiles;
    private List<GameObject> currentObstacles;

    private void Start()
    {
        currentTiles = new List<GameObject>();
        currentObstacles = new List<GameObject>();

        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);
        for (int i = 0; i < tileStartCount; i++)
        {
            SpawnTile(startingTile.GetComponent<Tile>());
        }

        SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>());
    }

    private void SpawnTile(Tile tile, bool spawnObstacle = false)
    {
        Quaternion newTileRotation = tile.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

        prevTile = Instantiate(tile.gameObject, currentTileLocation, newTileRotation);
        currentTiles.Add(prevTile);

        if (spawnObstacle) SpawnObstacle();

        if (tile.type == Enums.TileType.STRAIGHT)
            currentTileLocation += Vector3.Scale(prevTile.GetComponentInChildren<Renderer>().bounds.size, currentTileDirection);
    }

    private void SpawnObstacle()
    {
        if (Random.value > .2f) return;

        GameObject obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
        Quaternion newObstacleRotation = obstaclePrefab.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

        GameObject obstacle = Instantiate(obstaclePrefab, currentTileLocation, newObstacleRotation);
        currentObstacles.Add(obstacle);

    }

    public void AddNewDirection(Vector3 direction)
    {
        currentTileDirection = direction;
        DeletePreviusTile();

        Vector3 tilePlacementScale;
        if (prevTile.GetComponent<Tile>().type == Enums.TileType.SIDEWAYS)
        {
            tilePlacementScale = Vector3.Scale(prevTile.GetComponentInChildren<Renderer>().bounds.size / 2 + (Vector3.one * startingTile.GetComponentInChildren<BoxCollider>().bounds.size.z / 2), currentTileDirection);
        }
        else
        {
            tilePlacementScale = Vector3.Scale(prevTile.GetComponentInChildren<Renderer>().bounds.size + (Vector3.one * 7.5f) + (Vector3.one * startingTile.GetComponentInChildren<BoxCollider>().size.z / 2), currentTileDirection);

        }
        currentTileLocation += tilePlacementScale;
        int currentPathLenght = UnityEngine.Random.Range(minStraightTiles, maxStrightTiles);
        for (int i = 0; i < currentPathLenght; i++)
        {
            SpawnTile(startingTile.GetComponent<Tile>(), (i == 0) ? false : true);
        }
        SpawnTile(SelectRandomGameObjectFromList(turnTiles).GetComponent<Tile>(), false);
    }

    private void DeletePreviusTile()
    {
        //TODO:ObjectPool
        while (currentTiles.Count != 1)
        {
            GameObject tile = currentTiles[0];
            currentTiles.RemoveAt(0);
            Destroy(tile);
        }
        //TODO:ObjectPool
        while (currentObstacles.Count != 0)
        {
            GameObject obstacle = currentObstacles[0];
            currentObstacles.RemoveAt(0);
            Destroy(obstacle);
        }
    }

    private GameObject SelectRandomGameObjectFromList(List<GameObject> list)
    {
        if (list.Count == 0) return null;

        return list[UnityEngine.Random.Range(0, list.Count)];
    }
}
