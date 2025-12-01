using UnityEngine;

public class AttackZone : MonoBehaviour
{
    public bool enableAttack = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Alex player = collision.GetComponent<Alex>();
            if (player != null)
            {
                player.canAttack = enableAttack;

                // 🔥 IMPORTANTE: si NO puede atacar, también cancelar el ataque
                if (!enableAttack)
                {
                    player.isAttacking = false;
                }
            }
        }
    }
}
