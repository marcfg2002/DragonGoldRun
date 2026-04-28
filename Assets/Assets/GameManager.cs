using UnityEngine;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gesti�n de Partidas")]
    public string slotActual = "Slot1";
    public bool cargandoDesdeSave = false;

    [Header("Datos de Usuario Web")]
    public int usuarioId = -1; 
    public string usuarioEmail = "";

    [Header("Datos de Juego")]
    public int nivelMaximoDesbloqueado = 1;
    public int cantidadTotalNiveles = 2;
    public int monedasGlobales = 0;

    [Header("R�cord")]
    public int maxMonedasRecord = 0;

    [Header("Configuraci�n Audio")]
    public float volumenMusica = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CargarConfiguracion();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ReiniciarDatosParaNuevaPartida()
    {
        monedasGlobales = 0;
        cargandoDesdeSave = false;
    }

    public void VerificarRecordMonedas(int monedasActualesEnNivel)
    {
        if (monedasActualesEnNivel > maxMonedasRecord)
        {
            maxMonedasRecord = monedasActualesEnNivel;
            PlayerPrefs.SetInt("MaxMonedasRecord", maxMonedasRecord);
            PlayerPrefs.Save();
            
            if (usuarioId != -1)
            {
                StartCoroutine(GuardarPuntuacionWeb(usuarioId, maxMonedasRecord));
            }
        }
    }

    System.Collections.IEnumerator GuardarPuntuacionWeb(int id, int puntos)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("puntos", puntos);

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/juego_api/actualizar_score.php", form);
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success) {
            Debug.Log("Puntuación guardada en la nube: " + puntos);
        }
    }

    public void CambiarVolumenMusica(bool encendido)
    {
        volumenMusica = encendido ? 1f : 0f;
        GuardarConfiguracion(volumenMusica);
        ControladorMusicaLocal musica = FindObjectOfType<ControladorMusicaLocal>();
        if (musica != null) musica.ActualizarVolumen(volumenMusica);
    }

    public void GuardarConfiguracion(float nuevoVolumen)
    {
        volumenMusica = nuevoVolumen;
        PlayerPrefs.SetFloat("VolumenMusica", volumenMusica);
        PlayerPrefs.Save();
    }
    public void CargarConfiguracion()
    {
        volumenMusica = PlayerPrefs.GetFloat("VolumenMusica", 1f);

        maxMonedasRecord = PlayerPrefs.GetInt("MaxMonedasRecord", 0);

        nivelMaximoDesbloqueado = PlayerPrefs.GetInt("NivelMaximoDesbloqueado", 1);
    }
public void DesbloquearSiguienteNivel(int nivelCompletado)
    {
        int siguienteNivel = nivelCompletado + 1;

        if (siguienteNivel > nivelMaximoDesbloqueado && siguienteNivel <= cantidadTotalNiveles)
        {
            nivelMaximoDesbloqueado = siguienteNivel;

            PlayerPrefs.SetInt("NivelMaximoDesbloqueado", nivelMaximoDesbloqueado);
            PlayerPrefs.Save();

            Debug.Log("Nivell " + siguienteNivel + " DESBLOQUEJAT i GUARDAT!");

            if (usuarioId != -1)
            {
                StartCoroutine(GuardarNivelWeb(usuarioId, nivelMaximoDesbloqueado));
            }
        }
    }

    System.Collections.IEnumerator GuardarNivelWeb(int id, int nivel)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", id);
        form.AddField("nivel", nivel);

        UnityWebRequest www = UnityWebRequest.Post("http://localhost/juego_api/actualizar_nivel.php", form);
        yield return www.SendWebRequest();
        
        if (www.result == UnityWebRequest.Result.Success) {
            Debug.Log("Progreso de niveles guardado en la nube: Nivel " + nivel);
        }
    }
}