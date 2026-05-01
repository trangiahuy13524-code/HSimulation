using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PawnFinder : MonoBehaviour
{
    [SerializeField] GameObject pawnIcon;
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Transform objectGridPanel;

    Dictionary<Pawn, GameObject> pawnIconsFinder = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Pawn pawn = collision.GetComponent<Pawn>();
        if (pawn == null) return;
        if (image) image.sprite = pawn.IconSprite;
        if (text) text.text = pawn.displayName;
        GameObject icon = Instantiate(pawnIcon, objectGridPanel);
        Instantiate(text.gameObject, icon.transform);
        pawn.hightlight.SetActive(true);
        pawnIconsFinder[pawn] = icon;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Pawn pawn = collision.GetComponent<Pawn>();
        if (pawn == null) return;
        pawn.hightlight.SetActive(false);
        if (!pawnIconsFinder.TryGetValue(pawn, out GameObject icon))
            return;

        Destroy(icon);
        pawnIconsFinder.Remove(pawn);
    }
}