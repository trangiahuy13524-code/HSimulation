
public interface Idatamain
{
    public string thingName { get; set; }
    public string thingDescription { get; set; }
    public string nameKey { get; }
    public string descKey { get; }
    public void LocalizeText(LocalizationData localizationData);
}