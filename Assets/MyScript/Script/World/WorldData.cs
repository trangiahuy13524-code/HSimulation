using System.Collections.Generic;
using System.Linq;
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

        Language lang = Language.vi;
        Localize(genes, lang);
        Localize(buildings, lang);
        Localize(items, lang);
        Localize(attires, lang);
        Localize(tiles, lang);
        Localize(walls, lang);
        Localize(dataResearches, lang);

        //List<Idatamain> allData = new();

        //allData.AddRange(genes.Cast<Idatamain>());
        //allData.AddRange(buildings.Cast<Idatamain>());
        //allData.AddRange(items.Cast<Idatamain>());
        //allData.AddRange(attires.Cast<Idatamain>());
        //allData.AddRange(tiles.Cast<Idatamain>());
        //allData.AddRange(walls.Cast<Idatamain>());
        //allData.AddRange(dataResearches.Cast<Idatamain>());

        //foreach (var data in allData)
        //{
        //    data.LocalizeText(Language.vi);
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Localize<T>(T[] datas, Language lang) where T : Idatamain
    {
        foreach (var data in datas)
        {
            data.LocalizeText(lang);
        }
    }
}
