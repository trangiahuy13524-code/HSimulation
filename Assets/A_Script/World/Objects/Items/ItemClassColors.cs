using UnityEngine;

public static class ItemClassColors
{
    public static readonly Color[] colors =
    {
            Color.grey,                        // Poor
            Color.green,                        // Normal
            Color.blue,                         // Advanced
            new Color(1f, 0.5f, 0f),            // Epic
            new Color(0.6f, 0f, 0.8f),          // Legendary
            new Color(1f, 0.85f, 0.4f),         // Ultimate
            Color.red,                          // World
            Color.black                         // Universal
        };

    public static Color GetColor(ItemClass cls)
        => colors[(int)cls];
}
