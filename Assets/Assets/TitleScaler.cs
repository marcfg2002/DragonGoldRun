using UnityEngine;
using System.Collections;

public class TitleScaler : MonoBehaviour
{
    private float scaleAmount = 0.05f;
    private float scaleSpeed = 2f;

    private Vector3 minScale;
    private Vector3 maxScale;

    void Start()
    {
        Vector3 baseScale = transform.localScale;
        minScale = baseScale * (1f - scaleAmount);
        maxScale = baseScale * (1f + scaleAmount);

        StartCoroutine(ScaleAnimation());
    }

    IEnumerator ScaleAnimation()
    {
        while (true)
        {
            yield return ScaleOverTime(minScale, maxScale);
            yield return ScaleOverTime(maxScale, minScale);
        }
    }

    IEnumerator ScaleOverTime(Vector3 startScale, Vector3 endScale)
    {
        float timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * scaleSpeed;
            transform.localScale = Vector3.Lerp(startScale, endScale, timer);
            yield return null;
        }
        transform.localScale = endScale;
    }
}