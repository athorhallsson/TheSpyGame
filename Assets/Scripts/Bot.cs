using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;
using MonsterLove.StateMachine;


public class Bot : MonoBehaviour {
	[SerializeField] GameObject[] models;
	[SerializeField] GameObject model;

	private GameObject[] goals;
	private NavMeshAgent agent;
	private NetworkAnimator anim;

	// State Machine Data
	private StateMachine<States> fsm;
	private float rotationRemaining;
	private float rotationDirection;
	private float rotationSpeed;

	[SerializeField] System.String currentState;
	[SerializeField] System.String debugInfo;
	[SerializeField] GameObject debugObject;


	private enum States {
		Deciding,
		Idle,
		Walking,
		Looking
	}

	// Main ----------k---------------------------------------------------------
	void Awake() {
		agent = GetComponent<NavMeshAgent>();
		anim = GetComponent<NetworkAnimator>();
		fsm = StateMachine<States>.Initialize(this);

		ChooseModel();
		FindGoals();
	}

	void Start() {
		fsm.ChangeState(States.Deciding);
	}

	void LateUpdate() {
		currentState = fsm.State.ToString();
	}

	// Random Models ----------------------------------------------------------
	private void ChooseModel() {
		GameObject modelPrefab = models[Random.Range(0, models.Length)];
		model = Instantiate(modelPrefab, this.transform.position, Quaternion.identity);
		model.transform.parent = this.transform;
		this.anim.animator = model.GetComponent<Animator>();
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
		print ("Entering Idle");
		anim.animator.SetBool("Walking", false);
		agent.Stop();

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
			agent.Stop();
			fsm.ChangeState(States.Deciding);
		}
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
}
