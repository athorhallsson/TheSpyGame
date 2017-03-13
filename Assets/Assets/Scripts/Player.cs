using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;
using Panda;

[System.Serializable]
public class ToggleEvent : UnityEvent<bool>{}

public class Player : NetworkBehaviour 
{
	[SerializeField] ToggleEvent onToggleShared;
	[SerializeField] ToggleEvent onToggleLocal;
	[SerializeField] ToggleEvent onToggleRemote;
	[SerializeField] float respawnTime;

	private NetworkAnimator anim;
	private BotNavigation nav;
	private NavMeshAgent agent;
	private NavMeshObstacle obstacle;

	GameObject mainCamera;

	void Awake() {
	//	GameObject body = this.GetComponent<Body>().GetBody();
//		body.transform.position = transform.position;
//		body.transform.position += new Vector3(0f, -1f, 0f);
//		body.transform.parent = transform;

		nav = this.GetComponent<BotNavigation>();
		agent = this.GetComponent<NavMeshAgent> ();
		obstacle = this.GetComponent<NavMeshObstacle> ();
//		anim = this.GetComponent<NetworkAnimator>();
//		anim.animator = body.GetComponent<Animator>();
//		nav.anim = anim;
	}

	void Start()
	{
		mainCamera = Camera.main.gameObject;
		EnablePlayer ();
		InitializePlayer ();
	}

	void DisablePlayer()
	{
		if (isLocalPlayer)
			mainCamera.SetActive (true);

		onToggleShared.Invoke (false);

		if (isLocalPlayer)
			onToggleLocal.Invoke (false);
		else
			onToggleRemote.Invoke (false);

		this.GetComponent<FirstPersonController>().enabled = false;
	}

	void EnablePlayer()
	{
		if (isLocalPlayer) {
			mainCamera.SetActive (false);
			AudioListener audio = this.GetComponentInChildren<AudioListener> ();
			if (audio != null) {
				audio.enabled = true;
			}
			this.GetComponent<FirstPersonController>().enabled = true;
		}

		onToggleShared.Invoke (true);

		if (isLocalPlayer)
			onToggleLocal.Invoke (true);
		else
			onToggleRemote.Invoke (true);
	}

	public void Despawn() {
	}

	public void Die() {
		this.GetComponent<PandaBehaviour> ().enabled = false;
		this.GetComponent<NavMeshAgent> ().enabled = false;

		anim.SetTrigger( "Death");

		// Kill bot
		if (playerControllerId == -1 && this.gameObject != null) {
		} else {
			DisablePlayer ();
			Invoke ("Respawn", respawnTime);
		}
	}

	void Respawn()
	{
		if (isLocalPlayer) 
		{
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
//				anim.animator.SetBool ("Walking", true);
			} else {
//				anim.animator.SetBool ("Walking", false);
			}
		}

	}

	public bool isBot() {
		return this.GetComponent<NavMeshAgent> ().enabled;
	}

	[ServerCallback]
	private void InitializePlayer() {
		if (playerControllerId == -1) {
			obstacle.enabled = false;
			agent.enabled = true;
			agent.avoidancePriority = Random.Range(1, 100);
			nav.enabled = true;
			this.GetComponent<PandaBehaviour> ().enabled = true;
		}
	}
}

