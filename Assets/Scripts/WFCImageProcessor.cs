using UnityEngine;

public class WFCImageProcessor : MonoBehaviour
{
    public Texture2D image;
    public int colorCount;
    Vector2Int size;
    void Start()
    {
        WaveFunction wfc = gameObject.GetComponent<WaveFunction>();
        size = wfc.size;
    }
}
