#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class LocalizationJsonConverterWindow : EditorWindow
{
    private TextAsset csvFile;
    private string outputFolder = "Assets/Resources/Localization/JSON";

    [MenuItem("Tools/Localization/CSV to JSON")]
    public static void ShowWindow()
    {
        GetWindow<LocalizationJsonConverterWindow>("CSV to JSON");
    }

    private void OnGUI()
    {
        GUILayout.Label("Localization CSV → JSON", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        csvFile = (TextAsset)EditorGUILayout.ObjectField(
            "CSV File",
            csvFile,
            typeof(TextAsset),
            false
        );

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        outputFolder = EditorGUILayout.TextField(
            "Output Folder",
            outputFolder
        );

        if (GUILayout.Button("Select", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel(
                "Select Output Folder",
                "Assets",
                ""
            );

            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                {
                    outputFolder = "Assets" +
                        selected.Substring(Application.dataPath.Length);
                }
                else
                {
                    EditorUtility.DisplayDialog(
                        "Invalid Folder",
                        "Folder phải nằm bên trong Assets.",
                        "OK"
                    );
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(20);

        GUI.enabled = csvFile != null;

        if (GUILayout.Button("Convert to JSON", GUILayout.Height(35)))
        {
            Convert();
        }

        GUI.enabled = true;
    }

    private void Convert()
    {
        string[] lines = csvFile.text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Split('\n');

        if (lines.Length < 2)
        {
            EditorUtility.DisplayDialog(
                "Error",
                "CSV không có đủ dữ liệu.",
                "OK"
            );

            return;
        }

        // Header
        string[] headers = ParseCSVLine(lines[0]);

        if (headers.Length < 2)
        {
            EditorUtility.DisplayDialog(
                "Error",
                "CSV phải có ít nhất 1 cột ngôn ngữ.",
                "OK"
            );

            return;
        }

        int languageCount = headers.Length - 1;

        // language -> dictionary
        Dictionary<string, Dictionary<string, string>> languages =
            new Dictionary<string, Dictionary<string, string>>();

        for (int i = 1; i < headers.Length; i++)
        {
            string language = headers[i].Trim();

            if (string.IsNullOrEmpty(language))
                continue;

            languages[language] =
                new Dictionary<string, string>();
        }

        // Data
        for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
        {
            if (string.IsNullOrWhiteSpace(lines[lineIndex]))
                continue;

            string[] columns = ParseCSVLine(lines[lineIndex]);

            if (columns.Length == 0)
                continue;

            // Cột đầu tiên là Key
            string key = columns[0].Trim();

            if (string.IsNullOrEmpty(key))
                continue;

            for (int languageIndex = 1;
                 languageIndex < headers.Length;
                 languageIndex++)
            {
                string language = headers[languageIndex].Trim();

                if (!languages.ContainsKey(language))
                    continue;

                string value = "";

                if (languageIndex < columns.Length)
                    value = columns[languageIndex];

                languages[language][key] = value;
            }
        }

        // Create folder
        string fullOutputPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            outputFolder
        );

        if (!Directory.Exists(fullOutputPath))
            Directory.CreateDirectory(fullOutputPath);

        // Generate JSON
        foreach (var language in languages)
        {
            string json = ConvertDictionaryToJson(
                language.Value
            );

            string filePath = Path.Combine(
                fullOutputPath,
                language.Key + ".json"
            );

            File.WriteAllText(
                filePath,
                json,
                new UTF8Encoding(false)
            );

            Debug.Log(
                $"Localization generated: {filePath}"
            );
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Complete",
            $"Đã tạo {languages.Count} file JSON.",
            "OK"
        );
    }

    private static string ConvertDictionaryToJson(
        Dictionary<string, string> dictionary)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("{");

        int index = 0;

        foreach (var pair in dictionary)
        {
            sb.Append("  ");
            sb.Append(JsonEscape(pair.Key));
            sb.Append(": ");
            sb.Append(JsonEscape(pair.Value));

            if (index < dictionary.Count - 1)
                sb.Append(",");

            sb.AppendLine();

            index++;
        }

        sb.Append("}");

        return sb.ToString();
    }

    private static string JsonEscape(string value)
    {
        if (value == null)
            return "\"\"";

        StringBuilder sb = new StringBuilder();

        sb.Append('"');

        foreach (char c in value)
        {
            switch (c)
            {
                case '"':
                    sb.Append("\\\"");
                    break;

                case '\\':
                    sb.Append("\\\\");
                    break;

                case '\b':
                    sb.Append("\\b");
                    break;

                case '\f':
                    sb.Append("\\f");
                    break;

                case '\n':
                    sb.Append("\\n");
                    break;

                case '\r':
                    sb.Append("\\r");
                    break;

                case '\t':
                    sb.Append("\\t");
                    break;

                default:
                    sb.Append(c);
                    break;
            }
        }

        sb.Append('"');

        return sb.ToString();
    }

    private static string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();

        StringBuilder current = new StringBuilder();

        bool insideQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (insideQuotes &&
                    i + 1 < line.Length &&
                    line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }

                continue;
            }

            if (c == ',' && !insideQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());

        return result.ToArray();
    }
}

#endif