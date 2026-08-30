using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectFinder : MonoBehaviour
{
    [SerializeField] WorldMap world;
    [SerializeField] ObjectSelector objectSelector;
    [SerializeField] ObjectIconButton worldObjectIcon;
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Transform objectGridPanel;

    Dictionary<WorldObject, GameObject> pawnIconsFinder = new();
    //Dictionary<Vector2Int, List<WorldObject>> objectsPerTile;
    //static readonly Vector2Int[] searchOffsets =
    //{
    //    new(-1, 1), new(0, 1), new(1, 1),
    //    new(-1, 0), new(0, 0), new(1, 0),
    //    new(-1,-1), new(0,-1), new(1,-1),
    //};

    private void OnTriggerEnter2D(Collider2D collision)
    {
        WorldObject @object = collision.GetComponent<WorldObject>();
        CreateIcon(@object);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        WorldObject @object = collision.GetComponent<WorldObject>();
        RemoveIcon(@object);
    }

    //float scanTimer;
    //void Update()
    //{
    //    scanTimer += Time.deltaTime;

    //    if (scanTimer < 0.25f) return;

    //    scanTimer = 0;
    //    UpdateNearbyObjects(selectGridPos);
    //}

    //readonly List<WorldObject> removeBuffer = new();
    //void UpdateNearbyObjects(Vector2Int center)
    //{
    //    HashSet<WorldObject> found = new();

    //    foreach (var offset in searchOffsets)
    //    {
    //        Vector2Int pos = center + offset;

    //        WorldObject obj = world.GetObjectAtPosition(pos);

    //        if (obj != null) found.Add(obj);
    //    }

    //    SyncIcons(found);
    //}
    //void SyncIcons(HashSet<WorldObject> found)
    //{
    //    removeBuffer.Clear();

    //    foreach (var pair in pawnIconsFinder)
    //    {
    //        if (!found.Contains(pair.Key))
    //            removeBuffer.Add(pair.Key);
    //    }

    //    foreach (var obj in removeBuffer)
    //        RemoveIcon(obj);

    //    foreach (var obj in found)
    //    {
    //        if (!pawnIconsFinder.ContainsKey(obj))
    //            CreateIcon(obj);
    //    }
    //}
    void CreateIcon(WorldObject @object)
    {
        if (@object == null)
        {
            Debug.Log("class: ObjectFinder, void CreateIcon");
            return;
        }
        worldObjectIcon.worldObject = @object;
        image.sprite = @object.IconSprite;
        text.text = @object.ThingName;
        GameObject icon = Instantiate(worldObjectIcon.gameObject, objectGridPanel);
        Instantiate(text.gameObject, icon.transform);
        pawnIconsFinder[@object] = icon;
        Pawn p = @object as Pawn;
        if (p != null)
        {
            p.SetSelectThreshold(true, 1);
        }
    }
    void RemoveIcon(WorldObject @object)
    {
        if (@object == null) return;
        Pawn p = @object as Pawn;
        if (p != null)
        {
            p.SetSelectThreshold(false, 1);
        }
        if (!pawnIconsFinder.TryGetValue(@object, out GameObject icon))
            return;

        Destroy(icon);
        pawnIconsFinder.Remove(@object);
    }
}