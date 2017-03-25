using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAlarm : MonoBehaviour {

    [SerializeField] AudioSource fireAlarm;

    private GameObject player;
    private bool isPushed;
    private bool ableToPush;
    private float distance = 5f;
	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        ableToPush = false;
        isPushed = false;
	}
	
	// Update is called once per frame
	void Update () {
       
        if (GetComponent<Renderer>().isVisible) {
            if (((player.transform.position - this.transform.position).sqrMagnitude < 3 * 1))
            {
                ableToPush = true;
            }
            else
            {
                ableToPush = false;
            }
        }
        else {
            ableToPush = false;
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            //Play fire alarm
            if(ableToPush && !fireAlarm.isPlaying)
            {
                startSound();
            }
        }
    }
    private void startSound() {
        fireAlarm.Play();
    }   
}
