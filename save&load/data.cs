using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class data
{
    public List<string> notes;
    public List<float> time;
    public List<float> dynamics;
    public List<string> ornaments;
    public int tempo;

    public data() 
    {
        notes = new List<string>();
        time = new List<float>();
        dynamics = new List<float>();
        ornaments = new List<string>();
        tempo = 0;
    }
}
