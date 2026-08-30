#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;
using ClosedXML.Excel;

public class ExcelExporter : EditorWindow
{
    // ============================================================
    // Settings
    // ============================================================

    private string excelTemplatePath = "";

    private string outputPath =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments
            ),
            "Localization_Updated.xlsx"
        );

    private string sheetName = "Localization";

    private int headerRow = 1;
    private int keyColumn = 1;

    // ============================================================
    // Data
    // ============================================================

    private List<Idatamain> dataMains =
        new List<Idatamain>();

    // ============================================================
    // Menu
    // ============================================================

    [MenuItem("Tools/Localization/Generate Excel")]
    public static void ShowWindow()
    {
        GetWindow<ExcelExporter>(
            "Localization Excel"
        );
    }

    // ============================================================
    // GUI
    // ============================================================

    private void OnGUI()
    {
        GUILayout.Label(
            "Localization Excel",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space();

        // --------------------------------------------------------
        // Excel Template
        // --------------------------------------------------------

        GUILayout.Label(
            "Excel Template",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        excelTemplatePath =
            EditorGUILayout.TextField(
                excelTemplatePath
            );

        if (GUILayout.Button(
            "Select",
            GUILayout.Width(60)))
        {
            SelectExcelTemplate();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // --------------------------------------------------------
        // Output Excel
        // --------------------------------------------------------

        GUILayout.Label(
            "Output Excel",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        outputPath =
            EditorGUILayout.TextField(
                outputPath
            );

        if (GUILayout.Button(
            "Select",
            GUILayout.Width(60)))
        {
            SelectOutputFile();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // --------------------------------------------------------
        // Excel Settings
        // --------------------------------------------------------

        sheetName =
            EditorGUILayout.TextField(
                "Sheet Name",
                sheetName
            );

        headerRow =
            EditorGUILayout.IntField(
                "Header Row",
                headerRow
            );

        keyColumn =
            EditorGUILayout.IntField(
                "Key Column",
                keyColumn
            );

        EditorGUILayout.Space();

        // --------------------------------------------------------
        // Load Data
        // --------------------------------------------------------

        if (GUILayout.Button(
            "Load All Idatamain",
            GUILayout.Height(30)))
        {
            LoadAllIdatamain();
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField(
            "Found:",
            dataMains.Count.ToString()
        );

        EditorGUILayout.Space(10);

        // --------------------------------------------------------
        // Generate
        // --------------------------------------------------------

        GUI.enabled =
            dataMains.Count > 0 &&
            !string.IsNullOrEmpty(
                excelTemplatePath
            );

        if (GUILayout.Button(
            "Generate Excel",
            GUILayout.Height(40)))
        {
            GenerateExcel();
        }

        GUI.enabled = true;
    }

    // ============================================================
    // Load Idatamain
    // ============================================================

    private void LoadAllIdatamain()
    {
        dataMains.Clear();

        string[] guids =
            AssetDatabase.FindAssets(
                "t:ScriptableObject"
            );

        foreach (string guid in guids)
        {
            string path =
                AssetDatabase.GUIDToAssetPath(
                    guid
                );

            ScriptableObject asset =
                AssetDatabase.LoadAssetAtPath<
                    ScriptableObject
                >(path);

            if (asset == null)
                continue;

            if (asset is Idatamain data)
            {
                dataMains.Add(data);
            }
        }

        Debug.Log(
            $"Found {dataMains.Count} Idatamain assets."
        );
    }

    // ============================================================
    // Generate Excel
    // ============================================================

    private void GenerateExcel()
    {
        if (dataMains == null ||
            dataMains.Count == 0)
        {
            ShowError(
                "No Idatamain assets found."
            );

            return;
        }

        if (string.IsNullOrEmpty(
            excelTemplatePath))
        {
            ShowError(
                "Please select an Excel template."
            );

            return;
        }

        if (!File.Exists(
            excelTemplatePath))
        {
            ShowError(
                "The selected Excel template does not exist."
            );

            return;
        }

        if (string.IsNullOrEmpty(
            outputPath))
        {
            ShowError(
                "Please select an output path."
            );

            return;
        }

        try
        {
            // ----------------------------------------------------
            // Collect keys
            // ----------------------------------------------------

            HashSet<string> keys =
                CollectKeys();

            // ----------------------------------------------------
            // Load translations
            // ----------------------------------------------------

            Language[] languages =
                (Language[])Enum.GetValues(
                    typeof(Language)
                );

            Dictionary<
                Language,
                Dictionary<string, string>
            > translations =
                new Dictionary<
                    Language,
                    Dictionary<string, string>
                >();

            foreach (Language language in languages)
            {
                translations[language] =
                    LoadLanguageJSON(language);
            }

            // ----------------------------------------------------
            // Open existing Excel
            // ----------------------------------------------------

            using (XLWorkbook workbook =
                new XLWorkbook(
                    excelTemplatePath
                ))
            {
                IXLWorksheet worksheet =
                    workbook.Worksheets
                        .Worksheet(sheetName);

                // ------------------------------------------------
                // Find language columns
                // ------------------------------------------------

                Dictionary<
                    Language,
                    int
                > languageColumns =
                    FindLanguageColumns(
                        worksheet,
                        languages
                    );

                // ------------------------------------------------
                // Find existing keys
                // ------------------------------------------------

                Dictionary<
                    string,
                    int
                > existingKeys =
                    FindExistingKeys(
                        worksheet
                    );

                // ------------------------------------------------
                // Update / Add
                // ------------------------------------------------

                int updatedCount = 0;
                int addedCount = 0;

                int nextRow =
                    GetNextRow(
                        worksheet
                    );

                foreach (string key in keys)
                {
                    int row;

                    // ============================================
                    // Existing key
                    // ============================================

                    if (existingKeys.TryGetValue(
                        key,
                        out row))
                    {
                        updatedCount++;
                    }
                    else
                    {
                        // ========================================
                        // New key
                        // ========================================

                        row = nextRow++;

                        worksheet.Cell(
                            row,
                            keyColumn
                        ).Value = key;

                        CopyRowFormatting(
                            worksheet,
                            row - 1,
                            row
                        );

                        addedCount++;
                    }

                    // ============================================
                    // Write translations
                    // ============================================

                    foreach (Language language in languages)
                    {
                        int column;

                        if (!languageColumns.TryGetValue(
                            language,
                            out column))
                        {
                            continue;
                        }

                        string value = "";

                        if (translations[language]
                            .TryGetValue(
                                key,
                                out string translation))
                        {
                            value = translation;
                        }

                        // IMPORTANT:
                        //
                        // Only modify the value.
                        //
                        // Existing formatting remains.
                        //
                        worksheet.Cell(
                            row,
                            column
                        ).Value = value;
                    }
                }

                // ------------------------------------------------
                // Save
                // ------------------------------------------------

                string directory =
                    Path.GetDirectoryName(
                        outputPath
                    );

                if (!string.IsNullOrEmpty(
                    directory) &&
                    !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(
                        directory
                    );
                }

                workbook.SaveAs(
                    outputPath
                );

                // ------------------------------------------------
                // Result
                // ------------------------------------------------

                string message =
                    "Excel generated successfully.\n\n" +
                    $"Keys: {keys.Count}\n" +
                    $"Updated: {updatedCount}\n" +
                    $"Added: {addedCount}\n\n" +
                    outputPath;

                Debug.Log(message);

                EditorUtility.DisplayDialog(
                    "Complete",
                    message,
                    "OK"
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Failed to generate Excel:\n" +
                e
            );

            EditorUtility.DisplayDialog(
                "Error",
                e.Message,
                "OK"
            );
        }
    }

    // ============================================================
    // Collect Keys
    // ============================================================

    private HashSet<string> CollectKeys()
    {
        HashSet<string> keys =
            new HashSet<string>();

        foreach (Idatamain data in dataMains)
        {
            if (data == null)
                continue;

            if (!string.IsNullOrWhiteSpace(
                data.nameKey))
            {
                keys.Add(
                    data.nameKey.Trim()
                );
            }

            if (!string.IsNullOrWhiteSpace(
                data.descKey))
            {
                keys.Add(
                    data.descKey.Trim()
                );
            }
        }

        return keys;
    }

    // ============================================================
    // Find Language Columns
    // ============================================================

    private Dictionary<
        Language,
        int
    > FindLanguageColumns(
        IXLWorksheet worksheet,
        Language[] languages)
    {
        Dictionary<
            Language,
            int
        > result =
            new Dictionary<
                Language,
                int
            >();

        foreach (Language language in languages)
        {
            string languageName =
                language.ToString();

            foreach (
                IXLCell cell
                in worksheet.Row(headerRow).CellsUsed())
            {
                string header =
                    cell.GetString().Trim();

                if (string.Equals(
                    header,
                    languageName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    result[language] =
                        cell.Address.ColumnNumber;

                    break;
                }
            }
        }

        return result;
    }

    // ============================================================
    // Find Existing Keys
    // ============================================================

    private Dictionary<
        string,
        int
    > FindExistingKeys(
        IXLWorksheet worksheet)
    {
        Dictionary<
            string,
            int
        > result =
            new Dictionary<
                string,
                int
            >(
                StringComparer.OrdinalIgnoreCase
            );

        foreach (
            IXLCell cell
            in worksheet.Column(keyColumn).CellsUsed())
        {
            if (cell.Address.RowNumber <= headerRow)
                continue;

            string key =
                cell.GetString().Trim();

            if (string.IsNullOrEmpty(key))
                continue;

            if (!result.ContainsKey(key))
            {
                result.Add(
                    key,
                    cell.Address.RowNumber
                );
            }
        }

        return result;
    }

    // ============================================================
    // Get Next Row
    // ============================================================

    private int GetNextRow(
        IXLWorksheet worksheet)
    {
        IXLRange usedRange =
            worksheet.RangeUsed();

        if (usedRange == null)
        {
            return headerRow + 1;
        }

        return usedRange.LastRow()
            .RowNumber() + 1;
    }

    // ============================================================
    // Copy Formatting
    // ============================================================

    private void CopyRowFormatting(
        IXLWorksheet worksheet,
        int sourceRow,
        int targetRow)
    {
        if (sourceRow <= headerRow)
            return;

        IXLRow source =
            worksheet.Row(sourceRow);

        IXLRow target =
            worksheet.Row(targetRow);

        // --------------------------------------------------------
        // Row height
        // --------------------------------------------------------

        target.Height =
            source.Height;

        // --------------------------------------------------------
        // Copy cell styles
        // --------------------------------------------------------

        foreach (
            IXLCell sourceCell
            in source.CellsUsed())
        {
            int column =
                sourceCell.Address.ColumnNumber;

            IXLCell targetCell =
                worksheet.Cell(
                    targetRow,
                    column
                );

            targetCell.Style =
                sourceCell.Style;
        }
    }

    // ============================================================
    // Load JSON
    // ============================================================

    private Dictionary<
        string,
        string
    > LoadLanguageJSON(
        Language language)
    {
        string path =
            Path.Combine(
                Application.dataPath,
                "Resources/Localization/JSON",
                language + ".json"
            );

        if (!File.Exists(path))
        {
            Debug.LogWarning(
                $"JSON not found: {path}"
            );

            return new Dictionary<
                string,
                string
            >();
        }

        try
        {
            string json =
                File.ReadAllText(
                    path,
                    Encoding.UTF8
                );

            Dictionary<
                string,
                string
            > result =
                JsonConvert.DeserializeObject<
                    Dictionary<string, string>
                >(json);

            return result ??
                new Dictionary<
                    string,
                    string
                >();
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"Failed to read JSON:\n" +
                $"{path}\n\n{e}"
            );

            return new Dictionary<
                string,
                string
            >();
        }
    }

    // ============================================================
    // Select Excel Template
    // ============================================================

    private void SelectExcelTemplate()
    {
        string directory =
            Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments
            );

        string path =
            EditorUtility.OpenFilePanel(
                "Select Excel Template",
                directory,
                "xlsx"
            );

        if (string.IsNullOrEmpty(path))
            return;

        excelTemplatePath = path;

        // Automatically create output path
        string folder =
            Path.GetDirectoryName(path);

        string filename =
            Path.GetFileNameWithoutExtension(
                path
            );

        outputPath =
            Path.Combine(
                folder,
                filename + "_Updated.xlsx"
            );
    }

    // ============================================================
    // Select Output
    // ============================================================

    private void SelectOutputFile()
    {
        string directory =
            Path.GetDirectoryName(
                outputPath
            );

        if (string.IsNullOrEmpty(directory))
        {
            directory =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments
                );
        }

        string path =
            EditorUtility.SaveFilePanel(
                "Save Localization Excel",
                directory,
                "Localization_Updated",
                "xlsx"
            );

        if (!string.IsNullOrEmpty(path))
        {
            outputPath = path;
        }
    }

    // ============================================================
    // Error
    // ============================================================

    private void ShowError(
        string message)
    {
        Debug.LogError(message);

        EditorUtility.DisplayDialog(
            "Error",
            message,
            "OK"
        );
    }
}

#endif