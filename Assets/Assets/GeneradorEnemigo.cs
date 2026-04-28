using UnityEngine;
using System.Collections;

public class GeneradorEnemigo : MonoBehaviour
{
    public GameObject prefabEnemigo;
    public float retrasoInicial = 0.5f;
    public float retrasoReaparicion = 1f;

    private bool esperando = false;
    private Vector3 posicionFija = new Vector3(11f, -1.37f, 0f);

    void Start()
    {
    }

    public void Activar()
    {
        if (GameManager.Instance != null && GameManager.Instance.cargandoDesdeSave) return;
        StartCoroutine(GenerarConRetraso(retrasoInicial));
    }

    public void SolicitarReaparicion()
    {
        if (!esperando) StartCoroutine(GenerarConRetraso(retrasoReaparicion));
    }

    private IEnumerator GenerarConRetraso(float delay)
    {
        esperando = true;
        yield return new WaitForSeconds(delay);
        GenerarEnemigo();
        esperando = false;
    }

    private void GenerarEnemigo()
    {
        GameObject nuevo = Instantiate(prefabEnemigo, posicionFija, Quaternion.identity);
        Enemigo scriptEnemigo = nuevo.GetComponent<Enemigo>();
        if (scriptEnemigo != null) scriptEnemigo.spawner = this;
    }
}