using UnityEngine;

public static class ItemClassColors
{
    public static readonly Color[] colors =
    {
            Color.white,                        // Poor
            Color.green,                        // Advanced
            Color.blue,                         // Epic
            new Color(1f, 0.5f, 0f),            // Legendary - Orange
            new Color(0.6f, 0f, 0.8f),          // Ultimate - Purple
            new Color(1f, 0.85f, 0.4f),         // Artifact - Golden
            Color.red,                          // World
            Color.black                         // Universal
        };

    public static Color GetColor(ItemClass cls)
        => colors[(int)cls];
}
