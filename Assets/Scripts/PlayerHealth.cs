using UnityEngine;
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

	[ServerCallback]
	void OnEnable() {
		health = maxHealth;
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
		return died;
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