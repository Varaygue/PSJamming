using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grimoire : MonoBehaviour
{
    public GameObject grimoireUI;
    public bool grimoireOpened;
    public GameObject spellsPage;
    public GameObject monsterPage;
    public FirstPersonController fpsScript;
    // Start is called before the first frame update
    void Start()
    {
        grimoireOpened=false;
    }

    // Update is called once per frame
    void Update()
    {
        if (grimoireOpened==false&Input.GetKeyDown(KeyCode.G))
        {
            grimoireOpened=true;
            grimoireUI.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            fpsScript.lockLook=true;
        }
        if (grimoireOpened==true&Input.GetKeyDown(KeyCode.Escape))
        {
            grimoireOpened=false;
            grimoireUI.SetActive(false);
            fpsScript.lockLook=false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void NextPage()
    {
        if(spellsPage.activeSelf)
        {
            spellsPage.SetActive(false);
            monsterPage.SetActive(true);
        }
        else
        {
            monsterPage.SetActive(false);
            spellsPage.SetActive(true);
        }
    }
}
