using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public partial class Pawn : IManagedUpdate
{
    [Header("Pawn Update")]
    [SerializeField] float idleTime = 2f;
    [SerializeField] float currentIdleTime = 0f;
    [SerializeField] PawnState currentState = PawnState.Idle;
    public PawnState PawnState => currentState;
    public bool onDuty => currentState == PawnState.Working || currentState == PawnState.Controlled;
    protected override void Start()
    {
        displayTextName.sortingOrder = 2000;
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        paths = new Queue<Vector2Int>();
        oldGridPos = currentGridPos;
        oldDestination = currentGridPos;
        world.ModifyPawnCountGrid(currentGridPos, true);
        UpdateLayer();
        //ObjectName = "Noob";
        //currentJob = null;
        PawnManager.Register(this);
    }

    public void ManagedUpdate(WorldThreadSafe worldTS)
    {
        TickUpdate(worldTS);
    }

    public bool debug = false;
    void TickUpdate(WorldThreadSafe worldTS, byte speed = 1)
    {
        if (thinking)
            return;

        bool donePathing = Move(speed, worldTS);

        if (currentState == PawnState.Controlled)
            return;

        if (!donePathing)
            return;

        

        // reached movement destination
        if (currentState == PawnState.Working)
        {
            reachDestination = true;
            return;
        }

        // idle timer
        if (currentIdleTime < idleTime)
        {
            currentIdleTime += Time.deltaTime * speed;
            return;
        }

        // async think
        if (thinkCTS == null)
        {
            thinkCTS = new CancellationTokenSource();

            ThinkAsync(worldTS, thinkCTS.Token).Forget();
        }
    }

    public static bool aPawnThoughtThisFrame = false;
    CancellationTokenSource thinkCTS;
    bool thinking = false;
    public void CancelThink()
    {
        thinkCTS?.Cancel();
        thinkCTS = null;
        thinking = false;
    }
    async UniTaskVoid ThinkAsync(WorldThreadSafe worldTS, CancellationToken token)
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

            // IMPORTANT:
            // await job
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

    //public override void Despawn()
    //{
        
    //    base.Despawn();
    //}

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

    public override void SetSelected(bool value)
    {
        base.SetSelected(value);
        SetSelectThreshold(value);
    }

    public byte selectThreshHold = 0;
    public void SetSelectThreshold(bool value)
    {
        if (value)
        {
            selectThreshHold++;
        }
        else
        {
            selectThreshHold--;
        }
        if (selectThreshHold > 0)
        {
            hightlight.SetActive(true);
        }
        else
        {
            hightlight.SetActive(false);
        }
    }
}
