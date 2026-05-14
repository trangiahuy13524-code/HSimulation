using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIconUI : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] Button iconButton;
    [SerializeField] TextMeshProUGUI text;

    public void Setup(Ability ability, WorldObject owner)
    {
        icon.sprite = ability.GetDefaultIcon(owner);
        text.text = ability.name;

        iconButton.onClick.AddListener(() =>
        {
            ability.Execute(owner, icon);
        });
    }
}