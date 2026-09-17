using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectFinder : MonoBehaviour
{
    [SerializeField] WorldMap world;
    [SerializeField] ObjectSelector objectSelector;
    [SerializeField] ScreenAndTouchManager touchManager;
    [SerializeField] ObjectIconButton worldObjectIcon;
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Transform objectGridPanel;

    Dictionary<WorldObject, ObjectIconButton> dynamicIconsFinder = new();
    Dictionary<WorldObject, ObjectIconButton> staticIconsFinder = new();

    public static ObjectFinder Instance { get; private set; }
    static readonly Vector2Int[] searchOffsets =
    {
        //new(-1, 1), new(0, 1), new(1, 1),
        //new(-1, 0),
        new(0, 0),
        //new(1, 0),
        //new(-1,-1), new(0,-1), new(1,-1),
    };

    private void OnTriggerEnter2D(Collider2D collision)
    {
        WorldObject @object = collision.GetComponent<WorldObject>();
        CreateIcon(@object, false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        WorldObject @object = collision.GetComponent<WorldObject>();
        RemoveIcon(@object, false);
    }

    float scanTimer;
    private void Start()
    {
        Instance = this;
    }
    void Update()
    {
        scanTimer += Time.deltaTime;

        if (scanTimer < 0.25f) return;

        scanTimer = 0;
        UpdateNearbyObjects(touchManager.SelectedGrid);
    }

    readonly List<WorldObject> removeBuffer = new();
    readonly List<WorldObject> destroyedKeys = new();
    void UpdateNearbyObjects(Vector2Int center)
    {

        foreach (var pair in staticIconsFinder)
        {
            if (pair.Key == null)
            {
                Destroy(pair.Value.gameObject);
                destroyedKeys.Add(pair.Key);
            }
        }

        foreach (var key in destroyedKeys)
        {
            staticIconsFinder.Remove(key);
        }

        HashSet<WorldObject> found = new();

        foreach (var offset in searchOffsets)
        {
            Vector2Int pos = center + offset;

            WorldObjectStatic obj = world.GetObjectAtPosition(pos);

            if (obj != null) found.Add(obj);
        }

        SyncIcons(found);
    }
    void SyncIcons(HashSet<WorldObject> found)
    {
        removeBuffer.Clear();

        foreach (var pair in staticIconsFinder)
        {
            if (!found.Contains(pair.Key))
                removeBuffer.Add(pair.Key);
        }

        foreach (var obj in removeBuffer)
            RemoveIcon(obj, true);

        foreach (var obj in found)
        {
            if (!staticIconsFinder.ContainsKey(obj))
                CreateIcon(obj, true);
        }
    }
    void CreateIcon(WorldObject @object, bool @static)
    {
        Dictionary<WorldObject, ObjectIconButton> iconsFinder = @static ? staticIconsFinder : dynamicIconsFinder;

        if (@object == null)
        {
            Debug.Log("class: ObjectFinder, void CreateIcon");
            return;
        }
        image.sprite = @object.IconSprite;
        text.text = @object.ThingName;
        ObjectIconButton icon = Instantiate(worldObjectIcon, objectGridPanel);
        icon.worldObject = @object;
        Instantiate(text.gameObject, icon.transform);
        iconsFinder[@object] = icon;
        @object.SetSelectedThreshold(true, 1);
    }
    public void RemoveIcon(WorldObject @object, bool @static)
    {
        Dictionary<WorldObject, ObjectIconButton> iconsFinder = @static ? staticIconsFinder : dynamicIconsFinder;

        if (@object == null) return;
        @object.SetSelectedThreshold(false, 1);
        if (!iconsFinder.TryGetValue(@object, out ObjectIconButton icon))
        {
            Debug.Log("class: ObjectFinder, void RemoveIcon");
            return;
        }
            

        if (icon != null) Destroy(icon.gameObject);
        iconsFinder.Remove(@object);
    }
}