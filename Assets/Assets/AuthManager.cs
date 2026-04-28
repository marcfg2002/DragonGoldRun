using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class AuthManager : MonoBehaviour
{
    [Header("Rutas del Servidor")]

    public string urlRegistro = "http://localhost/juego_api/registro.php";
    public string urlLogin = "http://localhost/juego_api/login.php";

    [Header("Referencias UI")]
    public TMP_InputField inputEmail;
    public TMP_InputField inputPassword;
    public TMP_Text textoMensaje; 

    [Header("Paneles")]
    public GameObject panelAuth; 
    public GameObject panelPrincipal; 
    public MenuManager menuManager; 

    public void BotonRegistrar()
    {
        StartCoroutine(RutinaRegistro(inputEmail.text, inputPassword.text));
    }

    public void BotonLogin()
    {
        StartCoroutine(RutinaLogin(inputEmail.text, inputPassword.text));
    }

    IEnumerator RutinaRegistro(string email, string pass)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", pass);

        UnityWebRequest www = UnityWebRequest.Post(urlRegistro, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            textoMensaje.text = "Error de conexión: " + www.error;
        }
        else
        {
            string respuesta = www.downloadHandler.text;
            if (respuesta == "Exito")
            {
                textoMensaje.text = "¡Registro completado! Ahora inicia sesión.";
            }
            else
            {
                textoMensaje.text = respuesta;
            }
        }
    }

    IEnumerator RutinaLogin(string email, string pass)
    {
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", pass);

        UnityWebRequest www = UnityWebRequest.Post(urlLogin, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            textoMensaje.text = "Error de conexión: " + www.error;
        }
        else
        {
            string respuesta = www.downloadHandler.text;
            string[] datos = respuesta.Split(',');

           if (datos[0] == "Exito")
            {
                GameManager.Instance.usuarioId = int.Parse(datos[1]);
                GameManager.Instance.usuarioEmail = email;
                GameManager.Instance.nivelMaximoDesbloqueado = int.Parse(datos[2]);
                GameManager.Instance.maxMonedasRecord = int.Parse(datos[3]);

                PlayerPrefs.SetInt("NivelMaximoDesbloqueado", GameManager.Instance.nivelMaximoDesbloqueado);
                PlayerPrefs.SetInt("MaxMonedasRecord", GameManager.Instance.maxMonedasRecord);
                PlayerPrefs.Save();

                textoMensaje.text = "¡Login correcto!";
                
                panelAuth.SetActive(false);
                panelPrincipal.SetActive(true);

                menuManager.VerificarNivelesDesbloqueados(); 
            }
            else
            {
                textoMensaje.text = respuesta; 
            }
        }
    }
}