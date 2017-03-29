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

	private Vector3 gunshotPoint;
	private AudioSource audioSource;
	private GameObject[] goalsUp;
	private GameObject[] goalsDown;
	private GameObject[] exits;
	private GameObject fireAlarmExit;
	private NavMeshAgent agent;
	private NetworkAnimator anim;
	private NetworkIdentity ident;
	private bool isUpstairs;

	// State Machine Data
	private StateMachine<States> fsm;
	private float rotationRemaining;
	private float rotationDirection;
	private float rotationSpeed;

	public System.String currentState;
	public System.String debugInfo;
	public GameObject debugObject;

	private float assistTime;

	private enum States {
		Building,
		Deciding,
		Idle,
		Walking,
		Looking,
		Dead,
		Leaving,
		FireAlarm,
		Panic,
		Assist
	}

	// Main --------------------------------------------------------------------
	void Awake() {
		agent = GetComponent<NavMeshAgent>();
		ident = GetComponent<NetworkIdentity>();
		audioSource = GetComponent<AudioSource>();

		anim = GetComponent<NetworkAnimator>();
		anim.enabled = false;

		FindDestinations();
	}

	void Start() {
		isUpstairs = (Random.Range(0.0f, 1.0f) < 0.3f);

		if (ident.hasAuthority) {
			fsm = StateMachine<States>.Initialize(this);
			fsm.ChangeState(States.Building);
			agent.avoidancePriority = Random.Range (20, 80);
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

		NetworkTransformChild ntc = this.GetComponent<NetworkTransformChild>();
		ntc.target = model.transform;
		ntc.enabled = true;

		anim.animator = model.GetComponent<Animator>();
		anim.enabled = true;
	}

	// Navigation -------------------------------------------------------------
	private void FindDestinations() {
		goalsUp = GameObject.FindGameObjectsWithTag("GoalUp");
		goalsDown = GameObject.FindGameObjectsWithTag("GoalDown");
		exits = GameObject.FindGameObjectsWithTag("Exit");
		fireAlarmExit = GameObject.Find("Emergency Exit");
	}

	void SetDestination(GameObject dest) {
		agent.ResetPath();
		agent.destination = dest.transform.position;
		debugObject = dest;
	}

	void ChooseExit() {
		if (exits.Length > 0 && agent != null) {
			GameObject exit = exits[Random.Range (0, exits.Length)];
			SetDestination(exit);
		}
	}

	private GameObject[] GoalsOnFloor() {
		return isUpstairs ? goalsUp : goalsDown;
	}

	void ChooseDestination() {
		if (goalsUp.Length > 0 && agent != null) {
			if (Random.Range(0.0f, 1.0f) < 0.05f) {
				isUpstairs = !isUpstairs;
			}

			GameObject[] goals = GoalsOnFloor();
			GameObject goal = goals[Random.Range (0, goals.Length)];
			SetDestination(goal);
		}
	}

	void ChooseRotation() {
		rotationRemaining = Random.Range(30.0f, 210.0f);
		rotationDirection = Random.Range(0.0f, 1.0f) < 0.5f ? 1f : -1f;
		rotationSpeed = Random.Range(100.0f, 200.0f);
	}

	GameObject FindClosestGoal(Vector3 target) {
		float bestDistance = 1000000000.0f;
		GameObject bestGoal = null;

		GameObject[] goals = GoalsOnFloor();
		if (goals.Length > 0 && agent != null) {
			foreach (GameObject goal in goals) {
				float distance = (goal.transform.position - target).magnitude;
				if (distance < bestDistance) {
					bestDistance = distance;
					bestGoal = goal;
				}
			}
		}

		return bestGoal;
	}

	bool ReachedDestination(float minDistance) {
		if (agent.enabled) {
			return agent != null && !agent.pathPending && agent.remainingDistance < minDistance;
		}
		return false;
	}

	private void Scream() {
		this.audioSource.pitch = Random.Range (0.8f, 1.2f);
		this.audioSource.Play();
	}

	// AI State Machine -------------------------------------------------------
	// Building
	void Building_Update() {
		if (modelReady) {
			fsm.ChangeState(States.Idle);
		}
	}

	// Deciding
	void Deciding_Enter() {
		float n = Random.Range(0f, 1.0f);
		States nextState;

		if (n < 0.01f) {
			nextState = States.Leaving;
		} else if (n < 0.02f) {
			nextState = States.Assist;
		} else if (n < 0.10f) {
			nextState = States.Idle;
		} else if (n < 0.66f) {
			nextState = States.Walking;
		} else {
			nextState = States.Looking;
		}

		if (!agent.enabled) {
			nextState = States.Dead;
		}

		fsm.ChangeState(nextState);
	}

	// Idle
	IEnumerator Idle_Enter() {
		anim.animator.SetBool("Walking", false);

		yield return new WaitForSeconds(Random.Range(1.0f, 5.0f));

		fsm.ChangeState(States.Deciding);
	}

	// Walking
	void Walking_Enter() {
		if (agent.enabled) {
			ChooseDestination();
			anim.animator.SetBool("Walking", true);
			agent.Resume();
		}
	}

	void Walking_Update() {
		if (agent.enabled) {
			debugInfo = agent.remainingDistance.ToString();

			if (ReachedDestination(1.0f)) {
				fsm.ChangeState(States.Deciding);
			}
		}
	}

	void Walking_Finally() {
		anim.animator.SetBool("Walking", false);
		if (agent.enabled) {
			agent.Stop ();
		}
	}

	// Looking
	void Looking_Enter() {
		ChooseRotation();
	}

	void Looking_Update() {
		if (agent.enabled) {
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

	// Panic
	private void FindPanicDestination() {
		Vector3 pos = transform.position + new Vector3(0.0f, 1.0f, 0.0f);
		Vector3 toShot = (gunshotPoint - pos).normalized;
		Vector3 away = pos + (toShot * -1.0f * 10.0f);
		Vector3 goal = new Vector3(away.x, pos.y, away.z);

		SetDestination(FindClosestGoal(goal));
	}

	void Panic_Enter() {
		if (!agent.enabled) {
			fsm.ChangeState (States.Dead, StateTransition.Overwrite);
		} else {
			FindPanicDestination();
			agent.speed = 5f;
			anim.animator.SetBool("Running", true);
			Invoke("Scream", Random.Range(0.0f, 2.0f));
			agent.Resume();
		}
	}

	void Panic_Update() {
		if (agent.enabled) {
			debugInfo = agent.remainingDistance.ToString();
			if (ReachedDestination(1.0f)) {
				fsm.ChangeState(States.Deciding);
			}
		}
	}

	void Panic_Finally() {
		anim.animator.SetBool("Running", false);
		if (agent.enabled) {
			agent.Stop ();
			agent.speed = 1.2f;
		}
	}

	public IEnumerator Panic(Vector3 point) {
		if (agent != null && agent.enabled) {
			yield return new WaitForSeconds(Random.Range(0.2f, 0.8f));

			if (agent != null && agent.enabled) {
				gunshotPoint = point;
				fsm.ChangeState (States.Panic, StateTransition.Overwrite);
			}
		}
	}

	// Death
	public void Die() {
		anim.animator.SetTrigger( "Death");
		this.GetComponent<CapsuleCollider> ().enabled = false;

		if (fsm != null) {
			fsm.ChangeState (States.Dead, StateTransition.Overwrite);
		}

		//this.enabled = false;
	}

	void Dead_Enter() {
		if (agent.enabled) {
			agent.Stop ();
			agent.enabled = false;
		}
	}

	void Dead_Update() {
	}

	void Dead_Finally() {
		fsm.ChangeState (States.Dead, StateTransition.Overwrite);
	}

	// Leaving
	void Leaving_Enter() {
		if (agent.enabled) {
			ChooseExit();
			anim.animator.SetBool("Walking", true);
			agent.Resume();
		}
	}

	[ClientRpc]
	void RpcLeave() {
		GameObject.Destroy(gameObject);
	}

	void Leaving_Update() {
		if (agent.enabled) {
			debugInfo = agent.remainingDistance.ToString();

			if (ReachedDestination(1.0f)) {
				RpcLeave();
			}
		}
	}

	// Fire Alarm
	void FireAlarm_Enter() {
		if (agent.enabled) {
			SetDestination(fireAlarmExit);

			agent.speed = 3.0f;
			anim.animator.SetBool("Running", true);
			Invoke("Scream", Random.Range(0.0f, 2.0f));
			agent.Resume();
		}
	}

	void FireAlarm_Update() {
		if (agent.enabled) {
			debugInfo = agent.remainingDistance.ToString();

			if (ReachedDestination(3.0f)) {
				fsm.ChangeState(States.Deciding);
			}
		}
	}

	void FireAlarm_Finally() {
		anim.animator.SetBool("Running", false);
		if (agent.enabled) {
			agent.Stop ();
			agent.speed = 1.2f;
		}
	}

	private void ActuallyFireAlarm() {
		fsm.ChangeState (States.FireAlarm, StateTransition.Overwrite);
	}

	public void FireAlarm() {
		Invoke("ActuallyFireAlarm", Random.Range(0.2f, 0.8f));
	}


	// Assist
	void Assist_Enter() {
		if (agent.enabled) {
			GameObject[] assistPoints = GameObject.FindGameObjectsWithTag ("Assist");
			agent.destination = assistPoints [Random.Range (0, assistPoints.Length)].transform.position;
			agent.Resume ();
			anim.animator.SetBool("Walking", true);
			assistTime = 0f;
		}
	}

	void Assist_Update() {
		if (ReachedDestination(1.0f)) {
			anim.animator.SetBool("Walking", false);
			assistTime += Time.deltaTime;
			if (assistTime > 5f) {
				assistTime = 0f;
				if (Random.Range (0f, 1f) < 0.33f) {
					fsm.ChangeState(States.Deciding);
				}
			}
		}
	}
}
