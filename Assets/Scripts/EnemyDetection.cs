using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    public Enemy enemy;  // arrastra el Enemy padre aquí

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemy.playerDetected = true;
            enemy.player = other.transform; // <-- guardamos la posición del jugador
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemy.playerDetected = false;
            enemy.player = null;
        }
    }

}
