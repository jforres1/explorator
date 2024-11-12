using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class WaveFunction : MonoBehaviour
{
    public int length;
    public int height;
    public Tile[] tileObjects;
    public List<Cell> gridElements;
    public Cell cellObject;

    int iterations;

    void Awake()
    {
        gridElements = new List<Cell>();
        InitializeGrid();
    }

    void InitializeGrid()
    {
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = Instantiate(cellObject, new Vector2(x, y), Quaternion.identity);
                cell.CreateCell(false, tileObjects);
                gridElements.Add(cell);
            }
        }
        StartCoroutine(CheckEntropy());
    }

    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>(gridElements);
        tempGrid.RemoveAll(c => c.collapsed);
        tempGrid.Sort((a, b) => { return a.options.Length - b.options.Length; });
        int arrLength = tempGrid[0].options.Length;
        int stopIndex = default;
        for (int i = 1; i < tempGrid.Count; i++)
        {
            if (tempGrid[i].options.Length > arrLength)
            {
                stopIndex = i;
                break;
            }
        }
        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }
        yield return new WaitForSeconds(0.01f);
        CollapseCell(tempGrid);
    }
    void CollapseCell(List<Cell> tempGrid)
    {
        int randIndex = UnityEngine.Random.Range(0, tempGrid.Count);
        Cell target = tempGrid[randIndex];
        target.collapsed = true;
        Tile selectedTile = target.options[UnityEngine.Random.Range(0, target.options.Length)];
        target.options = new Tile[] { selectedTile };
        Tile foundTile = target.options[0];
        Instantiate(foundTile, target.transform.position, Quaternion.identity);
        UpdateGeneration();
    }
    void UpdateGeneration()
    {
        List<Cell> newCell = new List<Cell>(gridElements);

        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var index = x + y * height;
                if (gridElements[index].collapsed)
                {
                    newCell[index] = gridElements[index];
                }
                else
                {
                    List<Tile> options = new List<Tile>();
                    foreach (Tile t in gridElements[index].options)
                    {
                        options.Add(t);
                    }

                    if (y > 0)
                    {
                        Cell south = gridElements[x + (y - 1) * length];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in south.options)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].southNeighbors;

                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }
                    if (y < height - 1)
                    {
                        Cell north = gridElements[x + (y + 1) * length];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in north.options)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].northNeighbors;

                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }
                    if (x > 0)
                    {
                        Cell east = gridElements[x - 1 + y * length];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in east.options)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].eastNeighbors;

                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }
                    if (x < length - 1)
                    {
                        Cell west = gridElements[x + 1 + y * length];
                        List<Tile> validOptions = new List<Tile>();
                        foreach (Tile possibleOptions in west.options)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].westNeighbors;

                            validOptions = validOptions.Concat(valid).ToList();
                        }
                        CheckValidity(options, validOptions);
                    }
                    Tile[] newTileList = new Tile[options.Count];
                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }
                    newCell[index].RegenerateCell(newTileList);
                }
            }
        }
        gridElements = newCell;
        iterations++;
        if (iterations < length * height)
        {
            StartCoroutine(CheckEntropy());
        }
    }
    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for (int x = optionList.Count - 1; x >= 0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(x);
            }
        }
    }
}