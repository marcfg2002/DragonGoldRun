using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float velocidad = 5f;
    private Transform objetivo;
    private Vector3 direccion;

    void Start()
    {
        ScriptPersonaje[] jugadores = FindObjectsOfType<ScriptPersonaje>();
        foreach (ScriptPersonaje p in jugadores)
        {
            if (p.esLocal)
            {
                objetivo = p.transform;
                break;
            }
        }

        if (objetivo != null) direccion = (objetivo.position - transform.position).normalized;
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.position += direccion * velocidad * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ScriptPersonaje personaje = collision.GetComponent<ScriptPersonaje>();
            if (personaje != null && personaje.esLocal)
            {
                personaje.Morir();
            }
            Destroy(gameObject);
        }
    }
}