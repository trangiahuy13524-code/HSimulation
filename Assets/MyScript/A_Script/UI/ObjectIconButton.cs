using UnityEngine;
using UnityEngine.UI;

public class ObjectIconButton : MonoBehaviour
{
    [SerializeField] ScreenAndTouchManager screenAndTouchManager;
    [SerializeField] Button button;
    public WorldObject worldObject;

    
    private void Start()
    {
        screenAndTouchManager = ScreenAndTouchManager.Instance;
        button.onClick.AddListener(SelectObject);
    }

    public void SelectObject()
    {
        screenAndTouchManager.SelectObject(worldObject);
    }

    
}
