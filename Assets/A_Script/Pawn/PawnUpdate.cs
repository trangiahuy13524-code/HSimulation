using System.Collections.Generic;
using UnityEngine;

public partial class Pawn : WorldObject
{
    [SerializeField] float idleTime = 2f;
    [SerializeField] float currentIdleTime = 0f;

    public bool onDuty => currentJob != null;

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
        bool donePathing = Move();


        if (donePathing)
        {
            if (currentJob != null)
            {
                currentJob.workBuilding.PerformWork(this, currentJob);
            }
            bool jobExisted = TryFindJob();
            if (jobExisted)
            {
                return;
            }
            if (currentIdleTime < idleTime)
            {
                currentIdleTime += Time.deltaTime;
            }
            else
            {
                currentIdleTime = 0f;
                MakePath(GetRandomPosition());
            }
        }
    }
}
