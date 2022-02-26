using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class mistakedata
{
    //mistake types: 0 is early, 1 is late, 2 is wrong note, 3 is added note, 4 is omitted note, 5 is dynamics, 6 is accent, 7 is creshendo, 8 is sharp, 9 is flat
    public float[,] mistake_time_type = new float[888,3];
}
