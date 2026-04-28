using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    [Header("Textos del Botón")]
    public TMP_Text textoNivel;  
    public TMP_Text textoFecha;  
    public TMP_Text textoStats;  

    private string nombreArchivoID; 
    private MenuManager menuManager; 

    public void Configurar(DatosPartida datos, MenuManager manager)
    {
        menuManager = manager;
        nombreArchivoID = datos.nombreArchivo;

        textoNivel.text = "Escena: " + datos.nivelActualSceneIndex; 
        textoFecha.text = datos.fechaGuardado;
        textoStats.text = "Vida: " + datos.vida + " | Monedas: " + datos.monedasRecogidas;

        GetComponent<Button>().onClick.AddListener(AlClicar);
    }

    void AlClicar()
    {
        menuManager.CargarPartidaDesdeLista(nombreArchivoID);
    }
}