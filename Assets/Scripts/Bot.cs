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

	private StateMachine<States> fsm;

	[SerializeField] System.String currentState;
	[SerializeField] System.String debugInfo;
	[SerializeField] GameObject debugObject;

	private enum States {
		Deciding,
		Idle,
		Walking
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

	// AI State Machine -------------------------------------------------------
	void Deciding_Enter() {
		switch (Random.Range(0, 2)) {
			case 0:
				fsm.ChangeState(States.Idle);
				break;
			case 1:
				fsm.ChangeState(States.Walking);
				break;
		}
	}

	IEnumerator Idle_Enter() {
		print ("Entering Idle");
		anim.animator.SetBool("Walking", false);
		agent.Stop();

		yield return new WaitForSeconds(Random.Range(1.0f, 3.0f));

		fsm.ChangeState(States.Deciding);
	}

	void Walking_Enter() {
		print ("Entering Walking");

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
}
