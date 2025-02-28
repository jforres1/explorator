using UnityEngine;

public class WFCImageProcessor : MonoBehaviour
{
    public Texture2D image;
    public int colorCount;
    //public Vector2Int size;
    public int[,] SampleImage()
    {
        Vector2Int size = gameObject.GetComponent<WaveFunction>().size;
        int x = size.x;
        int y = size.y;
        int[,] ret = new int[x, y];
        float stepSize = 1.0f;
        if (colorCount > 0) stepSize = 1.0f / colorCount;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                if (colorCount > 0)
                {
                    int xIndex = Mathf.FloorToInt(((float)i / x) * image.width);
                    int yIndex = Mathf.FloorToInt(((float)j / y) * image.height);
                    ret[i, j] = Mathf.FloorToInt(image.GetPixel(xIndex, yIndex).grayscale / stepSize);
                }
                else
                {
                    ret[i, j] = 0;
                }
            }
        }
        return ret;
    }
}
