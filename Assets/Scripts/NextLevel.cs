using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public string scenename;
    public void OnTriggerEnter(Collider collider)
    {
        if(collider.CompareTag("Player"))
        {
            LoadScene(scenename);
        }
    }

    public void LoadScene(string scenename)
    {
        SceneManager.LoadScene(scenename);
    }
}
