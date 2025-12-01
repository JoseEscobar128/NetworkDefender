using UnityEngine;

public class Rino : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sprite;

    float time_move;
    int par_impar;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        time_move = Time.time;
        par_impar = 1;
    }

    public void DestroyRino()
    {
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > time_move)
        {
            if ((par_impar % 2) == 0)
            {
                rb.linearVelocity = new Vector2(10f, 0);
                sprite.flipX = true;
            }
            else
            {
                rb.linearVelocity = new Vector2(-10f, 0);
                sprite.flipX = false;
            }
            par_impar++;
            time_move += 3;
        }
    }
}
