using UnityEngine;

public class OutlineObjectId : MonoBehaviour
{
    [SerializeField] private int objectId;
    [SerializeField] private Renderer render;
    private Material mat;

    private void Start()
    {
        mat = render.material;
        mat.SetColor("_BaseColor", IDToColor(objectId));
    }

    public static Color IDToColor(int id)
    {
        // 低ビットの類似性を避けるための分散
        uint hash = (uint)(id * 2654435761);

        byte r = (byte)((hash >> 0) & 0xFF);
        byte g = (byte)((hash >> 8) & 0xFF);
        byte b = (byte)((hash >> 16) & 0xFF);

        return new Color32(r, g, b, 255);
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying)
        {
            mat?.SetColor("_BaseColor", IDToColor(objectId));
        }
#endif
    }
}
