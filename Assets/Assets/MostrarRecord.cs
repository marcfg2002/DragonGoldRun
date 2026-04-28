using UnityEngine;
using TMPro;

public class MostrarRecord : MonoBehaviour
{
    public TMP_Text textoRecord;

    void Update()
    {
        if (GameManager.Instance != null && textoRecord != null)
        {
            textoRecord.text = "Record: " + GameManager.Instance.maxMonedasRecord;
        }
    }
}