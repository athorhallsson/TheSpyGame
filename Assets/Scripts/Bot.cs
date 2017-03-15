using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Bot : MonoBehaviour {
	[SerializeField] GameObject[] models;
	[SerializeField] GameObject model;

	private GameObject[] goals;
	private NavMeshAgent agent;

	void Start () {
		agent = GetComponent<NavMeshAgent>();

		ChooseModel();
		FindGoals();
		ChooseDestination();
	}

	private void ChooseModel() {
		GameObject modelPrefab = models[Random.Range(0, models.Length)];
		model = Instantiate(modelPrefab, this.transform.position, Quaternion.identity);
		model.transform.parent = this.transform;
	}

	private void FindGoals() {
		goals = GameObject.FindGameObjectsWithTag("Goal");
	}

	void ChooseDestination() {
		if (goals.Length > 0 && agent != null) {
			print("Selecting destination...");
			agent.destination = goals[Random.Range (0, goals.Length)].transform.position;
			agent.Resume();
		}
	}
}
