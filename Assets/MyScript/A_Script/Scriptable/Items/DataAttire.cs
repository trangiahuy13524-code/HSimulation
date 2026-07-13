using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item/Attire/AttireData")]
public class DataAttire : DataItem
{
    [Header("Attire")]
    public BodyTag bodyTag = BodyTag.OffBody;
    public int wearingTime = 10;
    public PartAttire attirePart;

    public override bool IsStackable => false;
}