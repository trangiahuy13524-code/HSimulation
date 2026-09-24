using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectIconButton : MonoBehaviour
{
    [SerializeField] Button button;
    public TextMeshProUGUI text;
    public Image icon;
    public WorldObject worldObject;

    
    private void Start()
    {
        button.onClick.AddListener(SelectObject);
    }

    void SelectObject()
    {
        ObjectSelector.Instance.SelectObject(worldObject);
    }

    
}
