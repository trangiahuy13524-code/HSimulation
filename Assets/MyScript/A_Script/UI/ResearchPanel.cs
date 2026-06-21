using UnityEngine;
using System.Collections.Generic;

public class ResearchPanel : MonoBehaviour
{
    public static ResearchPanel Instance { get; private set; }

    public RectTransform contentTransform;
    public ResearchButtonUI researchButtonPrefab;
    public GameObject researchPanel;

    JobResearch curJob;

    [Header("Spacing")]
    public float xSpacing = 200f;
    public float ySpacing = 120f;
    [Header("Offset")]
    public float startX = 150f;
    public float startY = -50f;

    private Dictionary<ResearchNode, float> nodeX = new();
    float minX, maxX, minY, maxY;

    void Start()
    {
        Instance = this;
        gameObject.SetActive(false);
        minX = float.MaxValue;
        maxX = float.MinValue;
        minY = float.MaxValue;
        maxY = float.MinValue;
    }

    public void CreateResearchUI(BuildingResearch buildingResearch)
    {
        foreach (Transform child in contentTransform)
            Destroy(child.gameObject);

        nodeX.Clear();

        float x = 0;
        foreach (var root in buildingResearch.researchTree.mainNodes)
        {
            AssignX(root, ref x);
        }

        foreach (var root in buildingResearch.researchTree.mainNodes)
        {
            Draw(root, 0, buildingResearch);
        }

        ApplyContentSize();
    }

    // 🔥 Step 1: assign X based on leaf order
    float AssignX(ResearchNode node, ref float x)
    {
        if (node.children == null || node.children.Count == 0)
        {
            float px = x;
            x += 1;
            nodeX[node] = px;
            return px;
        }

        float sum = 0;
        foreach (var child in node.children)
        {
            sum += AssignX(child, ref x);
        }

        float center = sum / node.children.Count;
        nodeX[node] = center;
        return center;
    }

    // 🔥 Step 2: draw using computed positions
    void Draw(ResearchNode node, int depth, BuildingResearch buildingResearch)
    {
        Vector2 pos = new Vector2(
            startX + nodeX[node] * xSpacing,
            startY - depth * ySpacing
        );

        // 📦 track bounds
        if (pos.x < minX) minX = pos.x;
        if (pos.x > maxX) maxX = pos.x;
        if (pos.y < minY) minY = pos.y;
        if (pos.y > maxY) maxY = pos.y;

        CreateButton(node, buildingResearch, pos.x, pos.y);

        if (node.children == null) return;

        foreach (var child in node.children)
            Draw(child, depth + 1, buildingResearch);
    }

    void CreateButton(
        ResearchNode node,
        BuildingResearch buildingResearch,
        float x,
        float y)
    {
        ResearchButtonUI buttonUI =
            Instantiate(researchButtonPrefab, contentTransform);

        buttonUI.transform.localPosition = new Vector2(x, y);

        buttonUI.SetData(node.researchData);

        buttonUI.button.onClick.AddListener(() =>
        {
            if (curJob != null)
                curJob.externalRemoved = true;

            curJob = buildingResearch.CreateJob(node.researchData);
        });
    }

    void ApplyContentSize()
    {
        RectTransform rt = contentTransform;

        float width = maxX;
        float height = maxY - minY;

        // Add padding so nodes aren't clipped
        float padding = 200f;

        rt.sizeDelta = new Vector2(
            width + padding,
            height + padding
        );
    }
}