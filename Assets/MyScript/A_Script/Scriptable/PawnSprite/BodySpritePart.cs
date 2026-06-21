using UnityEngine;



[CreateAssetMenu(menuName = "Game/PawnSprite/BodySpritePart")]
public class BodySpritePart : SpritePart
{
    [Header("Base Stats")]
    public int baseHealth = 1;
    public int baseMana = 0;
    public int baseDefense = 0;
    public int baseAttack = 1;
    public float magicPotential = 0f;
    public float baseSpeed = 1f;
}