using UnityEngine;
using UnityEngine.Networking;

public class PlayerShooting : NetworkBehaviour 
{
	[SerializeField] float shotCooldown;
	[SerializeField] Transform firePosition;
	[SerializeField] ShotEffectsManager shotEffects;
	[SerializeField] GameObject gun;

	private NetworkAnimator parentPlayerAnim;
	float elapsedTime;
	bool canShoot;

	void Start() {
		parentPlayerAnim = GetComponentInParent<NetworkAnimator>();

		elapsedTime = 0f;
		if (isLocalPlayer) {
			canShoot = true;
		}
	}

	void Update() {
		if (!canShoot) {
			return;
		}

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
		this.parentPlayerAnim.SetTrigger("Shooting");
		RaycastHit hit;
		Ray ray = new Ray (origin, direction);
		Debug.DrawRay    (ray.origin, ray.direction * 3f, Color.red, 1f);

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
		RpcDrawGun();
	}

	private void UpdateGunParent() {
		Transform hand = this.GetComponent<Player>().model.transform.Find("master/Reference/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand");
		gun.transform.parent = hand;
		gun.transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	[ClientRpc]
	void RpcDrawGun() {
		UpdateGunParent();
		gun.SetActive(true);
	}

	[ClientRpc]
	void RpcProcessShotEffects(bool playImpact, Vector3 point) {
		shotEffects.PlayShotEffects ();
		//		if (playImpact)
		//			shotEffects.PlayImpactEffect (point);
	}
}
