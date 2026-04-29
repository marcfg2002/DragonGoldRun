using UnityEngine;
using System.Collections;

public class Enemigo : MonoBehaviour
{
    public float velocidad = 2f;
    public float limiteIzq = -11f;
    public float limiteDer = 11f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool moviendoDerecha = true;
    private bool estaMuerto = false;

    [HideInInspector] public GeneradorEnemigo spawner;
    [HideInInspector] public Vector3 posDestiXarxa;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (rb != null) rb.freezeRotation = true;
        if (spawner == null) spawner = FindObjectOfType<GeneradorEnemigo>();
        
        posDestiXarxa = transform.position;
    }

    void FixedUpdate()
    {
        if (rb == null || estaMuerto) return;

        if (GestorXarxa.Instance != null && !GestorXarxa.Instance.esServidor) 
        {
            transform.position = Vector3.Lerp(transform.position, posDestiXarxa, Time.deltaTime * 15f);
            return;
        }

        if (moviendoDerecha)
        {
            rb.velocity = new Vector2(velocidad, rb.velocity.y);
            if (sr != null) sr.flipX = true;
            if (transform.position.x >= limiteDer) moviendoDerecha = false;
        }
        else
        {
            rb.velocity = new Vector2(-velocidad, rb.velocity.y);
            if (sr != null) sr.flipX = false;
            if (transform.position.x <= limiteIzq) moviendoDerecha = true;
        }
    }

    public void MorirPerXarxa()
    {
        if (!estaMuerto)
        {
            if (spawner != null) spawner.SolicitarReaparicion();
            StartCoroutine(SecuenciaMuerte());
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (estaMuerto) return;
        if (!collision.collider.CompareTag("Player")) return;

        ScriptPersonaje script = collision.collider.GetComponent<ScriptPersonaje>();
        
        if (script == null || !script.esLocal) return;

        float yPieJugador = collision.collider.bounds.min.y;
        float yCentroEnemigo = col.bounds.center.y;
        bool ataqueAereo = yPieJugador > (yCentroEnemigo + 0.1f);

        if (ataqueAereo)
        {
            Rigidbody2D rbJugador = collision.collider.GetComponent<Rigidbody2D>();
            if (rbJugador != null)
            {
                rbJugador.velocity = new Vector2(rbJugador.velocity.x, 0);
                rbJugador.AddForce(Vector2.up * 6f, ForceMode2D.Impulse);
            }
            
            MorirPerXarxa();
            GestorXarxa.Instance.EnviarDades("KILL_ENEM");
        }
        else
        {
            script.RecibirDaño(transform, col);
        }
    }

    IEnumerator SecuenciaMuerte()
    {
        estaMuerto = true;
        if (col != null) col.enabled = false;
        if (rb != null) rb.simulated = false;

        Vector3 escalaOriginal = transform.localScale;
        transform.localScale = new Vector3(escalaOriginal.x * 1.2f, escalaOriginal.y * 0.2f, escalaOriginal.z);
        if (CameraShake.Instance != null) CameraShake.Instance.Sacudir(0.1f, 0.2f);

        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }
}