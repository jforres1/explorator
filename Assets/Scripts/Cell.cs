using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public bool collapsed;
    public Tile[] options;

    public void CreateCell(bool state, Tile[] tiles)
    {
        this.collapsed = state;
        this.options = tiles;
    }

    public void RegenerateCell(Tile[] tiles)
    {
        this.options = tiles;
    }
}