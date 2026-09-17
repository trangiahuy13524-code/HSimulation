using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExcelExporter : EditorWindow
{
    // ============================================================
    // Settings
    // ============================================================

    private string excelTemplatePath = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments
        ),
        "HumanSimulation.xlsx"
    );

    private string outputPath = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.MyDocuments
        ),
        "HumanSimulation.xlsx"
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
            // ====================================================
            // Collect Keys
            // ====================================================

            HashSet<string> keys =
                CollectKeys();

            if (keys.Count == 0)
            {
                ShowError(
                    "No localization keys were found."
                );

                return;
            }

            // ====================================================
            // Load Languages
            // ====================================================

            Language[] languages =
                (Language[])Enum.GetValues(
                    typeof(Language)
                );

            // ====================================================
            // Load Translations
            // ====================================================

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

            // ====================================================
            // Open Excel
            // ====================================================

            using (XLWorkbook workbook =
                new XLWorkbook(
                    excelTemplatePath
                ))
            {
                // ------------------------------------------------
                // Find Worksheet
                // ------------------------------------------------

                if (!workbook.Worksheets.Contains(
                    sheetName))
                {
                    ShowError(
                        $"Worksheet '{sheetName}' was not found."
                    );

                    return;
                }



                IXLWorksheet worksheet =
                    workbook.Worksheets
                        .Worksheet(sheetName);

                // =================================================
                // CLEAR EVERYTHING
                // =================================================

                

                worksheet.Clear(
                    XLClearOptions.All
                );

                for (int i = worksheet.Tables.Count() - 1; i >= 0; i--)
                {
                    worksheet.Tables.Remove(i);
                }

                // =================================================
                // Write Header
                // =================================================

                int lastColumn =
                    keyColumn + languages.Length;

                // Key header
                IXLCell cell = worksheet.Cell(
                    headerRow,
                    keyColumn
                );
                cell.Value = "Key";
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Language headers
                for (int i = 0;
                     i < languages.Length;
                     i++)
                {
                    int column =
                        keyColumn + i + 1;

                    cell = worksheet.Cell(
                        headerRow,
                        column
                    );
                    cell.Value = languages[i].ToString();
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // ------------------------------------------------
                // Header formatting
                // ------------------------------------------------

                IXLRange headerRange =
                    worksheet.Range(
                        headerRow,
                        keyColumn,
                        headerRow,
                        lastColumn
                    );

                headerRange.Style.Font.Bold = true;

                // =================================================
                // Write Data
                // =================================================

                int currentRow =
                    headerRow + 1;

                foreach (string key in keys)
                {
                    // ------------------------------------------------
                    // Key
                    // ------------------------------------------------

                    worksheet.Cell(
                        currentRow,
                        keyColumn
                    ).Value = key;

                    // ------------------------------------------------
                    // Translations
                    // ------------------------------------------------

                    for (int i = 0;
                         i < languages.Length;
                         i++)
                    {
                        Language language =
                            languages[i];

                        int column =
                            keyColumn + i + 1;

                        string value = "";

                        if (translations[language]
                            .TryGetValue(
                                key,
                                out string translation))
                        {
                            value =
                                translation ?? "";
                        }

                        worksheet.Cell(
                            currentRow,
                            column
                        ).Value = value;
                    }

                    currentRow++;
                }

                // =================================================
                // Create Table
                // =================================================

                int lastRow =
                    currentRow - 1;

                IXLRange tableRange =
                    worksheet.Range(
                        headerRow,
                        keyColumn,
                        lastRow,
                        lastColumn
                    );

                IXLTable table =
                    tableRange.CreateTable(
                        "LocalizationTable"
                    );

                // Show table styling
                table.Theme =
                    XLTableTheme.TableStyleMedium2;

                // =================================================
                // Formatting
                // =================================================

                worksheet.Columns()
                    .AdjustToContents();

                // Prevent extremely wide columns
                worksheet.Column(keyColumn)
                    .Width = 35;

                for (int i = 0;
                     i < languages.Length;
                     i++)
                {
                    int column =
                        keyColumn + i + 1;

                    worksheet.Column(column)
                        .Width = 40;
                }

                // =================================================
                // Save
                // =================================================

                string directory =
                    Path.GetDirectoryName(
                        outputPath
                    );

                if (!string.IsNullOrEmpty(
                    directory) &&
                    !Directory.Exists(
                        directory))
                {
                    Directory.CreateDirectory(
                        directory
                    );
                }

                workbook.SaveAs(
                    outputPath
                );

                // =================================================
                // Result
                // =================================================

                string message =
                    "Excel generated successfully.\n\n" +
                    $"Keys: {keys.Count}\n" +
                    $"Languages: {languages.Length}\n" +
                    $"Rows: {keys.Count}\n" +
                    $"Table: LocalizationTable\n\n" +
                    outputPath;

                Debug.Log(message);

                EditorUtility.DisplayDialog(
                    "Complete",
                    message,
                    "OK"
                );
            }
        }
        catch (IOException e)
        {
            string message =
                "Cannot access the Excel file.\n\n" +
                "The file may currently be open in Excel " +
                "or locked by OneDrive.\n\n" +
                e.Message;

            Debug.LogError(message);

            EditorUtility.DisplayDialog(
                "File Locked",
                message,
                "OK"
            );
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
                $"JSON not found, creating: {path}"
            );

            Dictionary<string, string> newData =
                new Dictionary<string, string>();

            string json =
                JsonConvert.SerializeObject(
                    newData,
                    Formatting.Indented
                );

            string directory =
                Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(
                path,
                json,
                Encoding.UTF8
            );

            return newData;
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
