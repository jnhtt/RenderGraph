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
        uint r = (uint)((id & 0x000000FF) >> 0);
        uint g = (uint)((id & 0x0000FF00) >> 8);
        uint b = (uint)((id & 0x00FF0000) >> 16);
        return new Color(r / 255f, g / 255f, b / 255f, 1);
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
