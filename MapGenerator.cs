using UnityEngine;
using System.Collections.Generic;
using MapGen;

public class MapGenerator : MonoBehaviour
{
    MapTile[,] tiles;
    bool spawn1, spawn2;

    [SerializeField]
    GameObject player;

    [SerializeField]
    GameObject walkableTiles;

    [SerializeField]
    GameObject nonwalkableTiles;

    [SerializeField]
    GameObject walkableFolder;

    [SerializeField]
    GameObject nonwalkableFolder;

    [SerializeField]
    GameObject walls;

    [SerializeField]
    GameObject wallFolder;

    [SerializeField]
    GameObject start;

    [SerializeField]
    GameObject goal;

    [SerializeField]
    GameObject enemy1;

    [SerializeField]
    GameObject enemy2;

    void Awake()
    {
        generateMap();
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void generateMap()
    {
        int dimx = 30;
        int dimy = 30;


        //PrimGenerator primGen = new PrimGenerator();        //Prim Map Generation
        //tiles = primGen.MapGen(dimx, dimy, 0.6f);    //Comment out Perlin

        PerlinGenerator perlingen = new PerlinGenerator(); //Perlin Map Generation
        tiles = perlingen.MapGen(dimx, dimy, 10.0f); //Comment out Prim


        foreach (MapTile a in tiles)   //Tile Generation
        {
            if (a.Walkable == true)
            {
                if (a.IsStart == true)
                {
                    Instantiate(player, new Vector3(a.X, 1.0f, a.Y), new Quaternion());
                    Instantiate(start, new Vector3(a.X, 0.0f, a.Y), new Quaternion());
                }
                else if (a.IsGoal == true)
                {
                    Instantiate(goal, new Vector3(a.X, 0.0f, a.Y), new Quaternion());
                }
                else
                {
                    Instantiate(walkableTiles, new Vector3(a.X, 0.0f, a.Y), new Quaternion(), walkableFolder.transform);
                }
            }
            else if (a.Walkable == false)
            {
                Instantiate(nonwalkableTiles, new Vector3(a.X, 0.0f, a.Y), new Quaternion(), nonwalkableFolder.transform);
            }
        }

        for (int i = -1; i < dimx + 1; i++) //Wall Generation
        {
            Instantiate(walls, new Vector3(i, 0.0f, -1), new Quaternion(), wallFolder.transform);
            Instantiate(walls, new Vector3(i, 0.0f, dimy), new Quaternion(), wallFolder.transform);
        }
        for (int i = 0; i < dimy + 1; i++)
        {
            Instantiate(walls, new Vector3(-1, 0.0f, i), new Quaternion(), wallFolder.transform);
            Instantiate(walls, new Vector3(dimx, 0.0f, i), new Quaternion(), wallFolder.transform);
        }

        while (spawn1 != true)
        {
            int randomX = Random.Range(0, dimx);
            int randomY = Random.Range(0, dimy);
            if (tiles[randomX, randomY].Walkable == true && tiles[randomX,randomY].IsStart!= true && tiles[randomX, randomY].IsGoal != true )
            {
                Instantiate(enemy1, new Vector3(randomX, 1.0f, randomY), new Quaternion());
                spawn1 = true;
            }
        }

        while (spawn2 != true)
        {
            int randomX = Random.Range(0, dimx );
            int randomY = Random.Range(0, dimy );
            if (tiles[randomX, randomY].Walkable == true && tiles[randomX, randomY].IsStart != true && tiles[randomX, randomY].IsGoal != true)
            {
                Instantiate(enemy2, new Vector3(randomX, 1.0f, randomY), new Quaternion());
                spawn2 = true;
            }
        }
    }

    public MapTile[,] getMap()
    {
        return tiles;
    }
}
