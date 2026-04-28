using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.Text;
using TMPro;

public class GestorXarxa : MonoBehaviour
{
    public static GestorXarxa Instance;
    public ushort port = 5701;

    NetworkDriver driver;
    NetworkConnection conexio;
    public bool esServidor = false;
    bool estaConnectat = false;

    [Header("Interfície (UI)")]
    public GameObject botoHost;
    public GameObject botoClient;
    public TMP_InputField inputIP;

    [Header("Jugador")]
    public GameObject prefabJugador;
    private GameObject jugadorRival;
    private ScriptPersonaje meuPersonatge;
    
    private Vector3 posicioDestiRival;

    void Awake() { Instance = this; }

    public void IniciarHost()
    {
        driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);
        if (driver.Bind(endpoint) != 0) Debug.LogError("Error: Port ocupat.");
        else 
        {
            driver.Listen();
            esServidor = true;
            InstanciarMeuJugador();
            Debug.Log("Servidor Obert!");
            AmagarBotons(); 
        }
    }

    public void IniciarClient()
    {
        string ipNeta = inputIP.text.Replace("\u200B", "").Trim();
        if (string.IsNullOrEmpty(ipNeta)) return; 

        driver = NetworkDriver.Create();
        conexio = default(NetworkConnection);
        var endpoint = NetworkEndpoint.Parse(ipNeta, port);
        conexio = driver.Connect(endpoint);
        esServidor = false;
    }

    void AmagarBotons()
    {
        if (botoHost != null) botoHost.SetActive(false);
        if (botoClient != null) botoClient.SetActive(false);
        if (inputIP != null) inputIP.gameObject.SetActive(false);
    }

    void ActivarMundo()
    {
        MonedasManager mm = FindObjectOfType<MonedasManager>();
        if (mm != null) mm.Activar();
        GeneradorEnemigo ge = FindObjectOfType<GeneradorEnemigo>();
        if (ge != null) ge.Activar();
        DragonFly[] dragons = FindObjectsOfType<DragonFly>();
        foreach(DragonFly d in dragons) d.activo = true;
    }

    void Update()
    {
        if (!driver.IsCreated) return;
        driver.ScheduleUpdate().Complete();

        if (esServidor)
        {
            NetworkConnection c;
            while ((c = driver.Accept()) != default(NetworkConnection))
            {
                conexio = c;
                estaConnectat = true;
                ActivarMundo();
            }
        }

        NetworkEvent.Type cmd;
        while ((cmd = driver.PopEvent(out NetworkConnection con, out DataStreamReader stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                estaConnectat = true;
                if (!esServidor) 
                {
                    InstanciarMeuJugador();
                    AmagarBotons(); 
                    ActivarMundo();
                }
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                NativeArray<byte> rawData = new NativeArray<byte>(stream.Length, Allocator.Temp);
                stream.ReadBytes(rawData);
                ProcessarMissatge(Encoding.UTF8.GetString(rawData.ToArray()));
                rawData.Dispose();
            }
            else if (cmd == NetworkEvent.Type.Disconnect) estaConnectat = false;
        }

        if (estaConnectat && conexio != default(NetworkConnection) && meuPersonatge != null)
        {
            Animator anim = meuPersonatge.GetComponent<Animator>();
            string dades = "POS|" + meuPersonatge.transform.position.x + "|" + meuPersonatge.transform.position.y + "|" + 
                           meuPersonatge.GetComponent<SpriteRenderer>().flipX + "|" +
                           anim.GetFloat("Speed") + "|" + anim.GetBool("Grounded");
            EnviarDades(dades);

            if (esServidor)
            {
                Enemigo e = FindObjectOfType<Enemigo>();
                if (e != null) EnviarDades("ENEM|" + e.transform.position.x + "|" + e.transform.position.y);

                DragonFly[] dragons = FindObjectsOfType<DragonFly>();
                if (dragons.Length > 0)
                {
                    string msgDracs = "DRAGS";
                    foreach(DragonFly d in dragons) msgDracs += "|" + d.transform.position.x + "|" + d.transform.position.y;
                    EnviarDades(msgDracs);
                }
            }
        }

        if (jugadorRival != null)
            jugadorRival.transform.position = Vector3.Lerp(jugadorRival.transform.position, posicioDestiRival, Time.deltaTime * 15f);
    }

    void InstanciarMeuJugador()
    {
        GameObject obj = Instantiate(prefabJugador, new Vector3(-5, 0, 0), Quaternion.identity);
        meuPersonatge = obj.GetComponent<ScriptPersonaje>();
        meuPersonatge.esLocal = true;
        obj.GetComponent<SpriteRenderer>().sortingOrder = 10;
    }

    public void EnviarDades(string msg)
    {
        if (!driver.IsCreated || conexio == default(NetworkConnection)) return;
        driver.BeginSend(conexio, out var writer);
        NativeArray<byte> rawData = new NativeArray<byte>(Encoding.UTF8.GetBytes(msg), Allocator.Temp);
        writer.WriteBytes(rawData);
        driver.EndSend(writer);
        rawData.Dispose();
    }

    void ProcessarMissatge(string msg)
    {
        string[] d = msg.Split('|');
        if (d[0] == "POS")
        {
            if (jugadorRival == null)
            {
                jugadorRival = Instantiate(prefabJugador, Vector3.zero, Quaternion.identity);
                jugadorRival.GetComponent<ScriptPersonaje>().esLocal = false;
                jugadorRival.GetComponent<SpriteRenderer>().sortingOrder = 10;
                Rigidbody2D rb = jugadorRival.GetComponent<Rigidbody2D>();
                if (rb != null) rb.simulated = false;
            }
            
            posicioDestiRival = new Vector3(float.Parse(d[1]), float.Parse(d[2]), 0);
            jugadorRival.GetComponent<SpriteRenderer>().flipX = bool.Parse(d[3]);

            if (d.Length >= 6) 
            {
                Animator animRival = jugadorRival.GetComponent<Animator>();
                if (animRival != null)
                {
                    animRival.SetFloat("Speed", float.Parse(d[4]));
                    animRival.SetBool("Grounded", bool.Parse(d[5]));
                }
            }
        }
        else if (d[0] == "COIN") 
        {
            Vector2 posMoneda = new Vector2(float.Parse(d[1]), float.Parse(d[2]));
            foreach (GameObject m in GameObject.FindGameObjectsWithTag("Moneda"))
            {
                if (Vector2.Distance(m.transform.position, posMoneda) < 1f) { Destroy(m); break; }
            }
        }
        else if (d[0] == "VICTORIA") meuPersonatge.MorirDefinitivo();
        
        else if (d[0] == "DEAD") { if (jugadorRival != null) Destroy(jugadorRival); }
        else if (d[0] == "HIT") { if (jugadorRival != null) StartCoroutine(ParpellejarRival()); }
        else if (d[0] == "KILL_ENEM" && esServidor) 
        { 
            Enemigo e = FindObjectOfType<Enemigo>(); 
            if (e != null) e.MorirPerXarxa(); 
        }
        else if (d[0] == "ENEM" && !esServidor)
        {
            Enemigo e = FindObjectOfType<Enemigo>();
            if (e != null) e.transform.position = Vector3.Lerp(e.transform.position, new Vector3(float.Parse(d[1]), float.Parse(d[2]), 0), Time.deltaTime * 15f);
        }
        else if (d[0] == "DRAGS" && !esServidor)
        {
            DragonFly[] dragons = FindObjectsOfType<DragonFly>();
            int idx = 1;
            for (int i = 0; i < dragons.Length; i++)
            {
                if (idx + 1 < d.Length)
                {
                    dragons[i].transform.position = Vector3.Lerp(dragons[i].transform.position, new Vector3(float.Parse(d[idx]), float.Parse(d[idx+1]), 0), Time.deltaTime * 15f);
                    idx += 2;
                }
            }
        }
    }

    System.Collections.IEnumerator ParpellejarRival()
    {
        SpriteRenderer sr = jugadorRival.GetComponent<SpriteRenderer>();
        for (int i = 0; i < 5; i++)
        {
            if (sr != null) sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        if (sr != null) sr.enabled = true;
    }

    void OnDestroy() { if (driver.IsCreated) driver.Dispose(); }
}