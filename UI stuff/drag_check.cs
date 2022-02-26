using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drag_check : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        float distance_to_screen = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        Vector3 startpos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
        GameObject.Find("drag_cursor").transform.position += new Vector3(startpos.x- GameObject.Find("drag_cursor").transform.position.x- GameObject.Find("drag_cursor").GetComponent<SpriteRenderer>().sprite.rect.x/2f, 0, 0);
    }
}
