﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : NetworkBehaviour 
{
	[SyncVar (hook = "OnNameChanged")] public string playerName;

	[SerializeField] GameObject[] models;
	public GameObject model;
	[SyncVar] public int modelNumber = -1;
	private bool modelReady = false;

	private NetworkAnimator anim;

	private PlayerShooting playerShooting;
	private PlayerHealth playerHealth;
	private Camera fpsCamera;
	private AudioListener fpsAudio;
	private FirstPersonController firstPersonController;

	private GameObject mainCamera;
	private Vector3 cameraPosition;

	// Bots
	[SerializeField] GameObject botPrefab;
	[SerializeField] int botCount;

	void Awake() {
		anim = this.GetComponent<NetworkAnimator>();
//		GameObject body = this.GetComponent<Body>().GetBody();
//		body.transform.position = transform.position;
//		body.transform.position += new Vector3(0f, -1f, 0f);
//		body.transform.parent = transform;
		playerShooting = this.GetComponent<PlayerShooting> ();
		playerHealth = this.GetComponent<PlayerHealth> ();
		fpsCamera = this.GetComponentInChildren<Camera> ();
		fpsAudio = this.GetComponentInChildren<AudioListener> ();
		firstPersonController = this.GetComponent<FirstPersonController> ();
	}

	void Start() {
		mainCamera = Camera.main.gameObject;
		EnablePlayer ();
	}

	private void SetModel(int number) {
		GameObject modelPrefab = models[number];
		model = Instantiate(modelPrefab, new Vector3(this.transform.position.x, this.transform.position.y - 0.9f, this.transform.position.z ), Quaternion.identity);
		model.transform.parent = this.transform;
		this.anim.animator = model.GetComponent<Animator>();
		//if (isLocalPlayer) {
			//foreach(Transform child in model.transform)
				//child.gameObject.layer = LayerMask.NameToLayer ("Invisible");
		//}
		NetworkTransformChild ntc = this.GetComponent<NetworkTransformChild>();
		ntc.target = model.transform;
		model.transform.localEulerAngles = new Vector3 (0, 0, 0);
	}

	void OnGUI(){
		if (isLocalPlayer) {
			// Aim
			GUI.Box(new Rect(Screen.width / 2,Screen.height / 2, 10, 10), "");
		}
	}

	void DisablePlayer() {
		playerShooting.enabled = false;
		playerHealth.enabled = false;

		if (isLocalPlayer) {
			this.firstPersonController.enabled = false;
			fpsCamera.enabled = false;
			mainCamera.SetActive (true);
			mainCamera.GetComponent<AudioListener> ().enabled = true;
			fpsAudio.enabled = false;
		}
	}

	void EnablePlayer() {
		if (isLocalPlayer) {
			for (int i = 0; i < botCount; i++) {
				CmdSpawnBot();
			}

			mainCamera.SetActive (false);
			fpsCamera.enabled = true;
			fpsAudio.enabled = true;
			mainCamera.GetComponent<AudioListener> ().enabled = false;
			firstPersonController.enabled = true;
			CmdResetModel();
		}

		playerShooting.enabled = true;
		playerHealth.enabled = true;
	}

	public void Die() {
		anim.SetTrigger("Death");
	}

	[ClientRpc]
	public void RpcEndGame() {
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		DisablePlayer ();
	}

	void Update() {
		if (!modelReady && modelNumber >= 0) {
			SetModel(modelNumber);
			modelReady = true;
		}

		if (modelReady) {
			if (isLocalPlayer) {
				if (Mathf.Abs(Input.GetAxis ("Vertical")) + Mathf.Abs(Input.GetAxis ("Horizontal")) > 0.001f) {
					if (Input.GetKey(KeyCode.LeftShift)) {
						MoveCameraForward ();
						anim.animator.SetBool ("Running", true);
					} else {
						MoveCameraBack();
						anim.animator.SetBool ("Running", false);
						anim.animator.SetBool ("Walking", true);
					}
				} else {
					MoveCameraBack();
					anim.animator.SetBool ("Running", false);
					anim.animator.SetBool ("Walking", false);
				}
			}
			if (fpsCamera.enabled) {
				float step = 1.0f * Time.deltaTime;
				fpsCamera.transform.localPosition = Vector3.MoveTowards(fpsCamera.transform.localPosition, cameraPosition, step);
			}
			if (Input.GetKeyDown(KeyCode.Escape)) {
				
					Cursor.visible = true;
					Cursor.lockState = CursorLockMode.None;
//				}
//				else {
//					Cursor.visible = false;
//					Cursor.lockState = CursorLockMode.Locked;
//				}
			}
		}
	}

	[Command]
	private void CmdResetModel() {
		modelNumber = Random.Range(0, models.Length);
	}

	[Command]
	private void CmdSpawnBot() {
		Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
		GameObject bot = Instantiate(botPrefab, spawnPoint.position, Quaternion.identity);

		bot.GetComponent<Bot>().ResetModelNumber();

		NetworkServer.Spawn(bot);
		// NetworkServer.SpawnWithClientAuthority(bot, connectionToClient);
	}

	private void MoveCameraForward() {
		cameraPosition = new Vector3(0, 0.7f, 0.2f);
	}

	private void MoveCameraBack() {
		cameraPosition = new Vector3(0, 0.7f, 0.0f);
	}

	void OnNameChanged(string value) {
		playerName = value;
		gameObject.name = playerName;
	}

	public void RotateAndAim() {
		model.transform.localEulerAngles = new Vector3 (0, -20, 0);
	}

	public void RotateBack() {
		model.transform.localEulerAngles = new Vector3 (0, 0, 0);
	}


	public void PlayFireAlarm() {
		CmdPlayFireAlarm ();
	}


	[Command]
	public void CmdPlayFireAlarm() {
		RpcStartSound();
		MakeBotsFirePanic();
	}

	void MakeBotsFirePanic() {
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Bot");
		foreach (GameObject obj in objects) {
			Bot bot = obj.GetComponent<Bot>();
			if (bot != null && bot.enabled) {
				bot.FireAlarm();
			}
		}
	}

	[ClientRpc]
	public void RpcStartSound() {
		GameObject[] alarms = GameObject.FindGameObjectsWithTag ("FireAlarm");
		FireAlarm alarm = alarms [0].GetComponent<FireAlarm> ();
		alarm.playSound();
	}

}

