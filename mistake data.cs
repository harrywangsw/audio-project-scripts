using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class mistakedata
{
    float[,] mistake_time_type = new float[88,2];

    public mistakedata(spectrumanalysis userpractice)
    {

        mistake_time_type = userpractice.mistakes;

    }
}
