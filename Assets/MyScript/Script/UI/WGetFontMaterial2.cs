using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WGetFontMaterial2 : MonoBehaviour
{
    [SerializeField] private List<TextMeshPro> texts2;


    void Start()
    {

        if (texts2 != null && texts2.Count > 0)
        {
            foreach (var text in texts2)
            {
                text.font = WorldData.Instance.globalFontAsset;
            }
        }

        Destroy(this);
    }
}