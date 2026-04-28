using UnityEngine;
using TMPro;

public class MonedasManager : MonoBehaviour
{
    [Header("Monedas")]
    public GameObject prefabMoneda;
    public int cantidadMonedas = 4;
    public Vector2 limiteX = new Vector2(-10f, 10f);
    public Vector2 limiteY = new Vector2(-3f, 3f);

    [Header("Contador")]
    public TMP_Text textoMonedas;
    public int monedas = 0;

    public bool activo = false;
    private int oleada = 0;

    void Start()
    {
        ActualizarTexto();
    }

    public void Activar()
    {
        activo = true;
        GenerarMonedas();
    }

    void Update()
    {
        if (!activo) return;

        if (GameObject.FindGameObjectsWithTag("Moneda").Length == 0)
        {
            GenerarMonedas();
        }
    }

    void GenerarMonedas()
    {
        oleada++;
        Random.InitState(oleada * 12345);

        for (int i = 0; i < cantidadMonedas; i++)
        {
            float x = Random.Range(limiteX.x, limiteX.y);
            float y = Random.Range(limiteY.x, limiteY.y);
            Vector2 posicion = new Vector2(x, y);

            GameObject moneda = Instantiate(prefabMoneda, posicion, Quaternion.identity);
            moneda.tag = "Moneda";
            moneda.AddComponent<MonedaSimple>().manager = this;
        }
    }

    public void SumarMonedaLocal()
    {
        monedas++;
        ActualizarTexto();

        if (GameManager.Instance != null) GameManager.Instance.VerificarRecordMonedas(monedas);

        if (monedas >= 40)
        {
            Debug.Log("40 Monedas conseguidas! Has guanyat!");
            GestorXarxa.Instance.EnviarDades("VICTORIA|JO");
            if (GameManager.Instance != null) GameManager.Instance.DesbloquearSiguienteNivel(1);
        }
    }

    public void ActualizarTexto()
    {
        if (textoMonedas != null) textoMonedas.text = monedas.ToString();
    }
}

public class MonedaSimple : MonoBehaviour
{
    public MonedasManager manager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ScriptPersonaje sp = other.GetComponent<ScriptPersonaje>();
            if (sp != null && sp.esLocal)
            {
                manager.SumarMonedaLocal();
                GestorXarxa.Instance.EnviarDades("COIN|" + transform.position.x + "|" + transform.position.y);
                Destroy(gameObject);
            }
        }
    }
}