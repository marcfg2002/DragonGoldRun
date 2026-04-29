using UnityEngine;

public class DragonFly : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 1.5f;
    public float limiteIzquierdo = -11f;
    public float limiteDerecho = 11f;
    public float alturaVuelo = 3f;
    public float amplitud = 0.5f;
    public float frecuencia = 2f;

    [Header("Disparo")]
    public GameObject fireballPrefab;
    public Transform puntoDisparo;
    private float tiempoProximoDisparo;

    private bool moviendoDerecha = false;
    private Vector3 escalaOriginal;
    private float tiempo = 0f;
    
    public bool activo = false;
    
    [HideInInspector] public Vector3 posDestiXarxa;
    [HideInInspector] public float scaleXDesti;

    void Start()
    {
        escalaOriginal = transform.localScale * 0.8f;
        transform.localScale = escalaOriginal;
        moviendoDerecha = false;
        Girar(false);
        
        posDestiXarxa = transform.position;
        scaleXDesti = transform.localScale.x;
    }

    void Update()
    {
        if (!activo) return;

        if (GestorXarxa.Instance != null)
        {
            if (GestorXarxa.Instance.esServidor)
            {
                MoverDragon(); 
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, posDestiXarxa, Time.deltaTime * 15f);
                Vector3 s = transform.localScale;
                s.x = scaleXDesti;
                transform.localScale = s;
            }
        }
        else 
        {
            MoverDragon(); 
        }
        
        DispararSiToca();
    }

    void MoverDragon()
    {
        tiempo += Time.deltaTime;
        float direccion = moviendoDerecha ? 1 : -1;

        float nuevaY = alturaVuelo + Mathf.Sin(tiempo * frecuencia) * amplitud;
        transform.Translate(Vector2.right * direccion * velocidad * Time.deltaTime);
        transform.position = new Vector3(transform.position.x, nuevaY, transform.position.z);

        if (transform.position.x >= limiteDerecho && moviendoDerecha) Girar(false);
        else if (transform.position.x <= limiteIzquierdo && !moviendoDerecha) Girar(true);
    }

    void Girar(bool haciaDerecha)
    {
        moviendoDerecha = haciaDerecha;
        Vector3 nuevaEscala = escalaOriginal;
        nuevaEscala.x = haciaDerecha ? -Mathf.Abs(escalaOriginal.x) : Mathf.Abs(escalaOriginal.x);
        transform.localScale = nuevaEscala;
    }

    void DispararSiToca()
    {
        if (Time.time >= tiempoProximoDisparo)
        {
            if (fireballPrefab != null && puntoDisparo != null)
                Instantiate(fireballPrefab, puntoDisparo.position, Quaternion.identity);
            
            tiempoProximoDisparo = Time.time + Random.Range(2f, 3f);
        }
    }
}