using UnityEngine;


public class Hitbox : MonoBehaviour
{
    

  
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spider"))
        {
            // Destruir cualquier enemigo tocado
            Destroy(other.gameObject);
        }
    }
    
}


