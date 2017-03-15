using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour {
	[SerializeField] GameObject[] models;
	[SerializeField] GameObject model;

	void Start () {
		ChooseModel();
	}

	private void ChooseModel() {
		GameObject modelPrefab = models[Random.Range(0, models.Length)];
		model = Instantiate(modelPrefab, this.transform.position, Quaternion.identity);
		model.transform.parent = this.transform;
	}
}
