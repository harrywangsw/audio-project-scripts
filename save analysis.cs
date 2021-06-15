﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public static class saveanalysis
{
    public static void SavePlayer(data analysistobesaved, string songname)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + songname + ".sa";
        FileStream stream = new FileStream(path, FileMode.Create);

        data translationprogress = analysistobesaved;
        formatter.Serialize(stream, translationprogress);
        stream.Close();
        Debug.Log("saved!");
        Camera.main.GetComponent<foundamentalfrequency>().generatesonglist();
    }

    public static data LoadPlayer(string loadname)
    {
        string path = Application.persistentDataPath + "/" + loadname +".sa";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            data data = formatter.Deserialize(stream) as data;
            stream.Close();
            Debug.Log("loaded");
            return data;
        }
        else
        {
            Debug.Log("path not found");
            return null;
        }
    }
    public static void SaveMistake(spectrumanalysis analysistobesaved, string songname)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + songname + ".mistake";
        FileStream stream = new FileStream(path, FileMode.Create);

        mistakedata translationprogress = new mistakedata(analysistobesaved);
        formatter.Serialize(stream, translationprogress);
        stream.Close();
        Debug.Log("saved!");

    }
}
