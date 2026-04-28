using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControladorMusicaLocal : MonoBehaviour
{
    private AudioSource miAudioSource;

    void Start()
    {
        miAudioSource = GetComponent<AudioSource>();

        if (GameManager.Instance != null && miAudioSource != null)
        {
            ActualizarVolumen(GameManager.Instance.volumenMusica);
        }
    }

    public void ActualizarVolumen(float volumen)
    {
        if (miAudioSource != null)
        {
            miAudioSource.volume = volumen;
        }
    }
}