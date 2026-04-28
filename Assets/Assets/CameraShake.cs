using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 posOriginal;
    private bool isShaking = false;

    void Awake()
    {
        Instance = this;
        posOriginal = transform.localPosition;
    }

    public void Sacudir(float duracion, float magnitud)
    {
        if (!isShaking)
        {
            StartCoroutine(DoShake(duracion, magnitud));
        }
    }

    IEnumerator DoShake(float duracion, float magnitud)
    {
        isShaking = true;

        float transcurrido = 0f;

        while (transcurrido < duracion)
        {
            float x = Random.Range(-1f, 1f) * magnitud;
            float y = Random.Range(-1f, 1f) * magnitud;

            transform.localPosition = new Vector3(posOriginal.x + x, posOriginal.y + y, posOriginal.z);

            transcurrido += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = posOriginal;
        isShaking = false;
    }
}