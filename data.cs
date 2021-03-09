using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class data
{
    string[] notes;
    float[,] duration = new float[888, 2];

    public data(preprocessspectrum analysistobesaved) 
    {
        notes = analysistobesaved.note;
        duration = analysistobesaved.duration;
    }
}
