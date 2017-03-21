using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using MonsterLove.StateMachine;
using System.Linq;


public class Bot : NetworkBehaviour {
	[SerializeField] GameObject[] models;
	[SerializeField] GameObject model;
	[SyncVar] public int modelNumber = -1;
	private bool modelReady = false;

	private AudioSource audioSource;
	private GameObject[] goals;
	private NavMeshAgent agent;
	private NetworkAnimator anim;
	private NetworkIdentity ident;

	// State Machine Data
	private StateMachine<States> fsm;
	private float rotationRemaining;
	private float rotationDirection;
	private float rotationSpeed;

	public System.String currentState;
	public System.String debugInfo;
	public GameObject debugObject;

	private enum States {
		Building,
		Deciding,
		Idle,
		Walking,
		Looking,
		Dead,
		Panic
	}

	// Main --------------------------------------------------------------------
	void Awake() {
		agent = GetComponent<NavMeshAgent>();
		ident = GetComponent<NetworkIdentity>();
		audioSource = GetComponent<AudioSource>();

		anim = GetComponent<NetworkAnimator>();
		anim.enabled = false;

		FindGoals();
	}

	void Start() {
		if (ident.hasAuthority) {
			fsm = StateMachine<States>.Initialize(this);
			fsm.ChangeState(States.Building);
		} else {
		}
	}

	void Update() {
		if (!modelReady && modelNumber >= 0) {
			SetModel(modelNumber);
			modelReady = true;
		}
	}
	void LateUpdate() {
		if (ident.hasAuthority) {
			currentState = fsm.State.ToString();
		}
	}

	// Random Models ----------------------------------------------------------
	public void ResetModelNumber() {
		modelNumber = Random.Range(0, models.Length);
	}
	public void SetModel(int number) {
		GameObject modelPrefab = models[number];
		model = Instantiate(modelPrefab, this.transform.position, Quaternion.identity);
		model.transform.parent = this.transform;

		anim.animator = model.GetComponent<Animator>();
		anim.enabled = true;
	}

	// Navigation -------------------------------------------------------------
	private void FindGoals() {
		goals = GameObject.FindGameObjectsWithTag("Goal");
	}

	void ChooseDestination() {
		if (goals.Length > 0 && agent != null) {
			GameObject goal = goals[Random.Range (0, goals.Length)];
			agent.destination = goal.transform.position;
			debugObject = goal;
			agent.Resume();
		}
	}

	void ChooseRotation() {
		rotationRemaining = Random.Range(30.0f, 210.0f);
		rotationDirection = Random.Range(0.0f, 1.0f) < 0.5f ? 1f : -1f;
		rotationSpeed = Random.Range(100.0f, 200.0f);
	}

	// AI State Machine -------------------------------------------------------
	// Building
	void Building_Update() {
		if (modelReady) {
			fsm.ChangeState(States.Deciding);
		}
	}

	// Deciding
	void Deciding_Enter() {
		switch (Random.Range(0, 3)) {
			case 0:
				fsm.ChangeState(States.Idle);
				break;
			case 1:
				fsm.ChangeState(States.Walking);
				break;
			case 2:
				fsm.ChangeState(States.Looking);
				break;
		}
	}

	// Idle
	IEnumerator Idle_Enter() {
		anim.animator.SetBool("Walking", false);

		yield return new WaitForSeconds(Random.Range(1.0f, 3.0f));

		fsm.ChangeState(States.Deciding);
	}

	// Walking
	void Walking_Enter() {
		ChooseDestination();
		anim.animator.SetBool("Walking", true);
		agent.Resume();
	}

	void Walking_Update() {
		debugInfo = agent.remainingDistance.ToString();

		if (agent.remainingDistance < 1f) {
			anim.animator.SetBool("Walking", false);
			fsm.ChangeState(States.Deciding);
		}
	}

	void Walking_Finally() {
		agent.Stop ();
	}

	// Looking
	void Looking_Enter() {
		ChooseRotation();
	}

	void Looking_Update() {
		debugInfo = rotationRemaining.ToString();

		if (rotationRemaining >= 0f) {
			float amount = rotationSpeed * Time.deltaTime;
			this.transform.Rotate(Vector3.up * rotationDirection * amount);
			rotationRemaining -= amount;
		} else {
			fsm.ChangeState(States.Deciding);
		}
	}

	void Panic_Enter() {
		print ("bla");
		agent.speed = 6.0f;
		this.audioSource.pitch = Random.Range (0.8f, 1.2f);
		anim.animator.SetBool("Running", true);
		Invoke("Scream", Random.Range(0.0f, 4.0f));
		agent.Resume();
	}

	void Panic_Update() {
		debugInfo = agent.remainingDistance.ToString();
		if (agent.remainingDistance < 1f) {
			anim.animator.SetBool("Running", false);
			fsm.ChangeState(States.Deciding);
		}
	}

	void Panic_Finally() {
		agent.Stop ();
		agent.speed = 1.2f;
	}

	public void Die() {
		agent.Stop ();
		anim.animator.SetTrigger( "Death");
		fsm.ChangeState (States.Dead, StateTransition.Overwrite);
		this.GetComponent<CapsuleCollider> ().enabled = false;
		this.enabled = false;
	}

	public void Panic(Vector3 point) {
//		List<Transform> positions = new List<Transform> ();
//		foreach (GameObject goal in goals) {
//			positions.Add(goal.GetComponent<Transform> ());
//		}
//		positions.OrderBy (s => Vector3.Distance (s.position, point));
//		agent.destination = positions.ElementAt(5).position;

		agent.destination = goals[10].transform.position;
		
		fsm.ChangeState (States.Panic, StateTransition.Overwrite);
	}

	private void Scream() {
		this.audioSource.Play();
	}
}
