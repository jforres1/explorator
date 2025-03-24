using UnityEngine;

public class WFCImageProcessor : MonoBehaviour
{
    public Texture2D image;
    // public int colorCount;
    public Tile[] tileSet;
    public WeightSetOption[] weightsBoundToColor;
    private int x = 0;
    private int y = 0;

    public float[] SampleImage(int i, int j)
    {
        if (x == 0 && y == 0)
        {
            Vector2Int size = gameObject.GetComponent<WaveFunction>().size;
            x = size.x;
            y = size.y;
        }
        int imageWidth = image.width;
        int imageHeight = image.height;
        float minDifference = 100.0f;
        float[] ret = new float[0];
        for (int k = 0; k < weightsBoundToColor.Length; k++)
        {
            Color referenceColor = image.GetPixel((i * imageWidth / x), (j * imageHeight / y));
            float diff = ColorDistance(weightsBoundToColor[k].colorBinding, referenceColor);
            if (diff < minDifference)
            {
                minDifference = diff;
                ret = weightsBoundToColor[k].weights;
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