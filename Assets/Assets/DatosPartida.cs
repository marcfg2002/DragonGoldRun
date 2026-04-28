using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DatosPartida
{
    public string nombreArchivo;
    public string fechaGuardado;
    public string nombreNivel;

    public int vida;
    public int monedasRecogidas;
    public int nivelActualSceneIndex;
    public float posX, posY, posZ;

    public bool existeDragon;
    public float dragonX, dragonY, dragonZ;

    public List<Vector3> posicionesEnemigos = new List<Vector3>();
    public List<Vector3> posicionesMonedas = new List<Vector3>();

    public DatosPartida() { }
}