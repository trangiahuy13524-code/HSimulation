using System.Collections.Generic;
using UnityEngine;

public class PawnFinder : MonoBehaviour
{
    [SerializeField] GameObject pawnIcon;
    [SerializeField] Transform objectGridPanel;

    Dictionary<Pawn, GameObject> pawnIconsFinder = new();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Pawn pawn = collision.GetComponent<Pawn>();
        if (pawn == null) return;
        GameObject icon =
            Instantiate(pawnIcon, objectGridPanel);
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