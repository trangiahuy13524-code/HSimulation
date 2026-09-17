using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using Newtonsoft.Json;
using ClosedXML.Excel;

public class ExcelLocalizationImporter : EditorWindow
{
    // ============================================================
    // Settings
    // ============================================================

    private string excelPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments
            ),
            "HumanSimulation.xlsx"
        );

    private string outputFolder =
        Path.Combine(
            Application.dataPath,
            "Resources/Localization/JSON"
        );

    private string sheetName = "Localization";

    private int headerRow = 1;
    private int keyColumn = 1;

    // ============================================================
    // Preview
    // ============================================================

    private int keyCount = 0;
    private int languageCount = 0;

    // ============================================================
    // Menu
    // ============================================================

    [MenuItem("Tools/Localization/Import Excel")]
    public static void ShowWindow()
    {
        GetWindow<ExcelLocalizationImporter>(
            "Import Localization"
        );
    }

    // ============================================================
    // GUI
    // ============================================================

    private void OnGUI()
    {
        GUILayout.Label(
            "Excel → JSON",
            EditorStyles.boldLabel
        );

        EditorGUILayout.Space();

        // --------------------------------------------------------
        // Excel
        // --------------------------------------------------------

        GUILayout.Label(
            "Excel File",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        excelPath =
            EditorGUILayout.TextField(
                excelPath
            );

        if (GUILayout.Button(
            "Select",
            GUILayout.Width(60)))
        {
            SelectExcelFile();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // --------------------------------------------------------
        // Output
        // --------------------------------------------------------

        GUILayout.Label(
            "JSON Output Folder",
            EditorStyles.boldLabel
        );

        EditorGUILayout.BeginHorizontal();

        outputFolder =
            EditorGUILayout.TextField(
                outputFolder
            );

        if (GUILayout.Button(
            "Select",
            GUILayout.Width(60)))
        {
            SelectOutputFolder();
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

        EditorGUILayout.Space(10);

        // --------------------------------------------------------
        // Analyze
        // --------------------------------------------------------

        GUI.enabled =
            !string.IsNullOrEmpty(excelPath) &&
            File.Exists(excelPath);

        if (GUILayout.Button(
            "Analyze Excel",
            GUILayout.Height(30)))
        {
            AnalyzeExcel();
        }

        GUI.enabled = true;

        EditorGUILayout.Space();

        // --------------------------------------------------------
        // Info
        // --------------------------------------------------------

        EditorGUILayout.LabelField(
            "Keys:",
            keyCount.ToString()
        );

        EditorGUILayout.LabelField(
            "Languages:",
            languageCount.ToString()
        );

        EditorGUILayout.Space(10);

        // --------------------------------------------------------
        // Import
        // --------------------------------------------------------

        GUI.enabled =
            !string.IsNullOrEmpty(excelPath) &&
            File.Exists(excelPath);

        if (GUILayout.Button(
            "Import Excel → JSON",
            GUILayout.Height(40)))
        {
            ImportExcel();
        }

        GUI.enabled = true;
    }

    // ============================================================
    // Select Excel
    // ============================================================

    private void SelectExcelFile()
    {
        string directory =
            Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments
            );

        string path =
            EditorUtility.OpenFilePanel(
                "Select Localization Excel",
                directory,
                "xlsx"
            );

        if (string.IsNullOrEmpty(path))
            return;

        excelPath = path;

        AnalyzeExcel();
    }

    // ============================================================
    // Select Output Folder
    // ============================================================

    private void SelectOutputFolder()
    {
        string path =
            EditorUtility.OpenFolderPanel(
                "Select JSON Output Folder",
                outputFolder,
                ""
            );

        if (!string.IsNullOrEmpty(path))
        {
            outputFolder = path;
        }
    }

    // ============================================================
    // Analyze Excel
    // ============================================================

    private void AnalyzeExcel()
    {
        keyCount = 0;
        languageCount = 0;

        if (string.IsNullOrEmpty(excelPath))
            return;

        if (!File.Exists(excelPath))
        {
            ShowError(
                "Excel file does not exist."
            );

            return;
        }

        try
        {
            using (XLWorkbook workbook =
                new XLWorkbook(excelPath))
            {
                if (!workbook.Worksheets.Contains(sheetName))
                {
                    ShowError(
                        $"Worksheet '{sheetName}' was not found."
                    );

                    return;
                }

                IXLWorksheet worksheet =
                    workbook.Worksheets
                        .Worksheet(sheetName);

                Language[] languages =
                    (Language[])Enum.GetValues(
                        typeof(Language)
                    );

                Dictionary<Language, int>
                    languageColumns =
                        FindLanguageColumns(
                            worksheet,
                            languages
                        );

                languageCount =
                    languageColumns.Count;

                Dictionary<string, int>
                    existingKeys =
                    FindExistingKeys(
                        worksheet
                    );

                keyCount =
                    existingKeys.Count;

                Debug.Log(
                    $"Excel analyzed.\n" +
                    $"Keys: {keyCount}\n" +
                    $"Languages: {languageCount}"
                );
            }
        }
        catch (Exception e)
        {
            ShowError(
                "Failed to analyze Excel:\n\n" +
                e.Message
            );
        }
    }

    // ============================================================
    // Import
    // ============================================================

    private void ImportExcel()
    {
        if (string.IsNullOrEmpty(excelPath))
        {
            ShowError(
                "Please select an Excel file."
            );

            return;
        }

        if (!File.Exists(excelPath))
        {
            ShowError(
                "The selected Excel file does not exist."
            );

            return;
        }

        if (string.IsNullOrEmpty(outputFolder))
        {
            ShowError(
                "Please select an output folder."
            );

            return;
        }

        try
        {
            using (XLWorkbook workbook =
                new XLWorkbook(excelPath))
            {
                // ------------------------------------------------
                // Find worksheet
                // ------------------------------------------------

                if (!workbook.Worksheets.Contains(sheetName))
                {
                    ShowError(
                        $"Worksheet '{sheetName}' was not found."
                    );

                    return;
                }

                IXLWorksheet worksheet =
                    workbook.Worksheets
                        .Worksheet(sheetName);

                // ------------------------------------------------
                // Languages
                // ------------------------------------------------

                Language[] languages =
                    (Language[])Enum.GetValues(
                        typeof(Language)
                    );

                Dictionary<
                    Language,
                    int
                > languageColumns =
                    FindLanguageColumns(
                        worksheet,
                        languages
                    );

                if (languageColumns.Count == 0)
                {
                    ShowError(
                        "No Language columns were found."
                    );

                    return;
                }

                // ------------------------------------------------
                // Keys
                // ------------------------------------------------

                Dictionary<
                    string,
                    int
                > existingKeys =
                    FindExistingKeys(
                        worksheet
                    );

                if (existingKeys.Count == 0)
                {
                    ShowError(
                        "No localization keys were found."
                    );

                    return;
                }

                // ------------------------------------------------
                // Create output directory
                // ------------------------------------------------

                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(
                        outputFolder
                    );
                }

                // ------------------------------------------------
                // Generate JSON for each language
                // ------------------------------------------------

                int generatedFiles = 0;

                foreach (
                    KeyValuePair<
                        Language,
                        int
                    > languageColumn
                    in languageColumns)
                {
                    Language language =
                        languageColumn.Key;

                    int column =
                        languageColumn.Value;

                    Dictionary<
                        string,
                        string
                    > translations =
                        new Dictionary<
                            string,
                            string
                        >(
                            StringComparer.OrdinalIgnoreCase
                        );

                    foreach (
                        KeyValuePair<
                            string,
                            int
                        > key
                        in existingKeys)
                    {
                        string localizationKey =
                            key.Key;

                        int row =
                            key.Value;

                        IXLCell cell =
                            worksheet.Cell(
                                row,
                                column
                            );

                        string value =
                            cell.GetString();

                        translations[
                            localizationKey
                        ] = value;
                    }

                    // ------------------------------------------------
                    // Serialize
                    // ------------------------------------------------

                    string json =
                        JsonConvert.SerializeObject(
                            translations,
                            Formatting.Indented
                        );

                    // ------------------------------------------------
                    // Save
                    // ------------------------------------------------

                    string filePath =
                        Path.Combine(
                            outputFolder,
                            language + ".json"
                        );

                    File.WriteAllText(
                        filePath,
                        json,
                        new UTF8Encoding(false)
                    );

                    generatedFiles++;

                    Debug.Log(
                        $"Generated: {filePath}\n" +
                        $"Keys: {translations.Count}"
                    );
                }

                // ------------------------------------------------
                // Refresh Unity
                // ------------------------------------------------

                AssetDatabase.Refresh();

                // ------------------------------------------------
                // Result
                // ------------------------------------------------

                string message =
                    "Localization imported successfully.\n\n" +
                    $"Keys: {existingKeys.Count}\n" +
                    $"Languages: {languageColumns.Count}\n" +
                    $"JSON files: {generatedFiles}\n\n" +
                    outputFolder;

                Debug.Log(message);

                EditorUtility.DisplayDialog(
                    "Import Complete",
                    message,
                    "OK"
                );
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Failed to import Excel:\n" +
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
                in worksheet
                    .Row(headerRow)
                    .CellsUsed())
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
            in worksheet
                .Column(keyColumn)
                .CellsUsed())
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