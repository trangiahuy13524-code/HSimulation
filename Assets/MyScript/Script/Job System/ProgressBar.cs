using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] Image fill;

    Transform target;
    [SerializeField] float heightOffset = 0f;

    public void Setup(Transform followTarget, float heightOffset)
    {
        target = followTarget;
        this.heightOffset = heightOffset;
        transform.position = target.position + Vector3.up * heightOffset;
    }

    public void SetProgress(float value)
    {
        fill.fillAmount = Mathf.Clamp01(value);
    }

    //void LateUpdate()
    //{
    //    if (target != null)
    //        transform.position = target.position + Vector3.up * heightOffset;
    //}
}