using UnityEngine;
using UnityEditor;
using System.IO;

public class RoundedSpriteGenerator
{
    [MenuItem("Tools/UI/Generate Rounded Rect Sprite")]
    public static void Generate()
    {
        int size = 512;
        int radius = 96; // corner roundness
        Color fill = Color.white;

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool inside = InsideRoundedRect(x, y, size, size, radius);
            tex.SetPixel(x, y, inside ? fill : new Color(0,0,0,0));
        }

        tex.Apply();

        string dir = "Assets/UI_Generated";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets", "UI_Generated");

        string path = dir + "/RoundedRect_512.png";
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.Refresh();
    }

    static bool InsideRoundedRect(int x, int y, int w, int h, int r)
    {
        // clamp into the inner rectangle, then check distance to corners
        int cx = Mathf.Clamp(x, r, w - r - 1);
        int cy = Mathf.Clamp(y, r, h - r - 1);
        int dx = x - cx;
        int dy = y - cy;
        return (dx * dx + dy * dy) <= (r * r);
    }
}
