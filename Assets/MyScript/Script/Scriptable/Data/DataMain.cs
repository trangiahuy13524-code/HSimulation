using UnityEngine;

public class DataMain : ScriptableObject, Idatamain
{
    public string thingName { get; set; }
    public string thingDescription { get; set; }
    public string nameKey => thingNameKey;
    public string descKey => thingDescriptionKey;
    [Header("Language")]
    public string thingNameKey = "";
    public string thingDescriptionKey = "";

    public void LocalizeText(LocalizationData localizationData)
    {
        thingName = localizationData.Get(thingNameKey);
        thingDescription = localizationData.Get(thingDescriptionKey);
    }
}