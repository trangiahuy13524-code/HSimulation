using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIconUI : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Button iconButton;
    [SerializeField] TextMeshProUGUI text;

    public void Setup(Ability ability, WorldObject caster)
    {
        icon.sprite = ability.GetDefaultIcon(caster);
        text.text = ability.name;

        iconButton.onClick.AddListener(() =>
        {
            ability.Execute(caster, icon);
        });
    }
}