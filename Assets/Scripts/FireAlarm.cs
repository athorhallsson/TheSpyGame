using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FireAlarm : NetworkBehaviour
{
    [SerializeField] AudioSource fireAlarm;

    private GameObject player;
    private bool isPushed;
    private bool ableToPush;
    private float distance = 5f;
    //Call the server and tell it to execute the firealarm

	// Use this for initialization
	void Start () {
        player = GameObject.FindGameObjectWithTag("Player");
        ableToPush = false;
        isPushed = false;
	}
	
	// Update is called once per frame
	void Update () {
       
        if (GetComponent<Renderer>().isVisible) {
            if (((player.transform.position - this.transform.position).sqrMagnitude < 3 * 1)) {
                ableToPush = true;
            }
            else {
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
                CmdPlayFireAlarm();
            }
        }
    }

    [Command]
    private void CmdPlayFireAlarm() {
        RpcStartSound();
    }

    [ClientRpc]
    private void RpcStartSound() {
        playSound();
    }

    private void playSound() {
        fireAlarm.Play();
    }


}
