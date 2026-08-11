using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResearchButtonUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text title;
    public Button button;

    public void SetData(DataJobResearch research)
    {
        title.text = research.thingName;
    }
}