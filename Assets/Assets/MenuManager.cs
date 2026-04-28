using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;
using TMPro; 

public class MenuManager : MonoBehaviour
{
    [Header("Paneles UI")]
    public GameObject panelAuth;
    public GameObject panelPrincipal;
    public GameObject panelCargar;

    [Header("UI Referencias")]
    public Toggle toggleMusica;
    public Button[] botonesNiveles;

    [Header("Lista Dinámica")]
    public GameObject prefabBotonPartida;
    public Transform contenedorLista;

    [Header("Panel Ranking")] 
    public GameObject panelRanking; 
    public TMP_Text textoRanking;   
    public string urlRanking = "http://localhost/juego_api/ranking.php";

void Start()
    {
        if (GameManager.Instance.usuarioId != -1)
        {
            if (panelAuth) panelAuth.SetActive(false);
            MostrarMenuPrincipal(); 
        }
        else
        {
            if (panelAuth) panelAuth.SetActive(true);
            if (panelPrincipal) panelPrincipal.SetActive(false);
            if (panelRanking) panelRanking.SetActive(false);
        }

        ConfigurarToggle();
        VerificarNivelesDesbloqueados();
    }

    public void MostrarMenuPrincipal()
    {
        if (panelPrincipal) panelPrincipal.SetActive(true);
        if (panelCargar) panelCargar.SetActive(false);
        if (panelRanking) panelRanking.SetActive(false); 
    }

    public void MostrarSelectorPartidas()
    {
        if (panelPrincipal) panelPrincipal.SetActive(false);
        if (panelCargar) panelCargar.SetActive(true);
        if (panelRanking) panelRanking.SetActive(false);
        GenerarListaDePartidas();
    }

    public void JugarNuevaPartida()
    {
        GameManager.Instance.ReiniciarDatosParaNuevaPartida();
        GameManager.Instance.cargandoDesdeSave = false;
        SceneManager.LoadScene("Nivel1");
    }

    public void CargarNivelEspecifico(string nombreEscena)
    {
        GameManager.Instance.ReiniciarDatosParaNuevaPartida();
        GameManager.Instance.cargandoDesdeSave = false;
        SceneManager.LoadScene(nombreEscena);
    }

    public void CargarPartidaDesdeLista(string nombreArchivo)
    {
        GameManager.Instance.slotActual = nombreArchivo;
        
        SistemaGuardado.CargarPartidaWeb(nombreArchivo, (datos) => {
            if (datos != null)
            {
                Debug.Log("Cargando histórico desde la nube: " + nombreArchivo);
                GameManager.Instance.monedasGlobales = datos.monedasRecogidas;
                GameManager.Instance.cargandoDesdeSave = true;
                SceneManager.LoadScene(datos.nivelActualSceneIndex);
            }
        });
    }

    private void GenerarListaDePartidas()
    {
        foreach (Transform hijo in contenedorLista) Destroy(hijo.gameObject);

        SistemaGuardado.ObtenerTodasLasPartidasWeb((todas) => {
            todas.Reverse();
            foreach (DatosPartida d in todas)
            {
                GameObject btn = Instantiate(prefabBotonPartida, contenedorLista);
                SlotUI ui = btn.GetComponent<SlotUI>();
                if (ui) ui.Configurar(d, this);
            }
        });
    }

    public void VerificarNivelesDesbloqueados()
    {
        if (botonesNiveles == null) return;
        int max = GameManager.Instance.nivelMaximoDesbloqueado;
        for (int i = 0; i < botonesNiveles.Length; i++)
        {
            if ((i + 1) <= max) botonesNiveles[i].interactable = true;
            else botonesNiveles[i].interactable = false;
        }
    }

    public void AlTocarToggle(bool on) { GameManager.Instance.CambiarVolumenMusica(on); }
    
    void ConfigurarToggle()
    {
        if (toggleMusica)
        {
            toggleMusica.isOn = GameManager.Instance.volumenMusica > 0.5f;
            toggleMusica.onValueChanged.AddListener(AlTocarToggle);
        }
    }
    
    public void QuitGame() { Application.Quit(); }


    public void MostrarRanking()
    {
        if (panelPrincipal) panelPrincipal.SetActive(false);
        if (panelCargar) panelCargar.SetActive(false); 
        if (panelRanking) panelRanking.SetActive(true);
        
        StartCoroutine(ObtenerRankingWeb());
    }

    public void OcultarRanking()
    {
        if (panelRanking) panelRanking.SetActive(false);
        if (panelPrincipal) panelPrincipal.SetActive(true);
    }

    System.Collections.IEnumerator ObtenerRankingWeb()
    {
        if (textoRanking != null) textoRanking.text = "Cargando ranking...";
        
        UnityWebRequest www = UnityWebRequest.Get(urlRanking);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            if (textoRanking != null) textoRanking.text = "Error al conectar: " + www.error;
        }
        else
        {
            string respuesta = www.downloadHandler.text;
            
            if (respuesta == "Aún no hay puntuaciones.")
            {
                if (textoRanking != null) textoRanking.text = respuesta;
            }
            else
            {
                if (textoRanking != null)
                {
                    textoRanking.text = "--- TOP JUGADORES ---\n\n";
                    
                    string[] jugadores = respuesta.Split('|'); 
                    
                    foreach (string jugador in jugadores)
                    {
                        if (!string.IsNullOrEmpty(jugador)) 
                        {
                            string[] datos = jugador.Split(':');
                            if(datos.Length >= 2) 
                            {
                                textoRanking.text += datos[0] + " ....... " + datos[1] + " pts\n";
                            }
                        }
                    }
                }
            }
        }
    }
}