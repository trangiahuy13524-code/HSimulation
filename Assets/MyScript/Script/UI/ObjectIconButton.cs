using UnityEngine;
using UnityEngine.UI;

public class ObjectIconButton : MonoBehaviour
{
    [SerializeField] Button button;
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
