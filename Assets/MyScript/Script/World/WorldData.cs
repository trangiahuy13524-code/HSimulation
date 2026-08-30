using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class WorldData : MonoBehaviour
{
    [Header("LayerIndex")]
    public int bottomGridLayer = 1000;
    public int spacing = 10;
    [Header("Ref")]
    public int topGridLayer = 1000;

    [Header("Shader")]
    public Material defaultMat;
    public Material hoverMat;
    public Material selectedMat;
    [Header("Data")]
    [SerializeField] DataGenetics[] genes;
    [SerializeField] DataBuilding[] buildings;
    [SerializeField] DataItem[] items;
    [SerializeField] DataAttire[] attires;
    [SerializeField] DataTile[] tiles;
    [SerializeField] DataWall[] walls;
    [SerializeField] DataJobResearch[] dataResearches;
    [SerializeField] DataAbility[] abilities;
    [SerializeField] DataSkill[] skills;
    [Header("Language")]
    [SerializeField] private Language currentLanguage;
    private LocalizationData localizationData;

    public static WorldData Instance { get; private set; }
    void Start()
    {
        Instance = this;
        topGridLayer = WorldMap.Instance.WorldSize * spacing + bottomGridLayer;

        genes = Resources.LoadAll<DataGenetics>("Data/Genes");
        buildings = Resources.LoadAll<DataBuilding>("Data/Buildings");
        items = Resources.LoadAll<DataItem>("Data/Items");
        attires = Resources.LoadAll<DataAttire>("Data/Attires");
        tiles = Resources.LoadAll<DataTile>("Data/Tiles");
        walls = Resources.LoadAll<DataWall>("Data/Walls");
        dataResearches = Resources.LoadAll<DataJobResearch>("Data/Researches");
        abilities = Resources.LoadAll<DataAbility>("Data/Abilities");
        skills = Resources.LoadAll<DataSkill>("Data/Skills");

        LoadLanguage(currentLanguage);

        List<Idatamain> allData = new();
        allData.AddRange(genes);
        allData.AddRange(buildings);
        allData.AddRange(items);
        allData.AddRange(attires);
        allData.AddRange(tiles);
        allData.AddRange(walls);
        allData.AddRange(dataResearches);
        allData.AddRange(abilities);
        allData.AddRange(skills);
        Localize(allData);
    }

    public void LoadLanguage(Language language)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>(
            $"Localization/JSON/{language}"
        );

        if (jsonFile == null)
        {
            Debug.LogError(
                $"Localization file not found: {language}"
            );

            return;
        }

        Dictionary<string, string> dictionary =
            JsonConvert.DeserializeObject<Dictionary<string, string>>(
                jsonFile.text
            );

        localizationData = new LocalizationData(dictionary);

        Debug.Log($"Loaded language: {language}");
    }

    void Localize<T>(T[] datas) where T : Idatamain
    {
        foreach (var data in datas)
        {
            data.LocalizeText(localizationData);
        }
    }

    void Localize<T>(List<T> datas) where T : Idatamain
    {
        foreach (var data in datas)
        {
            data.LocalizeText(localizationData);
        }
    }
}
