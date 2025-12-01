using UnityEngine;
using TMPro;

public class Cherry : MonoBehaviour
{
   // public TextMeshProUGUI texto_cherry;
    public AudioSource effectsSource;
    public AudioClip collectSound;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       // texto_cherry.SetText("" + Alex.numCherry);
        if (effectsSource == null)
        {
            effectsSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        Alex alex = collision.GetComponent<Alex>();
        if (alex == null) return;  // Solo funciona con Alex


        if (!collision.gameObject.CompareTag("Player")) return;

        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, Camera.main ? Camera.main.transform.position : transform.position);
        }

        //Alex.numCherry++;
        alex.CollectCherry();
        //texto_cherry.SetText("" + Alex.numCherry);

        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
