using System.Collections.Generic;


public class LocalizationData
{
    public Dictionary<string, string> Texts { get; private set; }

    public LocalizationData(Dictionary<string, string> texts)
    {
        Texts = texts;
    }

    public string Get(string key)
    {
        if (Texts == null) return $"[{key}]";

        if (Texts.TryGetValue(key, out string value))
            return value;

        return $"[{key}]";
    }
}
