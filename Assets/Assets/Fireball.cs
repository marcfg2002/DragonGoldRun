using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float velocidad = 5f;
    private Transform objetivo;
    private Vector3 direccion;

    void Start()
    {
        objetivo = GameObject.FindGameObjectWithTag("Player").transform;
        if (objetivo != null)
        {
            direccion = (objetivo.position - transform.position).normalized;
        }

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
            if (personaje != null)
            {
                personaje.Morir();
            }

            Destroy(gameObject);
        }
    }
}
