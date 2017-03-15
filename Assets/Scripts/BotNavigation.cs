using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Networking;
using System.Collections.Generic;

public class BotNavigation : MonoBehaviour {

// 	private GameObject[] goals;

// 	private NavMeshAgent agent;

// 	private float rotateSpeed = 150f;

// 	private GameObject destination;

// 	public bool ready = false;

// 	private float remainingTime = 0f;
// 	private float rotationDirection = 0f;

// 	public NetworkAnimator anim;

// 	void Awake () {
// 		goals = GameObject.FindGameObjectsWithTag ("Goal");
// 		agent = GetComponent<NavMeshAgent>();
// 	}

// 	void Start () {
// 		ready = true;
// 	}

// 	[Task] bool Ready() {
// 		return ready;
// 	}

// 	void ChooseDestination() {
// 		if (goals.Length == 0) {
// 			return;
// 		}
// 		int index = Random.Range (0, goals.Length);
// 		if (agent != null) {
// 			agent.destination = goals[index].transform.position;
// 		}
// 	}

// 	[Task] void Goto() {
// 		if (Task.current.isStarting) {
// 			ChooseDestination();
// 			anim.animator.SetBool("Walking", true);
// 			agent.Resume();
// 		}

// 		float distance = agent.remainingDistance;

// 		Task.current.debugInfo = string.Format("{0:0.00}", distance);

// 		if (distance < 1f) {
// 			anim.animator.SetBool("Walking", false);
// 			agent.Stop();
// 			Task.current.Succeed();
// 		}
// 	}

	// [Task] void RotateRandom() {
	// 	if (Task.current.isStarting) {
	// 		remainingTime = Random.Range(1.0f, 3.0f);
	// 		rotationDirection = (Random.Range(0f, 1f) < 0.5f) ? -1f : 1f;
	// 	} else {
	// 		remainingTime -= Time.deltaTime;
	// 	}

	// 	if (remainingTime <= 0f) {
	// 		Task.current.Succeed();
	// 	} else {
	// 		this.transform.Rotate(Vector3.up * rotationDirection * rotateSpeed * Time.deltaTime);
	// 	}

	// 	Task.current.debugInfo = string.Format("{0:0.00}", remainingTime);
	// }

	// void RandomRotation (int direction) {
	// 	this.transform.Rotate(Vector3.up * direction * rotateSpeed * Time.deltaTime);
	// }
}
