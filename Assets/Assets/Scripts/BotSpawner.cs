using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BotSpawner : MonoBehaviour {
	[SerializeField] GameObject botPrefab;
	[SerializeField] int botCount;

	void Start () {
		for (int i = 0; i < botCount; i++) {
			SpawnBot();
		}
	}

	private void SpawnBot() {
		Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
		Instantiate(botPrefab, spawnPoint.position, Quaternion.identity);
	}
}
