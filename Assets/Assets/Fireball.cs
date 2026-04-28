using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float velocidad = 5f;
    private Vector3 direccion;

    void Start()
    {
        direccion = new Vector3(Random.Range(-0.5f, 0.5f), -1f, 0).normalized;
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