using UnityEngine;

public class DataMain : ScriptableObject, Idatamain
{
    //public string thingID;
    
    

    public string thingName { get; set; }
    public string thingDescription { get; set; }
    [Header("Language")]
    public LocalizedText localizedText;
    public void LocalizeText(Language language)
    {
        thingName = language switch
        {
            Language.en => localizedText.thingName_en,
            Language.vi => localizedText.thingName_vi,
            Language.jp => localizedText.thingName_jp,
            Language.cn => localizedText.thingName_cn,
            _ => ""
        };
        thingDescription = language switch
        {
            Language.en => localizedText.thingDescription_en,
            Language.vi => localizedText.thingDescription_vi,
            Language.jp => localizedText.thingDescription_jp,
            Language.cn => localizedText.thingDescription_cn,
            _ => ""
        };
    }
}