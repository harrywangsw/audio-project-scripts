using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class populate_grid : MonoBehaviour
{
	public GameObject line; // This is our prefab object that will be exposed in the inspector
	public GameObject text, saved_song, timescale, checkmark;
	public int typeindex = 0, typeindexend = 2;
	public bool[] selectederrortypes = new bool[3];
	public GameObject mistakesymbol;
	public Sprite[] mistakesprites;
	public float secondsperunit, filledtime, scrollspeed, time, zoomfactor;
	List<string> loadablesong = new List<string>();
	Vector3 oldscale;
	public bool zoomed = false;
	Dropdown mydropdown;
	AudioSource microphone;

	void Start()
	{
		microphone = GameObject.Find("microphone").GetComponent<AudioSource>();
		timescale = GameObject.Find("timescale");
		generatesonglist();
		displaytimescale(filledtime, 10);
		mydropdown = GameObject.Find("error_type").GetComponent<Dropdown>();
		mydropdown.onValueChanged.AddListener(delegate {changeerrortype(mydropdown);});
	}

	public void generatesonglist()
	{		
		string[] x;
		int i = 1;
		DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
		FileInfo[] info = dir.GetFiles("*.sa");
		//GameObject.Find("saved songs container").GetComponent<VerticalLayoutGroup>().spacing = info.Length;
		foreach (FileInfo f in info)
		{
			if (!loadablesong.Contains(f.Name))
			{
				x = f.Name.Split('.');
				GameObject b = GameObject.Instantiate(saved_song, GameObject.Find("saved songs container").transform);
				b.transform.localPosition += new Vector3(3, 0, 0);
				b.transform.GetChild(0).gameObject.GetComponent<Text>().text = x[0];
				loadablesong.Add(f.Name);
			}
		}
	}

	public void displaytimescale(float seconds, int beats)
    {
		GameObject newObj;
		int childCount = transform.childCount;
		for (int i = 0; i <= Math.Round(seconds *beats); i++)
		{
			// Create new instances of our prefab until we've created as many as we specified
			newObj = (GameObject)Instantiate(line, transform);
			if (GameObject.Find("Slider").GetComponent<chang_tempo>().usingseconds) newObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = ((i + childCount-1)/10f).ToString();
			if ((i + childCount - 1) % beats == 0)
			{
				newObj.transform.localScale += new Vector3(0, 3, 0);
				newObj.transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(2f, 0.5f, 1);
			}
		}
	}

	void Update()
	{
		GameObject newline;
		time = Camera.main.GetComponent<AudioSource>().time;
		if (secondsperunit == 0f){ secondsperunit = 10f * (gameObject.transform.GetChild(2).localPosition.x - gameObject.transform.GetChild(1).localPosition.x);}

		//move the scrollbar when we reach filledtime
		if (microphone.time > filledtime) { GameObject.Find("note scroll").transform.GetChild(1).GetComponent<Scrollbar>().value += 10f/(gameObject.transform.childCount-1)*(microphone.time-filledtime); filledtime = microphone.time; }

		if (GameObject.Find("error_type").transform.childCount == 4)
		{
			for (int i = 0; i < selectederrortypes.Length; i++)
			{
				Debug.Log(i.ToString() + selectederrortypes[i]);
				if (selectederrortypes[i] && GameObject.Find("error_type").transform.GetChild(3).GetChild(0).GetChild(0).GetChild(i + 1).childCount == 2)
				{
					GameObject.Instantiate(checkmark, GameObject.Find("error_type").transform.GetChild(3).GetChild(0).GetChild(0).GetChild(i + 1));
				}
				else if (!selectederrortypes[i] && GameObject.Find("error_type").transform.GetChild(3).GetChild(0).GetChild(0).GetChild(i + 1).childCount > 2)
                {
					Destroy(GameObject.Find("error_type").transform.GetChild(3).GetChild(0).GetChild(0).GetChild(i + 1).GetChild(2).gameObject);
                }
			}
		}
	}

	public void displaymistake(int type, int notecount)
    {
		if (type<=4 && type >=0 && selectederrortypes[0] || type <= 7 && type > 4 && selectederrortypes[1] || type <= 9 && type > 7 && selectederrortypes[2])
		{
			GameObject symbol = GameObject.Instantiate(mistakesymbol, timescale.transform.GetChild(1).GetChild(notecount + 1));
			symbol.GetComponent<Image>().sprite = mistakesprites[type];
		}
    }

	public void changeerrortype(Dropdown changed)
	{
		int errortype = changed.value;
		if (selectederrortypes[errortype]) selectederrortypes[errortype] = false;
		else selectederrortypes[errortype] = true;
		GameObject.Find("saved songs container").transform.GetChild(0).GetComponent<loadsong>().loadmistakes();
		mydropdown.gameObject.SetActive(true);
	}


	public void displaynote(string currentnote, float seconds, float duration)
    {
		GameObject note = GameObject.Instantiate(text, gameObject.transform.GetChild(0).transform);
		note.transform.GetChild(0).GetComponent<Text>().text = currentnote;
		note.GetComponent<Image>().color = notetocolor(currentnote);
		//float conversion = note.transform.GetChild(0).sizeDelta.x/
		//seconds + 0.1 beacause the 0 seconds marker is off by 5.11 for some reason
		if (Camera.main.gameObject.GetComponent<PDA>().in_practice) note.GetComponent<RectTransform>().localPosition = new Vector3((seconds+0.1f) * secondsperunit, -8f, 0f);
		else note.GetComponent<RectTransform>().localPosition = new Vector3((seconds + 0.1f) * secondsperunit, -2f, 0f);
		//Debug.Log("asds: " + " " + note.GetComponent<RectTransform>().sizeDelta.x + " " + (secondsperunit) * duration);
		note.GetComponent<RectTransform>().sizeDelta = new Vector2(secondsperunit * duration, 8f);

		// if the note is too crammed together, don't show the text
        if (duration < 1f)
        {
			note.transform.GetChild(0).localScale = new Vector3(0, 0, 0);
		}
		note.GetComponent<Button>().onClick.AddListener(delegate { expandtext(note); });
	}

	public void displayobj(GameObject obj, int startnote, float endtime, bool downward)
    {
		Vector3 offset;
		Transform parent;
		if (startnote + 1 < gameObject.transform.GetChild(1).childCount) parent = gameObject.transform.GetChild(1).GetChild(startnote + 1);
		else return;
		if (downward) offset = new Vector3(0f, parent.childCount * -1f, - 10f);
		else offset = new Vector3(0f, parent.childCount, -10f);
		GameObject displayed = GameObject.Instantiate(obj, transform.GetChild(0).transform);
		Debug.Log("name: " + displayed.name);
		displayed.transform.position = parent.position;
		displayed.transform.position += offset;

    }

	static Color notetocolor(string note)
    {
		Color returncolor = new Color();
		string[] x = note.Split('_');
        switch (note[0])
        {
			case 'a':
				returncolor = Color.red;
				break;
			case 'b':
				returncolor = new Color(1f, 0.647f, 0f);
				break;
			case 'c':
				returncolor = Color.yellow;
				break;
			case 'd':
				returncolor = Color.green;
				break;
			case 'e':
				returncolor = Color.cyan;
				break;
			case 'f':
				returncolor = Color.blue;
				break;
			case 'g':
				returncolor = Color.magenta;
				break;
			default:
				returncolor = Color.white;
				break;
		}
		return returncolor;
    }

	public void zoom()
	{
		if (zoomed) return;
		displaytimescale(zoomfactor * (transform.childCount - 1) / 10f, 10);
		foreach (Transform child in transform.GetChild(1))
		{
			child.transform.localPosition = Vector3.Scale(child.transform.localPosition, new Vector3(zoomfactor, 1, 1));
			child.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.Scale(child.gameObject.GetComponent<RectTransform>().sizeDelta, new Vector2(zoomfactor, 1));
		}
		for (int i = 2; i < transform.childCount; i++)
		{
			if ((i - 1) % zoomfactor == 0) transform.GetChild(i).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = ((i - 1) / (10 * zoomfactor)).ToString();
			else transform.GetChild(i).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = " ";
		}
		secondsperunit /= zoomfactor;
		zoomed = true;
	}
	public void zoomout()
    {
		if (!zoomed) return;
		for (int i = transform.childCount - 1; i > transform.childCount * (1 + zoomfactor); i--) Destroy(transform.GetChild(i).gameObject);
		foreach (Transform child in transform.GetChild(1))
		{
			child.transform.localPosition = Vector3.Scale(child.transform.localPosition, new Vector3(1f/zoomfactor, 1, 1));
			child.gameObject.GetComponent<RectTransform>().sizeDelta = Vector2.Scale(child.gameObject.GetComponent<RectTransform>().sizeDelta, new Vector2(1f/zoomfactor, 1));
		}
		for (int i = 2; i < transform.childCount; i++) transform.GetChild(i).GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = ((i-1) / 10f).ToString();
		secondsperunit *= zoomfactor;
		zoomed = false;
	}

	void expandtext(GameObject note_text)
    {
		if(note_text.transform.GetChild(0).GetComponent<RectTransform>().localScale == Vector3.zero) note_text.transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        else note_text.transform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(0,0,0);
	}
}
