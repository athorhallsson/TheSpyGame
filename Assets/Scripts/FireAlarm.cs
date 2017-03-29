using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FireAlarm : NetworkBehaviour
{
    [SerializeField] AudioSource fireAlarm;

    private Player player;
    private bool ableToPush;
    private float distance = 1.5f;
	private Text instructions;

	void Start () {
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject p in players) {
			if (p.GetComponent<Player> ().isLocalPlayer) {
				player = p.GetComponent<Player>();
			}
		}
		instructions = GameObject.FindGameObjectWithTag("InstructionsText").GetComponent<Text>();

        ableToPush = false;
	}

	void Update () {
		if (player != null) {
			if (GetComponent<Renderer>().isVisible) {
				float dist = Vector3.Distance(player.transform.position, this.transform.position);
				ableToPush = dist < distance;
			}
			else {
				ableToPush = false;
			}
			if (Input.GetKeyDown(KeyCode.E)) {
				//Play fire alarm
				if (ableToPush && !fireAlarm.isPlaying) {
					player.PlayFireAlarm ();
				}
			}
			if (!ableToPush) {
				instructions.text = "";
			}
		}
    }

	void LateUpdate() {
		if (ableToPush) {
			instructions.text = "Press E to activate the fire alarm";
		}
	}

    public void playSound() {
        fireAlarm.Play();
    }


}
