using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WGetFontMaterial : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> texts;


    void Start()
    {
        if (texts != null && texts.Count > 0)
        {
            foreach (var text in texts)
            {
                text.font = WorldData.Instance.globalFontAsset;
            }
        }
        Destroy(this);
    }
}