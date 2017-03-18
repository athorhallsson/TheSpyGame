using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : NetworkBehaviour 
{
	[SerializeField] float respawnTime;

	[SerializeField] GameObject[] models;
	[SerializeField] GameObject model;

	private NetworkAnimator anim;

	private PlayerShooting playerShooting;
	private PlayerHealth playerHealth;
	private Camera fpsCamera;
	private AudioListener fpsAudio;
	private FirstPersonController firstPersonController;

	GameObject mainCamera;

	// Bots
	[SerializeField] GameObject botPrefab;
	[SerializeField] int botCount;

	void Awake() {
		anim = this.GetComponent<NetworkAnimator>();
		ChooseModel();
//		GameObject body = this.GetComponent<Body>().GetBody();
//		body.transform.position = transform.position;
//		body.transform.position += new Vector3(0f, -1f, 0f);
//		body.transform.parent = transform;

		playerShooting = this.GetComponent<PlayerShooting> ();
		playerHealth = this.GetComponent<PlayerHealth> ();
		fpsCamera = this.GetComponentInChildren<Camera> ();
		fpsAudio = this.GetComponentInChildren<AudioListener> ();
		firstPersonController = this.GetComponent<FirstPersonController> ();
		anim.animator = model.GetComponent<Animator>();
	}

	private void ChooseModel() {
		GameObject modelPrefab = models[Random.Range(0, models.Length)];
		model = Instantiate(modelPrefab, new Vector3(this.transform.position.x, this.transform.position.y - 0.9f, this.transform.position.z ), Quaternion.identity);
		model.transform.parent = this.transform;
		this.anim.animator = model.GetComponent<Animator>();
	}

	void Start() {
		mainCamera = Camera.main.gameObject;

		for (int i = 0; i < botCount; i++) {
			CmdSpawnBot();
		}

		EnablePlayer ();
	}

	void DisablePlayer() {
		playerShooting.enabled = false;
		playerHealth.enabled = false;

		if (isLocalPlayer) {
			this.firstPersonController.enabled = false;
			mainCamera.SetActive (true);
			fpsCamera.enabled = false;
		}
		//onToggleRemote.Invoke (false);
	}

	void EnablePlayer() {
		if (isLocalPlayer) {
			mainCamera.SetActive (false);
			fpsCamera.enabled = true;
			fpsAudio.enabled = true;
			firstPersonController.enabled = true;
		}

		playerShooting.enabled = true;
		playerHealth.enabled = true;
	
		//onToggleRemote.Invoke (true);
	}

	public void Despawn() {
	}

	public void Die() {
		anim.SetTrigger( "Death");
		DisablePlayer ();
		Invoke ("Respawn", respawnTime);
	}

	void Respawn() {
		if (isLocalPlayer) {
			anim.SetTrigger("Respawn");
			Transform spawn = NetworkManager.singleton.GetStartPosition ();
			transform.position = spawn.position;
			transform.rotation = spawn.rotation;
		}

		EnablePlayer ();
	}

	void Update() {
		if (isLocalPlayer) {
			if (Mathf.Abs(Input.GetAxis ("Vertical")) + Mathf.Abs(Input.GetAxis ("Horizontal")) > 0.001f) {
				anim.animator.SetBool ("Walking", true);
			} else {
				anim.animator.SetBool ("Walking", false);
			}
		}
	}

	[ServerCallback]
	private void InitializePlayer() {

	}

	[Command]
	private void CmdSpawnBot() {
		Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
		GameObject bot = Instantiate(botPrefab, spawnPoint.position, Quaternion.identity);
		NetworkServer.SpawnWithClientAuthority(bot, connectionToClient);
	}
}

