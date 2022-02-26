using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI;

public class pop_up : MonoBehaviour
{
    Action one;
    Action two;
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void editor(string infotext, string text1, string text2, Sprite image, Action button1func, Action button2func)
    {
        gameObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>().text = infotext;
        transform.GetChild(2).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = text1;
        transform.GetChild(3).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = text2;
        if (image != null) transform.GetChild(4).GetComponent<Image>().sprite = image;
        else transform.GetChild(4).GetComponent<Image>().color = new Color(0, 0, 0, 0);

        one = button1func;
        two = button2func;
    }

    public void buttonone()
    {
        one();
        closepopup();
    }

    public void buttontwo()
    {
        two();
        closepopup();
    }

    public void closepopup()
    {
        gameObject.GetComponent<RectTransform>().localScale = new Vector3(0,0,0);
    }
}
