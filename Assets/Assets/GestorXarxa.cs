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
    bool esServidor = false;
    bool estaConnectat = false;

    [Header("Interfície (UI)")]
    public GameObject botoHost;
    public GameObject botoClient;
    public TMP_InputField inputIP;

    [Header("Jugador")]
    public GameObject prefabJugador;
    private GameObject jugadorRival;
    private ScriptPersonaje meuPersonatge;

    void Awake() { Instance = this; }

    public void IniciarHost()
    {
        driver = NetworkDriver.Create();
        var endpoint = NetworkEndpoint.AnyIpv4.WithPort(port);
        if (driver.Bind(endpoint) != 0) 
        {
            Debug.LogError("Error: Port " + port + " ocupat.");
        } 
        else 
        {
            driver.Listen();
            esServidor = true;
            InstanciarMeuJugador();
            Debug.Log("Servidor Obert! Esperant rival...");
            AmagarBotons(); 
        }
    }

    public void IniciarClient()
    {
        if (string.IsNullOrEmpty(inputIP.text))
        {
            Debug.LogError("ERROR: Has d'escriure una IP per connectar-te!");
            return; 
        }

        string ipDesti = inputIP.text; 
        driver = NetworkDriver.Create();
        conexio = default(NetworkConnection);
        
        var endpoint = NetworkEndpoint.Parse(ipDesti, port);
        conexio = driver.Connect(endpoint);
        
        esServidor = false;
        Debug.Log("Intentant connectar a " + ipDesti + "...");
    }

    void AmagarBotons()
    {
        if (botoHost != null) botoHost.SetActive(false);
        if (botoClient != null) botoClient.SetActive(false);
        if (inputIP != null) inputIP.gameObject.SetActive(false);
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
                Debug.Log("RIVAL CONNECTAT!");
            }
        }

        NetworkEvent.Type cmd;
        while ((cmd = driver.PopEvent(out NetworkConnection con, out DataStreamReader stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                estaConnectat = true;
                Debug.Log("Connectat amb èxit!");
                if (!esServidor) 
                {
                    InstanciarMeuJugador();
                    AmagarBotons(); 
                }
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                NativeArray<byte> rawData = new NativeArray<byte>(stream.Length, Allocator.Temp);
                stream.ReadBytes(rawData);
                ProcessarMissatge(Encoding.UTF8.GetString(rawData.ToArray()));
                rawData.Dispose();
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Connexió perduda.");
                estaConnectat = false;
            }
        }

        if (estaConnectat && conexio != default(NetworkConnection) && meuPersonatge != null)
        {
            string dades = "POS|" + meuPersonatge.transform.position.x + "|" + 
                           meuPersonatge.transform.position.y + "|" + 
                           meuPersonatge.GetComponent<SpriteRenderer>().flipX;
            EnviarDades(dades);
        }
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
            }
            jugadorRival.transform.position = new Vector3(float.Parse(d[1]), float.Parse(d[2]), 0);
            jugadorRival.GetComponent<SpriteRenderer>().flipX = bool.Parse(d[3]);
        }
        else if (d[0] == "VICTORIA")
        {
            Debug.Log("Has perdut la cursa!");
            meuPersonatge.MorirDefinitivo();
        }
    }

    void OnDestroy()
    {
        if (driver.IsCreated) driver.Dispose();
    }
}