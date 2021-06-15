using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class populate_grid : MonoBehaviour
{
	public GameObject line; // This is our prefab object that will be exposed in the inspector
	public GameObject text;
	public float secondsperunit, maxtime, scrollspeed, time;
	Vector3 oldscale;

	void Start()
	{
		maxtime = 3f;
		gameObject.GetComponent<HorizontalLayoutGroup>().spacing = (GameObject.Find("note scroll").GetComponent<RectTransform>().sizeDelta.x) / 169f;
		GameObject newObj;
		for (int i = 0; i <= 30; i++)
		{
			// Create new instances of our prefab until we've created as many as we specified
			newObj = (GameObject)Instantiate(line, transform);
			newObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (i / 10f).ToString();
			if (i % 10 == 0)
			{
				newObj.transform.localScale += new Vector3(0, 3, 0);
				newObj.transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(0.5f, 0.5f, 1);
			}
		}
		scrollspeed = 2*(gameObject.GetComponent<HorizontalLayoutGroup>().spacing+text.GetComponent<RectTransform>().sizeDelta.x);
		secondsperunit = (gameObject.GetComponent<HorizontalLayoutGroup>().spacing+ 0.4500122f) * 10f;
	}

	void Update()
	{
		GameObject newline;
		time = Camera.main.GetComponent<AudioSource>().time;
		if (time > maxtime+0.1f)
        {
			newline = (GameObject)Instantiate(line, transform);
			newline.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (Math.Round(time, 1)).ToString();
            if ((Math.Round(time, 1) * 10f)%10 == 0)
            {
				newline.transform.localScale += new Vector3(0f, 3f, 0f);
			}
			maxtime += 0.1f;
			gameObject.transform.localPosition += new Vector3(-scrollspeed, 0f, 0f);
		}

	}

	public void displaynote(string currentnote, float seconds)
    {
		GameObject note = GameObject.Instantiate(text, gameObject.transform.GetChild(0).transform);
		note.GetComponent<TextMeshProUGUI>().text = currentnote;
		note.GetComponent<RectTransform>().localPosition = new Vector3(seconds * secondsperunit, -1.8f, 0f);
		note.GetComponent<RectTransform>().localScale = new Vector3(0.8f, 0.8f, 1);

		Debug.Log("note displayed:)");
	}
}
