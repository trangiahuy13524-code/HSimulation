using System.Collections.Generic;
using UnityEngine;

public partial class Pawn : WorldObject
{
    [SerializeField] float idleTime = 2f;
    [SerializeField] float currentIdleTime = 0f;
    public byte selectThreshHold = 0;

    public bool onDuty => currentJob != null || isControlled;
    public string taskID;
    protected override void Start()
    {
        displayTextName.sortingOrder = 1000;
        transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);
        paths = new Queue<Vector2Int>();
        oldGridPos = currentGridPos;
        oldDestination = currentGridPos;
        world.ModifyPawnCountGrid(currentGridPos, true);
        UpdateLayer();
        ObjectName = "Noob";
        currentJob = null;
    }

    private void Update()
    {
        TickUpdate();
    }

    void TickUpdate(byte speed = 1)
    {
        bool donePathing = Move(speed);

        if (isControlled)
        {
            return;
        }

        if (donePathing)
        {
            if (currentJob != null)
            {
                reachDestination = true;
                //ChangeDirection(currentJob.workBuilding.direction);
            }

            if (currentIdleTime < idleTime)
            {
                currentIdleTime += Time.deltaTime * speed;
            }
            else
            {
                currentIdleTime = 0f;
                bool jobExisted = TryFindJob();

                if (jobExisted)
                {
                    reachDestination = false;
                    destinationInvalid = false;
                    currentJob.workBuilding.PerformWork(this);
                    return;
                }
                MakePath(GetRandomPosition());
            }
        }

        //if (currentJob != null)
        //{
        //    currentJob.workBuilding.DoTask(this, taskID);
        //}
    }

    protected override void OnDestroy()
    {
        if (world != null) world.ModifyPawnCountGrid(currentGridPos, false);
        ReturnJob();
        base.OnDestroy();
    }

    public override void SetSelected(bool value)
    {
        base.SetSelected(value);
        SetSelectThreshold(value);
    }

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
