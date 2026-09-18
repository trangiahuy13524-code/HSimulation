using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public partial class Pawn : IManagedUpdate
{
    private const int DISPLAY_NAME_SORTING_ORDER = 2000;

    [Header("Pawn Update")]
    [SerializeField] float idleTime = 2f;
    [SerializeField] float currentIdleTime;
    [SerializeField] PawnState currentState = PawnState.Idle;

    private CancellationTokenSource thinkCTS;
    private bool thinking;

    public PawnState PawnState => currentState;
    public bool onDuty => currentState == PawnState.Working || currentState == PawnState.Controlled;

    protected override void Start()
    {
        displayTextName.sortingOrder = DISPLAY_NAME_SORTING_ORDER;
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        paths = new Queue<Vector2Int>();
        oldGridPos = currentGridPos;
        oldDestination = currentGridPos;
        world.ModifyPawnCountGrid(currentGridPos, true);
        UpdateLayer();
        PawnManager.Register(this);
    }

    public void ManagedUpdate(WorldThreadSafe worldTS)
    {
        TickUpdate(worldTS);
    }

    private void TickUpdate(WorldThreadSafe worldTS, byte speed = 1)
    {
        if (thinking) return;

        bool donePathing = Move(speed, worldTS);
        if (currentState == PawnState.Controlled || !donePathing) return;

        if (currentState == PawnState.Working)
        {
            reachDestination = true;
            return;
        }

        if (currentIdleTime < idleTime)
        {
            currentIdleTime += Time.deltaTime * speed;
            return;
        }

        if (thinkCTS != null) return;

        thinkCTS = new CancellationTokenSource();
        ThinkAsync(worldTS, thinkCTS.Token).Forget();
    }

    public static bool aPawnThoughtThisFrame = false;

    public void CancelThink()
    {
        thinkCTS?.Cancel();
        thinkCTS = null;
        thinking = false;
    }

    private async UniTaskVoid ThinkAsync(WorldThreadSafe worldTS, CancellationToken token)
    {
        if (aPawnThoughtThisFrame)
        {
            thinkCTS = null;
            return;
        }
        thinking = true;
        aPawnThoughtThisFrame = true;

        bool jobExisted = await TryFindJob(token);

        currentIdleTime = 0f;

        if (jobExisted)
        {
            currentState = PawnState.Working;

            jobCTS = new CancellationTokenSource();
            currentJob.DoJob(this, jobCTS.Token).Forget();
        }
        else
        {
            await MakePath(GetRandomPosition(), worldTS);
        }
        thinkCTS.Dispose();
        thinkCTS = null;
        thinking = false;
    }

    protected override void OnDestroy()
    {
        PawnManager.Unregister(this);
        DropAllItemsInventory(currentGridPos, null);
        if (world != null) world.ModifyPawnCountGrid(currentGridPos, false);
        ReturnJob();
        jobCTS?.Cancel();
        jobCTS?.Dispose();
        thinkCTS?.Cancel();
        thinkCTS?.Dispose();
        if (progressBarInstance != null) Destroy(progressBarInstance.gameObject);
        base.OnDestroy();
    }

    public void SetPawnMaterial(Material mat)
    {
        bodyData.SetMaterial(mat);
        if (headData) headData.SetMaterial(mat);
        if (hairData) hairData.SetMaterial(mat);
    }
}
