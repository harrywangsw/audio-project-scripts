using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;

[RequireComponent(typeof(AudioSource))]
public class uploadmp3 : MonoBehaviour
{

    string path;
    public List<AudioClip> Cliplist;
    AudioClip[] clips;

    public List<string> audioname;
    AudioSource song;
    AudioClip clip;

    void Start()
    {
        song = GameObject.Find("Main Camera").GetComponent<AudioSource>();
    }
    void Update()
    {
       
    }

    public void loadsong()
    {
        path = gameObject.transform.GetChild(2).gameObject.GetComponent<Text>().text;


        if (Directory.Exists(path))
        {
            DirectoryInfo info = new DirectoryInfo(path);

            foreach (FileInfo item in info.GetFiles("*.wav"))
            {

                audioname.Add(item.Name);
            }

        }
        StartCoroutine(LoadAudioFile());
    }
    IEnumerator LoadAudioFile()
    {
        for (int i = 0; i < audioname.Count; i++)
        {
            UnityWebRequest AudioFiles = UnityWebRequestMultimedia.GetAudioClip(path + string.Format("{0}", audioname[i]), AudioType.WAV);
            yield return AudioFiles.SendWebRequest();
            if (AudioFiles.isNetworkError)
            {
                Debug.Log(AudioFiles.error);
                Debug.Log(path + string.Format("{0}", audioname[i]));
            }
            else
            {
                clip = DownloadHandlerAudioClip.GetContent(AudioFiles);
                clip.name = audioname[i];
                Cliplist.Add(clip);
                Debug.Log(path + string.Format("{0}", audioname[i]));
            }
            song.clip = clip;
        }

    }
}
