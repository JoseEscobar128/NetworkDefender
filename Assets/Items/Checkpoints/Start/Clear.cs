using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Clear : MonoBehaviour
{
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void isClear()
    {
        PlayerPrefs.SetInt("user_progress", 2);
        SceneManager.LoadScene(1);
    }
}
