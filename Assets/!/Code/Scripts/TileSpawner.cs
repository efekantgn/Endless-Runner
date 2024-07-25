using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class TileSpawner : MonoBehaviour
{

    [SerializeField] private int tileStartCount = 10;
    [SerializeField] private int tileSafeCount = 5;
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

        Random.InitState(System.DateTime.Now.Millisecond);
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
            currentTileLocation += Vector3.Scale(prevTile.GetComponentInChildren<Renderer>().bounds.size - new Vector3(.4f, 0, .4f)/*Modeldeki Yeşil Zemin Fazlalığı*/, currentTileDirection);
    }

    private void SpawnObstacle()
    {
        if (Random.value > .3f) return;

        GameObject obstaclePrefab = SelectRandomGameObjectFromList(obstacles);
        Quaternion newObstacleRotation = obstaclePrefab.gameObject.transform.rotation * Quaternion.LookRotation(currentTileDirection, Vector3.up);

        GameObject obstacle = Instantiate(obstaclePrefab, currentTileLocation, newObstacleRotation);
        currentObstacles.Add(obstacle);

    }

    public void AddNewDirection(Vector3 direction)
    {
        currentTileDirection = direction;
        DeletePreviusTile();
        currentTileLocation += Vector3.Scale(prevTile.GetComponentInChildren<Renderer>().bounds.size - new Vector3(.4f, 0, .4f)/*Modeldeki Yeşil Zemin Fazlalığı*/, currentTileDirection);

        for (int i = 0; i < tileSafeCount; i++)
        {
            SpawnTile(startingTile.GetComponent<Tile>(), false);
        }

        int currentPathLenght = Random.Range(minStraightTiles, maxStrightTiles);
        for (int i = 0; i < currentPathLenght; i++)
        {
            SpawnTile(startingTile.GetComponent<Tile>(), (i % 3 == 0) ? true : false);
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

        return list[Random.Range(0, list.Count)];
    }
}
