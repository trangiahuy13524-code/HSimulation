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

//[CustomEditor(typeof(Pawn), true)]
//public class TestButtonEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        // VERY IMPORTANT SAFETY CHECK
//        if (target == null)
//            return;

//        serializedObject.Update();

//        DrawDefaultInspector();

//        Pawn script = target as Pawn;

//        if (script == null)
//            return;

//        GUILayout.Space(10);

//        if (GUILayout.Button("Up"))
//            script.Walk(DiagonalDirection.North);

//        if (GUILayout.Button("Down"))
//            script.Walk(DiagonalDirection.South);

//        if (GUILayout.Button("Left"))
//            script.Walk(DiagonalDirection.West);

//        if (GUILayout.Button("Right"))
//            script.Walk(DiagonalDirection.East);

//        if (GUILayout.Button("UpLeft"))
//            script.Walk(DiagonalDirection.NorthWest);

//        if (GUILayout.Button("UpRight"))
//            script.Walk(DiagonalDirection.NorthEast);

//        if (GUILayout.Button("DownLeft"))
//            script.Walk(DiagonalDirection.SouthWest);

//        if (GUILayout.Button("DownRight"))
//            script.Walk(DiagonalDirection.SouthEast);

//        serializedObject.ApplyModifiedProperties();
//    }
//}