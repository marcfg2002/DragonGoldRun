using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public static class SistemaGuardado
{
    public static string urlGuardar = "http://localhost/juego_api/guardar_partida.php";
    public static string urlCargar = "http://localhost/juego_api/cargar_partida.php";
    public static string urlListar = "http://localhost/juego_api/listar_partidas.php";

    // GUARDAR PARTIDA EN EL SERVIDOR
    public static void GuardarPartida(DatosPartida datos, string nomArxiu)
    {
        datos.nombreArchivo = nomArxiu;
        string json = JsonUtility.ToJson(datos, true);

        if (GameManager.Instance.usuarioId != -1) {
            GameManager.Instance.StartCoroutine(RutinaGuardarWeb(GameManager.Instance.usuarioId, nomArxiu, json));
        } else {
            Debug.LogWarning("¡Aviso! No hay usuario logeado. La partida NO se guardará en la nube.");
        }
    }

    private static IEnumerator RutinaGuardarWeb(int usuarioId, string nombreArchivo, string json)
    {
        WWWForm form = new WWWForm();
        form.AddField("usuario_id", usuarioId);
        form.AddField("nombre_archivo", nombreArchivo);
        form.AddField("datos_json", json);

        UnityWebRequest www = UnityWebRequest.Post(urlGuardar, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success) {
            Debug.Log("Partida subida a la BD: " + nombreArchivo);
        } else {
            Debug.LogError("Error al guardar en nube: " + www.error);
        }
    }

    // CARGAR PARTIDA DEL SERVIDOR
    public static void CargarPartidaWeb(string nomArxiu, System.Action<DatosPartida> callback)
    {
        if (GameManager.Instance.usuarioId != -1) {
            GameManager.Instance.StartCoroutine(RutinaCargarWeb(GameManager.Instance.usuarioId, nomArxiu, callback));
        } else {
            callback(null);
        }
    }

    private static IEnumerator RutinaCargarWeb(int usuarioId, string nomArxiu, System.Action<DatosPartida> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("usuario_id", usuarioId);
        form.AddField("nombre_archivo", nomArxiu);

        UnityWebRequest www = UnityWebRequest.Post(urlCargar, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success && www.downloadHandler.text != "Error") {
            DatosPartida datos = JsonUtility.FromJson<DatosPartida>(www.downloadHandler.text);
            callback(datos);
        } else {
            callback(null);
        }
    }

    // LISTAR PARTIDAS DEL USUARIO
    public static void ObtenerTodasLasPartidasWeb(System.Action<List<DatosPartida>> callback)
    {
        if (GameManager.Instance.usuarioId != -1) {
            GameManager.Instance.StartCoroutine(RutinaListarWeb(GameManager.Instance.usuarioId, callback));
        } else {
            callback(new List<DatosPartida>());
        }
    }

    private static IEnumerator RutinaListarWeb(int usuarioId, System.Action<List<DatosPartida>> callback)
    {
        WWWForm form = new WWWForm();
        form.AddField("usuario_id", usuarioId);

        UnityWebRequest www = UnityWebRequest.Post(urlListar, form);
        yield return www.SendWebRequest();

        List<DatosPartida> lista = new List<DatosPartida>();

        if (www.result == UnityWebRequest.Result.Success && www.downloadHandler.text != "Vacio") {
            string[] jsons = www.downloadHandler.text.Split(new string[] { "|||" }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string json in jsons) {
                DatosPartida d = JsonUtility.FromJson<DatosPartida>(json);
                if (d != null) lista.Add(d);
            }
        }
        callback(lista);
    }
}