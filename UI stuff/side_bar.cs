using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class side_bar : MonoBehaviour
{
    bool retracted = true;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void display(GameObject sidebar)
    {
        if (retracted)
        {
            retracted = false;
            sidebar.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            retracted = true;
            sidebar.transform.localScale = new Vector3(0, 0, 0);
        }
    }
}
