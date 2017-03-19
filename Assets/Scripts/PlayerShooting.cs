using UnityEngine;
using UnityEngine.Networking;

public class PlayerShooting : NetworkBehaviour 
{
	[SerializeField] float shotCooldown;
	[SerializeField] ShotEffectsManager shotEffects;
	[SerializeField] GameObject gun;

	private NetworkAnimator parentPlayerAnim;
	float elapsedTime;
	bool canShoot;

	void Start() {
		parentPlayerAnim = GetComponentInParent<NetworkAnimator>();
		shotEffects.Initialize ();

		elapsedTime = 0f;
		if (isLocalPlayer) {
			canShoot = true;
		}
	}

	void Update() {
		Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 3f, Color.red, 1f);
		if (!canShoot) {
			return;
		}

		elapsedTime += Time.deltaTime;

		if (Input.GetButtonDown ("Fire1")) {
			if (elapsedTime > shotCooldown) {
				elapsedTime = 0f;
				this.parentPlayerAnim.SetTrigger("Shooting");
				CmdFireShot ();
			}
		}
	}

	// Shooting
	private void FireGun() {
		Vector3 origin = Camera.main.transform.position;
		Vector3 direction = Camera.main.transform.forward;

		RaycastHit hit;
		Ray ray = new Ray (origin, direction);
		Debug.DrawRay    (ray.origin, ray.direction * 3f, Color.red, 1f);


		bool result = Physics.Raycast (ray, out hit, 50f);

		if (result) {
			PlayerHealth enemy = hit.transform.GetComponentInParent<PlayerHealth> ();
			if (enemy != null) {
				enemy.TakeDamage ();
				RpcProcessShotEffects (hit.point, true);
			} else {
				RpcProcessShotEffects (hit.point, false);
			}
		}
	}

	void OnDrawGizmosSelected() {
		Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 3f, Color.red, 1f);
	}

	[Command]
	void CmdFireShot() {
		RpcDrawGun();
		Invoke("FireGun", 2.0f);
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
	void RpcProcessShotEffects(Vector3 point, bool hit) {
		shotEffects.PlayShotEffects ();
		if (hit) {
			shotEffects.PlayImpactEffect (point);
		} else {
			shotEffects.PlayHitEffect (point);
		}
	}
}
