using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerHealth : NetworkBehaviour 
{
	[SerializeField] int maxHealth = 1;

	Bot bot;
	Player player;
	int health;

	void Awake() {
		player = GetComponent<Player> ();
		bot = GetComponent<Bot> ();
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.J)) {
			RestartGame();
		}
	}

	[ServerCallback]
	void OnEnable() {
		health = maxHealth;
	}

	[Server]
	void RestartGame() {
		GameOver();
	}

	[Server]
	public bool TakeDamage() {
		bool died = false;

		if (health <= 0) {
			return died;
		}
		health--;
		died = health <= 0;

		RpcTakeDamage (died);

		if (bot == null && died) {
			GameOver();
		}

		return died;
	}

	void GameOver() {
		// Disable and unlock all players
		foreach (Player p in FindObjectsOfType<Player>()) {
			p.RpcEndGame();
		}

		// Show GO text on all players
		RpcShowGameOverText (player.playerName + " was eliminated");

		// Wait 8 seconds and kick back to Lobby once
		Invoke ("EndGame", 7.0f);
	}

	[ClientRpc]
	public void RpcShowGameOverText(string message) {
		GameObject goText = GameObject.FindGameObjectWithTag("GameOverText");
		Text gameOverText = goText.GetComponent<Text>();
		gameOverText.text = message;
		gameOverText.enabled = true;
	}

	public void EndGame() {
		FindObjectOfType<NetworkLobbyManager> ().SendReturnToLobby ();
	}

	[ClientRpc]
	void RpcTakeDamage(bool died) {
		if (died) {
			if (player) {
				player.Die ();
			} else {
				bot.Die ();
			}
		}
	}
}
