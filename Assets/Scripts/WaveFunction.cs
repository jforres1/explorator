//using System;
//using System.Collections;
using System.Collections.Generic;
//using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

/*TODO: Separate the image processing into another class
*Replace iteration generation system with a save state stack
*Create more tilesets
*/
public class WaveFunction : MonoBehaviour
{
    public bool debug;
    public bool backtrack;

    public Vector2Int size;
    public int maximumIterations;
    Tile[] defaultTileSet;
    Cell[,] map;
    PriorityQueue pq;
    Stack<SaveState> saveStack;
    int iterations;

    /*On load, initialize a new array of cells and a priority queue.
    */
    void Start()
    {
        if (debug) Debug.Log("Beginning Generation\n");
        if (backtrack) saveStack = new Stack<SaveState>();
        map = new Cell[size.x, size.y];
        pq = new PriorityQueue();
        iterations = 0;
        TileSetOptions optionSet = gameObject.GetComponent<TileSetOptions>();
        defaultTileSet = optionSet.defaultTileSet;
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
                Cell cell = new Cell(false, defaultTileSet);

                map[i, j] = cell;
                pq.Enqueue(new Vector2Int(i, j), cell.Options.Length);
            }
        }
    }

    /*Remove the first element of the list, which will be the element with the smallest key each time.
    */
    bool CollapseGrid()
    {
        // bool finished = false;
        // bool success = true;
        // while (!finished && success)
        // {
        //     //Get an element that is not collapsed with the lowest entropy
        //     //If there is no such element we can finish
        //     Vector2Int cellLocation = GetLowestEntropy();
        //     if (cellLocation.x == size.x && cellLocation.y == size.y)
        //     {
        //         if (debug) Debug.Log("Generation Succeeded\n");
        //         finished = true;
        //     }
        //     //If generation fails and there is a cell with no possible options left
        //     else if (cellLocation.x == -1 && cellLocation.y == -1)
        //     {
        //         if (debug) Debug.Log("Generation Failed...\n");
        //         success = false;
        //     }
        //     else
        //     {
        //         //Perform operations on lowest entropy element
        //         Cell target = map[cellLocation.x, cellLocation.y];
        //         Tile selection = target.Options[Random.Range(0, target.Options.Length)];
        //         target.Collapsed = true;
        //         target.Options = new Tile[] { selection };
        //         PropagateChange(cellLocation);
        //     }
        // }
        // return success;
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
            else if (target.Options.Length == 1)
            {
                selection = target.Options[0];
            }
            else
            {
                selection = target.Options[Random.Range(0, target.Options.Length)];
            }
            target.Collapsed = true;
            target.Options = new Tile[] { selection };
            PropagateChange(cellLocation);
        }
        return true;
    }

    // Vector2Int GetLowestEntropy()
    // {
    //     List<Vector2Int> possibleSpaces = new List<Vector2Int>();
    //     Cell temp = new Cell(false, defaultTileSet);
    //     int lowest = defaultTileSet.Length;
    //     //iterate through each cell in the map, adding its location to the list of possible spaces if it is one of the lowest entropies.
    //     for (int i = 0; i < size.x; i++)
    //     {
    //         for (int j = 0; j < size.y; j++)
    //         {
    //             temp = map[i, j];
    //             int count = temp.Options.Length;
    //             if (count < lowest && !temp.Collapsed)
    //             {
    //                 //if there is a failure in generation and there are no more options we can stop here
    //                 //Return (-1, -1) if there are no possible values.
    //                 if (count == 0) return new Vector2Int(-1, -1);
    //                 //otherwise
    //                 possibleSpaces.Clear();
    //                 possibleSpaces.Add(new Vector2Int(i, j));
    //                 lowest = count;
    //             }
    //             else if (count == lowest && !temp.Collapsed)
    //             {
    //                 possibleSpaces.Add(new Vector2Int(i, j));
    //             }
    //         }
    //     }
    //     //Return one position from the list of possible options.
    //     //Return a vector out of bounds if every position has been solved.
    //     if (possibleSpaces.Count > 0)
    //     {
    //         return possibleSpaces[Random.Range(0, possibleSpaces.Count)];
    //     }
    //     else
    //     {
    //         return new Vector2Int(size.x, size.y);
    //     }
    // }

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
                foreach (Tile t in target.Options)
                {
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

                    if (flag) newTileSet.Add(t);
                }
                target.Options = newTileSet.ToArray();

                //If the current length is less than the old length, adjust its priority in the priority queue, then enqueue its surrounding nodes
                if (oldCount > target.Options.Length)
                {
                    pq.AdjustPriority(position, target.Options.Length);
                    if (x > 0 && !map[x - 1, y].Collapsed) q.Enqueue(new Vector2Int(x - 1, y));
                    if (x < size.x - 1 && !map[x + 1, y].Collapsed) q.Enqueue(new Vector2Int(x + 1, y));
                    if (y > 0 && !map[x, y - 1].Collapsed) q.Enqueue(new Vector2Int(x, y - 1));
                    if (y < size.y - 1 && !map[x, y + 1].Collapsed) q.Enqueue(new Vector2Int(x, y + 1));
                }
            }
        }
    }
}