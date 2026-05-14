using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] Image fill;

    Transform target;

    public void Setup(Transform followTarget)
    {
        target = followTarget;
    }

    public void SetProgress(float value)
    {
        fill.fillAmount = Mathf.Clamp01(value);
    }

    void LateUpdate()
    {
        if (target != null)
            transform.position = target.position + Vector3.up * 1.2f;
    }
}