using System;
//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    bool collapsed;
    Tile[] options;
    float[] weights;

    public Cell(bool state, Tile[] tiles)
    {
        collapsed = state;
        options = tiles;
        weights = new float[tiles.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = 1.0f;
        }
    }
    public Cell(bool state, Tile[] tiles, float[] w)
    {
        collapsed = state;
        options = tiles;
        weights = w;
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
    public float[] Weights
    {
        get { return weights; }
        set { weights = value; }
    }
    public float[] SoftMax()
    {
        float[] p = new float[weights.Length];
        float sum = 0.0f;
        for (int i = 0; i < weights.Length; i++)
        {
            p[i] = (float)Math.Exp(weights[i]);
            sum += p[i];
        }
        for (int i = 0; i < weights.Length; i++)
        {
            p[i] /= sum;
        }
        return p;
    }
    public Tile GetRandomTile()
    {
        Tile t = Options[0];
        float f = UnityEngine.Random.Range(0, 1.0f);
        float[] probabilities = SoftMax();
        for (int i = 0; i < probabilities.Length; i++)
        {
            if (f < probabilities[i])
            {
                t = Options[i];
                break;
            }
            else
            {
                f -= probabilities[i];
            }
        }
        return t;
    }
}
