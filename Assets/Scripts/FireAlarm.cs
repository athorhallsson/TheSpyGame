using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireAlarm : MonoBehaviour {

    [SerializeField] AudioSource fireAlarm;

    private bool isPushed;
    private bool ableToPush;
	// Use this for initialization
	void Start () {

        ableToPush = false;
        isPushed = false;
	}
	
	// Update is called once per frame
	void Update () {
        print("Visible " + GetComponent<Renderer>().isVisible);
        if (GetComponent<Renderer>().isVisible) {
            ableToPush = true;
        }
        else {
            ableToPush = false;
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            //Play fire alarm
            if(ableToPush)
            {
                isPushed = true;
                startSound();
                isPushed = false;
            }
        }
    }
    private void startSound() {
        if (isPushed)
        {
            fireAlarm.Play();
        }
    }
}
