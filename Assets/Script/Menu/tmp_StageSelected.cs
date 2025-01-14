using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tmp_StageSelected : MonoBehaviour
{
    public void ButtonClick()
    {
        GameObject StageMenu = GameObject.Find("StageMenu");
        GameObject Title = GameObject.Find("Title");
        StageMenu.SetActive(false);
        Title.SetActive(false);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
