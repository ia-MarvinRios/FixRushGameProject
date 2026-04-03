#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureChannelPacker : MonoBehaviour
{
    [Header("Channel Inputs")]
    [Tooltip("R = Metallic\nG = Ambient Oclussion\nB = Height/Displacement\nAlpha = Roughness/Smoothness")]
    public Texture2D red;
    [Tooltip("R = Metallic\nG = Ambient Oclussion\nB = Height/Displacement\nAlpha = Roughness/Smoothness")]
    public Texture2D green;
    [Tooltip("R = Metallic\nG = Ambient Oclussion\nB = Height/Displacement\nAlpha = Roughness/Smoothness")]
    public Texture2D blue;
    [Tooltip("R = Metallic\nG = Ambient Oclussion\nB = Height/Displacement\nAlpha = Roughness/Smoothness")]
    public Texture2D alpha;

    [Header("Options")]
    [Tooltip("Should convert roughness to smoothness?")]
    public bool invertAlpha = false; // Roughness to Smoothness

    public string outputName = "PackedTexture";

    [ContextMenu("Pack Channels")]
    void PackChannels()
    {
        if (red == null && green == null && blue == null && alpha == null)
        {
            Debug.LogError("No textures assigned.");
            return;
        }

        int width = GetWidth();
        int height = GetHeight();

        Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float r = red ? red.GetPixel(x, y).grayscale : 0f;
                float g = green ? green.GetPixel(x, y).grayscale : 0f;
                float b = blue ? blue.GetPixel(x, y).grayscale : 0f;
                float a = alpha ? alpha.GetPixel(x, y).grayscale : 1f;

                // conversion
                if (invertAlpha && alpha != null)
                {
                    a = 1f - a; // Roughness to Smoothness
                }

                output.SetPixel(x, y, new Color(r, g, b, a));
            }
        }

        output.Apply();

        SaveTexture(output);
    }

    int GetWidth()
    {
        if (red) return red.width;
        if (green) return green.width;
        if (blue) return blue.width;
        if (alpha) return alpha.width;
        return 512;
    }

    int GetHeight()
    {
        if (red) return red.height;
        if (green) return green.height;
        if (blue) return blue.height;
        if (alpha) return alpha.height;
        return 512;
    }

    void SaveTexture(Texture2D tex)
    {
        byte[] bytes = tex.EncodeToPNG();

        string path = EditorUtility.SaveFilePanel(
            "Save Packed Texture",
            Application.dataPath,
            outputName,
            "png"
        );

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllBytes(path, bytes);
            Debug.Log("<color=#7DF527>[Texture Packer]</color> Texture saved to: " + path);

            AssetDatabase.Refresh();
        }
    }
}
#endif