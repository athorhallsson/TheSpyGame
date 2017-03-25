using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class FireAlarm : NetworkBehaviour
{
    [SerializeField] AudioSource fireAlarm;

    private Player player;
    private bool ableToPush;
    private float distance = 5f;
    //Call the server and tell it to execute the firealarm

	// Use this for initialization
	void Start () {
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject p in players) {
			if (p.GetComponent<Player> ().isLocalPlayer) {
				player = p.GetComponent<Player>();
			}
		}
        ableToPush = false;
	}
	
	// Update is called once per frame
	void Update () {
       
        if (GetComponent<Renderer>().isVisible) {

			float dist = Vector3.Distance(player.transform.position, this.transform.position);
			if (dist < distance) {
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
            if (ableToPush && !fireAlarm.isPlaying)
            {
				player.PlayFireAlarm ();
            }
        }
    }

    public void playSound() {
        fireAlarm.Play();
    }


}
