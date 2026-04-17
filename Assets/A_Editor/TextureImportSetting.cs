using UnityEditor;
using UnityEngine;

//public class TextureImportSettings : AssetPostprocessor
//{
//    void OnPreprocessTexture()
//    {
//        TextureImporter importer = (TextureImporter)assetImporter;

//        // Set as Sprite
//        importer.textureType = TextureImporterType.Sprite;

//        // Sprite Mode = Multiple
//        importer.spriteImportMode = SpriteImportMode.Single;

//        // Filter Mode = Bilinear
//        importer.filterMode = FilterMode.Bilinear;

//        // Optional recommended 2D settings
//        importer.mipmapEnabled = false;
//        importer.alphaIsTransparency = true;
//        importer.textureCompression = TextureImporterCompression.Uncompressed;
//    }
//}

[CustomEditor(typeof(Pawn), true)]
public class TestButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // VERY IMPORTANT SAFETY CHECK
        if (target == null)
            return;

        serializedObject.Update();

        DrawDefaultInspector();

        Pawn script = target as Pawn;

        if (script == null)
            return;

        GUILayout.Space(10);

        if (GUILayout.Button("Up"))
            script.Walk(DiagonalDirection.up);

        if (GUILayout.Button("Down"))
            script.Walk(DiagonalDirection.down);

        if (GUILayout.Button("Left"))
            script.Walk(DiagonalDirection.left);

        if (GUILayout.Button("Right"))
            script.Walk(DiagonalDirection.right);

        if (GUILayout.Button("UpLeft"))
            script.Walk(DiagonalDirection.upleft);

        if (GUILayout.Button("UpRight"))
            script.Walk(DiagonalDirection.upright);

        if (GUILayout.Button("DownLeft"))
            script.Walk(DiagonalDirection.downleft);

        if (GUILayout.Button("DownRight"))
            script.Walk(DiagonalDirection.downright);

        serializedObject.ApplyModifiedProperties();
    }
}