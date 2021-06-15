using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class data
{
    public List<string> notes;
    public float[,] duration = new float[888, 2];
    public float[] correctdynamics;

    public data() 
    {
        notes = new List<string>();
        duration = new float[888, 2];
        correctdynamics = new float[888];
    }
}
