using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ObjectSelector: MonoBehaviour
{
    public static ObjectSelector Instance { get; private set; }
    [SerializeField] ScreenAndTouchManager touchManager;

    [Header("AbilityIcon")]
    [SerializeField] AbilityIconUI abilityIconPrefab;
    [SerializeField] Transform abilityGrid;
    [Header("DeselectButton")]
    [SerializeField] GameObject deselectIcon;
    [SerializeField] Button deselectButton;


    public Vector2Int selectedGrid => touchManager.SelectedGrid;
    public Pawn selectedPawn { get; private set; }
    public WorldObject selectedObject { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        deselectButton.onClick.AddListener(DeselectObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectObject(WorldObject worldObject)
    {
        if (selectedObject != null)
        {
            selectedObject.SetSelected(false, 2);
            //if (selectedPawn != null)
            //{
            //    selectedPawn.StopControlPawn();
            //}
        }
        selectedObject = worldObject;
        selectedObject.SetSelected(true, 2);
        selectedPawn = selectedObject as Pawn;
        ShowAbilities(selectedObject);
        deselectIcon.SetActive(true);
    }

    public void ShowAbilities(WorldObject @object)
    {
        // Clear old UI
        RemoveGridAbilities();

        foreach (Ability ability in @object.abilities)
        {
            AbilityIconUI icon =
                Instantiate(abilityIconPrefab, abilityGrid);

            icon.Setup(ability, @object);
        }
    }
    public void DeselectObject()
    {
        if (deselectIcon != null) deselectIcon.SetActive(false);
        if (selectedObject != null) selectedObject.SetSelected(false, 2);
        selectedPawn = null;
        selectedObject = null;
        RemoveGridAbilities();
    }
    public void RemoveGridAbilities()
    {
        if (abilityGrid == null) return;
        foreach (Transform child in abilityGrid)
        {
            Destroy(child.gameObject);
        }
    }

    public void HandleTap(Vector2Int pos)
    {
        if (selectedObject == null) return;

        if (selectedPawn)
        {
            if (selectedPawn.PawnState == PawnState.Controlled)
            {
                selectedPawn.MakePathContinuous(pos, PawnManager.Instance.GetWTS()).Forget();
            }
            else
            {
                DeselectObject();
            }
            return;
        }
        DeselectObject();

        //Debug.Log($"Tapped grid: {gridPos}");

        // TODO:
        // Select pawn
        // Place building
        // Open UI
    }


    void LateUpdate()
    {
        if (selectedPawn != null)
        {
            PathDrawer.Instance.DrawPath(selectedPawn.CurrentWorldPos, selectedPawn.Paths);
        }
    }
}
