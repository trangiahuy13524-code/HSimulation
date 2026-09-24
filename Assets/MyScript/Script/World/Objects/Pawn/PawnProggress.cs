using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Pawn
{
    private const float WORK_PROGRESS_PER_SECOND = 20f;
    private const float PROGRESS_BAR_VERTICAL_OFFSET = 0.5f;
    [Header("Progress Bar")]
    [SerializeField] ProgressBar progressBarPrefab;
    private ProgressBar progressBarInstance;

    private ProgressBar CreateProgressBar()
    {
        progressBarInstance = Instantiate(progressBarPrefab, WorldCanvasUI.Instance.transform);
        progressBarInstance.Setup(transform, PROGRESS_BAR_VERTICAL_OFFSET);
        return progressBarInstance;
    }

    private void DestroyProgressBar()
    {
        DestroyProgressBar(progressBarInstance);
    }

    private void DestroyProgressBar(ProgressBar progressBar)
    {
        if (progressBar != null)
        {
            Destroy(progressBar.gameObject);
        }

        if (progressBarInstance == progressBar)
        {
            progressBarInstance = null;
        }
    }
}
