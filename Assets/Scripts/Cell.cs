//using System;
//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public bool collapsed;
    public Tile[] options;

    public Cell(bool state, Tile[] tiles)
    {
        collapsed = state;
        options = tiles;
    }
    public bool Collapsed
    {
        get { return collapsed; }
        set { collapsed = value; }
    }
    public Tile[] Options
    {
        get { return options; }
        set { options = value; }
    }
}
