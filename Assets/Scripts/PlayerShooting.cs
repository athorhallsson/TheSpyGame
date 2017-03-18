using UnityEngine;
using UnityEngine.Networking;

public class PlayerShooting : NetworkBehaviour 
{
	[SerializeField] float shotCooldown;
	[SerializeField] Transform firePosition;
	[SerializeField] ShotEffectsManager shotEffects;

	float elapsedTime;
	bool canShoot;

	void Start() {
//		shotEffects.Initialize ();
		elapsedTime = 0f;
		if (isLocalPlayer) {
			canShoot = true;
		}
	}

	void Update() {
		if (!canShoot)
			return;

		elapsedTime += Time.deltaTime;

		if (Input.GetButtonDown ("Fire1")) {
			if (elapsedTime > shotCooldown) {
				elapsedTime = 0f;
				CmdFireShot (firePosition.position, firePosition.forward);
			}
		}
	}

	[Command]
	void CmdFireShot(Vector3 origin, Vector3 direction) {
		RaycastHit hit;

		Ray ray = new Ray (origin, direction);
		Debug.DrawRay (ray.origin, ray.direction * 3f, Color.red, 1f);

		bool result = Physics.Raycast (ray, out hit, 50f);

		if (result) {
			PlayerHealth enemy = hit.transform.GetComponentInParent<PlayerHealth> ();
			if (enemy != null) {
				enemy.TakeDamage();
			}
		}

		// Make the bots panic
//		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
//		foreach (GameObject player in players) {
//			if (player.GetComponent<Player>().isBot()) {
//				player.GetComponent<Navigation> ().Panic ();
//			}
//		}

		RpcProcessShotEffects (result, hit.point);
	}

	[ClientRpc]
	void RpcProcessShotEffects(bool playImpact, Vector3 point) {
		shotEffects.PlayShotEffects ();

		//		if (playImpact)
		//			shotEffects.PlayImpactEffect (point);
	}
}