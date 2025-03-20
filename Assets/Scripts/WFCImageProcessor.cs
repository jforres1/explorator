using UnityEngine;

public class WFCImageProcessor : MonoBehaviour
{
    public Texture2D image;
    // public int colorCount;
    public Tile[] tileSet;
    public WeightSetOption[] weightsBoundToColor;

    public int[,] SampleImage()
    {
        Vector2Int size = gameObject.GetComponent<WaveFunction>().size;
        int x = size.x;
        int y = size.y;
        int imageWidth = image.width;
        int imageHeight = image.height;
        int[,] ret = new int[x, y];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                float minDifference = 100.0f;
                int minIndex = -1;
                for (int k = 0; k < weightsBoundToColor.Length; k++)
                {
                    Color referenceColor = image.GetPixel((i * imageWidth / x), (j * imageHeight / y));
                    float diff = ColorDistance(weightsBoundToColor[k].colorBinding, referenceColor);
                    if (diff < minDifference)
                    {
                        minDifference = diff;
                        minIndex = k;
                    }
                }
                ret[i, j] = minIndex;
            }
        }
        return ret;
    }
    public float ColorDistance(Color c1, Color c2)
    {
        float redDiff = c2.r - c1.r;
        float blueDiff = c2.b - c1.b;
        float greenDiff = c2.g - c1.g;
        float dist = redDiff * redDiff + blueDiff * blueDiff + greenDiff * greenDiff;
        return dist;
    }
}

[System.Serializable]
public class WeightSetOption
{
    public Color colorBinding;
    public float[] weights;
}