using UnityEngine;

public interface Idatamain
{
    public string thingName { get; set; }
    public string thingDescription { get; set; }
    public void LocalizeText(Language language);
}
