using UnityEngine;

public class SpriteScriptableBase : ScriptableObject
{
    public Vector2 offset;
    public Vector2 childOffset;
    public float horizontalOffset;
    public Vector2 scale = Vector2.one;

    [Header("Biology")]
    public BodySex bodySex = BodySex.Both;
    public bool hybridable = false;
}

