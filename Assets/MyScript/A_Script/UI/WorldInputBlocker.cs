using UnityEngine;

public class WorldInputBlocker : MonoBehaviour, IBlocksWorldInput
{
    [SerializeField] bool blockInput = true;

    public bool BlocksWorldInput()
    {
        return blockInput;
    }
}