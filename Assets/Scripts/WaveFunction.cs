//using System;
//using System.Collections;
using System.Collections.Generic;
//using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveFunction : MonoBehaviour
{
    public bool debug;
    public Vector2Int size;
    public Tile[] tileSetDefault;
    public int maximumIterations;
    public bool useSampleImage;
    public Texture2D sampleImage;
    public Tile[] tileSetType0;
    public Tile[] tileSetType1;
    public Tile[] tileSetType2;
    public Tile[] tileSetType3;
    Cell[,] map;
    int iterations;

    /*On load, initialize a new array of cells and a sorted list to act as a priority queue.
    */
    void Start()
    {
        if (debug) Debug.Log("Beginning Generation\n");
        map = new Cell[size.x, size.y];
        iterations = 0;
        Generate();
    }
    void Generate()
    {
        InitializeGrid();
        if (sampleImage && useSampleImage) SetStatesFromImage();
        //CollapseGrid will return false if the generation fails, so we can restart generation.
        if (!CollapseGrid())
        {
            iterations++;
            if (debug) Debug.Log("Beginning Iteration " + iterations + "\n");
            if (iterations < maximumIterations)
            {
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
                Cell cell = new Cell(false, tileSetDefault);

                map[i, j] = cell;
            }
        }
    }

    /*Set states from the supplied image.
    *
    */
    void SetStatesFromImage()
    {
        int[,] typeMap = GetTileTypeFromImage(sampleImage);

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                Tile[] t = TileSetSelector(typeMap[i, j]);
                map[i, j].Options = t;
            }
        }
        //Update the grid to propagate the restrictions in the added tilesets
        PropagateChange(new Vector2Int(0, 0));
    }

    /*Remove the first element of the list, which will be the element with the smallest key each time.
    */
    bool CollapseGrid()
    {
        bool finished = false;
        bool success = true;
        while (!finished && success)
        {
            //Get an element that is not collapsed with the lowest entropy
            //If there is no such element we can finish
            Vector2Int cellLocation = GetLowestEntropy();
            if (cellLocation.x == size.x && cellLocation.y == size.y)
            {
                if (debug) Debug.Log("Generation Succeeded\n");
                finished = true;
            }
            //If generation fails and there is a cell with no possible options left
            else if (cellLocation.x == -1 && cellLocation.y == -1)
            {
                if (debug) Debug.Log("Generation Failed...\n");
                success = false;
            }
            else
            {
                //Perform operations on lowest entropy element
                Cell target = map[cellLocation.x, cellLocation.y];
                Tile selection = target.Options[Random.Range(0, target.Options.Length)];
                target.Collapsed = true;
                target.Options = new Tile[] { selection };
                PropagateChange(cellLocation);
            }
        }
        return success;
    }

    Vector2Int GetLowestEntropy()
    {
        List<Vector2Int> possibleSpaces = new List<Vector2Int>();
        Cell temp = new Cell(false, tileSetDefault);
        int lowest = tileSetDefault.Length;
        //iterate through each cell in the map, adding its location to the list of possible spaces if it is one of the lowest entropies.
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                temp = map[i, j];
                int count = temp.Options.Length;
                if (count < lowest && !temp.Collapsed)
                {
                    //if there is a failure in generation and there are no more options we can stop here
                    //Return (-1, -1) if there are no possible values.
                    if (count == 0) return new Vector2Int(-1, -1);
                    //otherwise
                    possibleSpaces.Clear();
                    possibleSpaces.Add(new Vector2Int(i, j));
                    lowest = count;
                }
                else if (count == lowest && !temp.Collapsed)
                {
                    possibleSpaces.Add(new Vector2Int(i, j));
                }
            }
        }
        //Return one position from the list of possible options.
        //Return a vector out of bounds if every position has been solved.
        if (possibleSpaces.Count > 0)
        {
            return possibleSpaces[Random.Range(0, possibleSpaces.Count)];
        }
        else
        {
            return new Vector2Int(size.x, size.y);
        }
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

            //If the current length is less than the old length, enqueue its surrounding nodes
            if (oldCount < target.Options.Length)
            {
                if (x > 0 && !map[x - 1, y].Collapsed) q.Enqueue(new Vector2Int(x - 1, y));
                if (x < size.x - 1 && !map[x + 1, y].Collapsed) q.Enqueue(new Vector2Int(x + 1, y));
                if (y > 0 && !map[x, y - 1].Collapsed) q.Enqueue(new Vector2Int(x, y - 1));
                if (y < size.y - 1 && !map[x, y + 1].Collapsed) q.Enqueue(new Vector2Int(x, y + 1));
            }
        }
    }
    int[,] GetTileTypeFromImage(Texture2D image)
    {
        Color[] imageData = image.GetPixels();
        //Create a new texture that's size matches the requested size of the map.
        int[,] typeArray = new int[size.x, size.y];
        //Find the number of pixels in the source image for each pixel in the destination image.
        int xBlockSize = image.width / size.x;
        int yBlockSize = image.height / size.y;

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                //Set each pixel in the return texture to a random color in the corresponding range in the input texture.
                Color pixelColor = imageData[(i + Random.Range(0, xBlockSize)) + (j + Random.Range(0, yBlockSize)) * xBlockSize];
                float g = pixelColor.grayscale;
                int type = 0;
                //get the catagory based on the greyscale value
                if (g > 0 && (g < 0.25 || Mathf.Approximately(g, 0.25f))) type = 0;
                if (g > 0.25 && (g < 0.5 || Mathf.Approximately(g, 0.5f))) type = 1;
                if (g > 0.5 && (g < 0.75 || Mathf.Approximately(g, 0.75f))) type = 2;
                if (g > 0.75 && (g < 1 || Mathf.Approximately(g, 1f))) type = 3;
                typeArray[i, j] = type;
            }
        }
        return typeArray;
    }
    Tile[] TileSetSelector(int type)
    {
        if (type == 0) return tileSetType0;
        if (type == 1) return tileSetType1;
        if (type == 2) return tileSetType2;
        if (type == 3) return tileSetType3;
        else return tileSetDefault;
    }
}