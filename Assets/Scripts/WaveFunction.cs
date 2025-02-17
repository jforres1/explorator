using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/*TODO:
*Create more tilesets
*/
public class WaveFunction : MonoBehaviour
{
    public bool debug;
    public Vector2Int size;
    public int maximumIterations;
    Tile[] defaultTileSet;
    float[][] weightTable = new float[2][];
    Cell[,] map;
    int[,] weightMap;
    PriorityQueue pq;
    int iterations;

    /*On load, initialize a new array of cells and a priority queue.
    */
    void Start()
    {
        if (debug) Debug.Log("Beginning Generation\n");
        map = new Cell[size.x, size.y];
        pq = new PriorityQueue();
        iterations = 0;
        TileSetOptions optionSet = gameObject.GetComponent<TileSetOptions>();
        defaultTileSet = optionSet.defaultTileSet;
        weightTable[0] = optionSet.weightSet1;
        weightTable[1] = optionSet.weightSet2;
        WFCImageProcessor ip = gameObject.GetComponent<WFCImageProcessor>();
        weightMap = ip.SampleImage();
        Generate();
    }
    void Generate()
    {
        InitializeGrid();
        //CollapseGrid will return false if the generation fails, so we can restart generation.
        if (!CollapseGrid())
        {
            iterations++;
            if (debug) Debug.Log("Beginning Iteration " + iterations + "\n");
            if (iterations < maximumIterations)
            {
                pq.Clear();
                Generate();
            }
            else
            {
                if (debug) Debug.Log("Maximum Number of Iterations Reached \n");
            }
        }
        else
        {
            InstantiateGrid();
        }
    }
    void InstantiateGrid()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Instantiate(map[i, j].Options[0], new Vector2(i, j), Quaternion.identity);
            }
        }
    }
    /*To initialize the grid, iterate through the grid length and height.
    *Create a new cell for each i, j entry, and add each cell to the grid and list.
    */
    void InitializeGrid()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {

                if (weightMap[i, j] == 0)
                {
                    Cell cell = new Cell(false, defaultTileSet);
                    map[i, j] = cell;
                    pq.Enqueue(new Vector2Int(i, j), CalculateEntropy(cell.SoftMax()));
                }
                else
                {
                    Cell cell = new Cell(false, defaultTileSet, weightTable[2 - weightMap[i, j]]);
                    map[i, j] = cell;
                    pq.Enqueue(new Vector2Int(i, j), CalculateEntropy(cell.SoftMax()));
                }


            }
        }
    }

    /*Remove the first element of the list, which will be the element with the smallest key each time.
    */
    bool CollapseGrid()
    {
        Vector2Int cellLocation;
        while (pq.Count > 0)
        {
            cellLocation = pq.Dequeue();
            Cell target = map[cellLocation.x, cellLocation.y];
            Tile selection;
            if (target.Options.Length == 0)
            {
                return false;
            }
            else
            {
                selection = target.GetRandomTile();
                target.Collapsed = true;
                target.Options = new Tile[] { selection };
                target.Weights = new float[] { 1.0f };
                PropagateChange(cellLocation);
            }
        }
        return true;
    }

    void PropagateChange(Vector2Int location)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        //Enqueue the surrounding nodes on the first pass
        int x = location.x;
        int y = location.y;
        if (x > 0 && !map[x - 1, y].Collapsed) q.Enqueue(new Vector2Int(x - 1, y));
        if (x < size.x - 1 && !map[x + 1, y].Collapsed) q.Enqueue(new Vector2Int(x + 1, y));
        if (y > 0 && !map[x, y - 1].Collapsed) q.Enqueue(new Vector2Int(x, y - 1));
        if (y < size.y - 1 && !map[x, y + 1].Collapsed) q.Enqueue(new Vector2Int(x, y + 1));

        while (q.Count != 0)
        {
            Vector2Int position = q.Dequeue();
            x = position.x;
            y = position.y;
            Cell target = map[x, y];
            if (!target.Collapsed)
            {
                //Get the initial length of the cell's option list
                int oldCount = target.Options.Length;
                //Adjust the target's options list
                List<Tile> newTileSet = new List<Tile>();
                List<float> newWeightSet = new List<float>();
                for (int i = 0; i < target.Options.Length; i++)
                {
                    Tile t = target.Options[i];
                    bool flag = true;
                    Tile[] options;

                    //If a tile does not match any of the options of its neighbors, remove it from the list.
                    //Ignore the cells on the boarders
                    if (x > 0 && flag)
                    {
                        flag = false;
                        options = map[x - 1, y].Options;
                        foreach (Tile o in options)
                        {
                            if (o.eastConnectionType.Equals(t.westConnectionType)) flag = true;
                        }
                    }
                    if (x < size.x - 1 && flag)
                    {
                        flag = false;
                        options = map[x + 1, y].Options;
                        foreach (Tile o in options)
                        {
                            if (o.westConnectionType.Equals(t.eastConnectionType)) flag = true;
                        }
                    }
                    if (y > 0 && flag)
                    {
                        flag = false;
                        options = map[x, y - 1].Options;
                        foreach (Tile o in options)
                        {
                            if (o.northConnectionType.Equals(t.southConnectionType)) flag = true;
                        }
                    }
                    string northType = t.northConnectionType;
                    if (y < size.y - 1 && flag)
                    {
                        flag = false;
                        options = map[x, y + 1].Options;
                        foreach (Tile o in options)
                        {
                            if (o.southConnectionType.Equals(t.northConnectionType)) flag = true;
                        }
                    }

                    if (flag)
                    {
                        newTileSet.Add(t);
                        newWeightSet.Add(target.Weights[i]);
                    }
                }
                target.Options = newTileSet.ToArray();
                target.Weights = newWeightSet.ToArray();

                //If the current length is less than the old length, adjust its priority in the priority queue, then enqueue its surrounding nodes
                if (oldCount > target.Options.Length)
                {
                    pq.AdjustPriority(position, CalculateEntropy(target.SoftMax()));
                    if (x > 0 && !map[x - 1, y].Collapsed) q.Enqueue(new Vector2Int(x - 1, y));
                    if (x < size.x - 1 && !map[x + 1, y].Collapsed) q.Enqueue(new Vector2Int(x + 1, y));
                    if (y > 0 && !map[x, y - 1].Collapsed) q.Enqueue(new Vector2Int(x, y - 1));
                    if (y < size.y - 1 && !map[x, y + 1].Collapsed) q.Enqueue(new Vector2Int(x, y + 1));
                }
            }
        }
    }
    float CalculateEntropy(float[] weights)
    {
        float entropy = 0;
        foreach (float w in weights)
        {
            entropy -= (w * (float)Math.Log(w));
        }
        return entropy;
    }
}