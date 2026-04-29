using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ScriptPersonaje : MonoBehaviour
{
    public bool esLocal = true;

    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 12f;

    [Header("Game Feel")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private bool esInvulnerable = false;
    private bool controlBloqueado = false;

    [Header("Detección de suelo")]
    public float distanciaSuelo = 0.3f;
    public LayerMask capaSuelo;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private CircleCollider2D col;

    private bool enSuelo;
    private bool estabaEnSuelo;
    private float mov;

    [Header("Salto múltiple")]
    public int maxSaltos = 2;
    private int saltosHechos = 0;

    [Header("Límites de pantalla")]
    public float limiteIzq = -8f;
    public float limiteDer = 8f;
    public float limiteAbajo = -4.5f;
    public float limiteArriba = 4.5f;

    [Header("Sistema de vidas")]
    public int vidasMaximas = 3;
    private int vidasActuales;
    private Vector3 puntoReaparicion;

    [Header("Visual Effects")]
    private Vector3 escalaOriginal;
    private Texture2D texturaRoja;
    private float alfaFlash = 0f;

    [Header("UI")]
    public TMP_Text textoVidas;
    public TMP_Text textoGameOver;
    public float tiempoEsperaGameOver = 4f;

    [Header("REFERÈNCIES PER GUARDAR/CARREGAR")]
    public GameObject prefabEnemigo;
    public GameObject prefabMoneda;
    public GameObject dragonSceneObject;

    private MonedasManager monedasManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<CircleCollider2D>();

        escalaOriginal = transform.localScale;

        texturaRoja = new Texture2D(1, 1);
        texturaRoja.SetPixel(0, 0, new Color(1, 0, 0, 0.5f));
        texturaRoja.Apply();

        monedasManager = FindObjectOfType<MonedasManager>();
        rb.freezeRotation = true;
        if (capaSuelo == 0) capaSuelo = LayerMask.GetMask("Ground");

        vidasActuales = vidasMaximas;
        puntoReaparicion = transform.position;

        if (textoVidas == null)
        {
            GameObject txtObj = GameObject.Find("TextoVidas");
            if (txtObj != null) textoVidas = txtObj.GetComponent<TMP_Text>();
        }

        if (textoGameOver == null)
        {
            GameObject goTxt = GameObject.Find("TextoGameOver");
            if (goTxt != null) textoGameOver = goTxt.GetComponent<TMP_Text>();
        }

        ActualizarTextoVidas();

        if (textoGameOver != null) textoGameOver.gameObject.SetActive(false);
        Time.timeScale = 1f;

        if (dragonSceneObject == null)
        {
            DragonFly df = FindObjectOfType<DragonFly>();
            if (df != null) dragonSceneObject = df.gameObject;
        }
        if (GameManager.Instance != null && GameManager.Instance.cargandoDesdeSave)
        {
            CargarJugador();
            StartCoroutine(ApagarBanderaCarga());
        }
    }

    IEnumerator ApagarBanderaCarga()
    {
        yield return null;
        if (GameManager.Instance != null) GameManager.Instance.cargandoDesdeSave = false;
    }

    void Update()
    {
        if (!esLocal) return;
        if (Input.GetKeyDown(KeyCode.G)) GuardarJugador();
        if (Input.GetKeyDown(KeyCode.C)) CargarJugador();

        if (Time.timeScale == 0f) return;

        Vector2 origen = (Vector2)transform.position + Vector2.down * (col.radius);
        RaycastHit2D hit = Physics2D.Raycast(origen, Vector2.down, distanciaSuelo, capaSuelo);
        enSuelo = hit.collider != null;

        if (enSuelo && !estabaEnSuelo && rb.velocity.y <= 0.1f)
        {
            saltosHechos = 0;
            StartCoroutine(EfectoSquashStretch(1.2f, 0.8f));
        }
        estabaEnSuelo = enSuelo;

        if (enSuelo) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space)) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.deltaTime;

        if (!controlBloqueado)
        {
            mov = Input.GetAxisRaw("Horizontal");

            if (mov > 0) sr.flipX = false;
            else if (mov < 0) sr.flipX = true;

            if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            {
                Saltar();
                jumpBufferCounter = 0f;
            }
            else if (Input.GetKeyDown(KeyCode.Space) && saltosHechos > 0 && saltosHechos < maxSaltos)
            {
                Saltar();
            }

            if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            }
        }

        anim.SetFloat("Speed", Mathf.Abs(mov));
        anim.SetBool("Grounded", enSuelo);

        if (transform.position.y <= limiteAbajo) MorirDefinitivo();

        if (alfaFlash > 0) alfaFlash -= Time.deltaTime * 2f;
    }

    void FixedUpdate()
    {
        if (!esLocal) return;
        if (!controlBloqueado)
        {
            rb.velocity = new Vector2(mov * velocidad, rb.velocity.y);
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, limiteIzq, limiteDer);
        pos.y = Mathf.Clamp(pos.y, limiteAbajo, limiteArriba);
        transform.position = pos;
    }

    void Saltar()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * fuerzaSalto, ForceMode2D.Impulse);
        anim.SetTrigger("Jump");
        saltosHechos++;
        coyoteTimeCounter = 0f;
        StartCoroutine(EfectoSquashStretch(0.8f, 1.2f));
    }

    IEnumerator EfectoSquashStretch(float scaleX, float scaleY)
    {
        float duracion = 0.1f;
        float tiempo = 0f;
        Vector3 escalaObjetivo = new Vector3(escalaOriginal.x * scaleX, escalaOriginal.y * scaleY, escalaOriginal.z);

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaOriginal, escalaObjetivo, tiempo / duracion);
            yield return null;
        }
        tiempo = 0f;
        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            transform.localScale = Vector3.Lerp(escalaObjetivo, escalaOriginal, tiempo / duracion);
            yield return null;
        }
        transform.localScale = escalaOriginal;
    }

    void OnGUI()
    {
        if (alfaFlash > 0)
        {
            GUI.color = new Color(1, 1, 1, alfaFlash);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturaRoja);
            GUI.color = Color.white;
        }
    }

    public void RecibirDaño(Transform enemigoTransform, Collider2D colliderEnemigo)
    {
        if (esInvulnerable || vidasActuales <= 0) return;

        vidasActuales--;
        ActualizarTextoVidas();
        alfaFlash = 0.8f;
        
        if (GestorXarxa.Instance != null) GestorXarxa.Instance.EnviarDades("HIT");

        if (CameraShake.Instance != null) CameraShake.Instance.Sacudir(0.3f, 0.4f);

        if (vidasActuales <= 0) MorirDefinitivo();
        else StartCoroutine(RutinaRetroceso(enemigoTransform, colliderEnemigo));
    }

    IEnumerator RutinaRetroceso(Transform enemigo, Collider2D colEnemigo)
    {
        esInvulnerable = true;
        controlBloqueado = true;

        if (colEnemigo != null) Physics2D.IgnoreCollision(col, colEnemigo, true);

        rb.velocity = Vector2.zero;
        float dirX = (enemigo != null && transform.position.x < enemigo.position.x) ? -1 : 1;
        rb.AddForce(new Vector2(dirX * 8f, 6f), ForceMode2D.Impulse);

        for (int i = 0; i < 10; i++)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);

            if (i == 3) controlBloqueado = false;
        }

        sr.enabled = true;
        esInvulnerable = false;

        if (colEnemigo != null) Physics2D.IgnoreCollision(col, colEnemigo, false);
    }

    public void MorirDefinitivo()
    {
        vidasActuales = 0;
        ActualizarTextoVidas();
        
        if (GestorXarxa.Instance != null) GestorXarxa.Instance.EnviarDades("RIVAL_MORT");
        
        StartCoroutine(HandleGameOver("GAME OVER"));
    }

    public void Guanyar()
    {
        controlBloqueado = true;
        rb.velocity = Vector2.zero;
        
        if (textoGameOver != null)
        {
            textoGameOver.text = "HAS GUANYAT!";
            textoGameOver.gameObject.SetActive(true);
        }
        
        Time.timeScale = 0f; 
    }

    public void Morir()
    {
        if (!esInvulnerable) RecibirDaño(transform, null);
    }

    private IEnumerator HandleGameOver(string missatge)
    {
        if (textoGameOver != null)
        {
            textoGameOver.text = missatge;
            textoGameOver.gameObject.SetActive(true);
        }
        Time.timeScale = 0f;
        
        yield return new WaitForSecondsRealtime(tiempoEsperaGameOver);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator HandleGameOver(string missatge)
    {
        if (textoGameOver != null)
        {
            textoGameOver.text = missatge;
            textoGameOver.gameObject.SetActive(true);
        }
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(tiempoEsperaGameOver);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    void ActualizarTextoVidas()
    {
        if (textoVidas != null) textoVidas.text = "" + vidasActuales;
    }

    public void GuardarJugador()
    {
        DatosPartida datos = new DatosPartida();
        datos.vida = this.vidasActuales;
        datos.posX = transform.position.x;
        datos.posY = transform.position.y;
        datos.posZ = transform.position.z;
        datos.nivelActualSceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (monedasManager != null) datos.monedasRecogidas = monedasManager.monedas;

        if (dragonSceneObject != null)
        {
            datos.existeDragon = true;
            datos.dragonX = dragonSceneObject.transform.position.x;
            datos.dragonY = dragonSceneObject.transform.position.y;
            datos.dragonZ = dragonSceneObject.transform.position.z;
        }
        else datos.existeDragon = false;

        Enemigo[] enemigosEnEscena = FindObjectsOfType<Enemigo>();
        foreach (Enemigo enemic in enemigosEnEscena) datos.posicionesEnemigos.Add(enemic.transform.position);

        GameObject[] monedasEnEscena = GameObject.FindGameObjectsWithTag("Moneda");
        foreach (GameObject moneda in monedasEnEscena) datos.posicionesMonedas.Add(moneda.transform.position);

        datos.fechaGuardado = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string nuevoNombre = "Partida_" + timestamp;

        SistemaGuardado.GuardarPartida(datos, nuevoNombre);
        GameManager.Instance.slotActual = nuevoNombre;
        Debug.Log("Partida Guardada: " + nuevoNombre);
    }

    public void CargarJugador()
    {
        string slot = GameManager.Instance.slotActual;
        
        SistemaGuardado.CargarPartidaWeb(slot, (datos) => {
            if (datos != null)
            {
                this.vidasActuales = datos.vida;
                ActualizarTextoVidas();
                transform.position = new Vector3(datos.posX, datos.posY, datos.posZ);

                if (monedasManager != null)
                {
                    monedasManager.monedas = datos.monedasRecogidas;
                    monedasManager.ActualizarTexto();
                }

                if (dragonSceneObject != null)
                {
                    if (datos.existeDragon)
                    {
                        dragonSceneObject.SetActive(true);
                        dragonSceneObject.transform.position = new Vector3(datos.dragonX, datos.dragonY, datos.dragonZ);
                    }
                    else dragonSceneObject.SetActive(false);
                }

                Enemigo[] enemigosViejos = FindObjectsOfType<Enemigo>();
                foreach (Enemigo e in enemigosViejos) Destroy(e.gameObject);

                GameObject[] monedasViejas = GameObject.FindGameObjectsWithTag("Moneda");
                foreach (GameObject m in monedasViejas) Destroy(m);

                Fireball[] bolasFuego = FindObjectsOfType<Fireball>();
                foreach (Fireball b in bolasFuego) Destroy(b.gameObject);

                if (prefabEnemigo != null)
                {
                    foreach (Vector3 pos in datos.posicionesEnemigos)
                    {
                        GameObject nouEnemic = Instantiate(prefabEnemigo, pos, Quaternion.identity);
                        Enemigo script = nouEnemic.GetComponent<Enemigo>();
                        if (script != null) script.spawner = FindObjectOfType<GeneradorEnemigo>();
                    }
                }

                if (prefabMoneda != null)
                {
                    foreach (Vector3 pos in datos.posicionesMonedas)
                    {
                        GameObject novaMoneda = Instantiate(prefabMoneda, pos, Quaternion.identity);
                        novaMoneda.tag = "Moneda";
                        MonedaSimple scriptMoneda = novaMoneda.AddComponent<MonedaSimple>();
                        if (monedasManager != null) scriptMoneda.manager = monedasManager;
                        else scriptMoneda.manager = FindObjectOfType<MonedasManager>();
                    }
                }
                Debug.Log("¡Mundo restaurado desde la BD!");
            }
        });
    }
}