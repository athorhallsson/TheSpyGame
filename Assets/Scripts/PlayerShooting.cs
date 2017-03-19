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
		
	private void FireGun() {
		Transform camTransform = Camera.main.transform;
		Vector3 origin = camTransform.position + camTransform.forward;
		Vector3 direction = camTransform.forward;

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

		// Make bots panic
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Bot");
		foreach (GameObject obj in objects) {
			obj.GetComponent<Bot> ().Panic (hit.point);
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
		// Lower hand
//		gun.transform.localPosition = new Vector3(0f, -0.01f, -0.05f);
//		gun.transform.localRotation = Quaternion.identity;
//		gun.transform.localEulerAngles = new Vector3(10, 60, 5);
		gun.transform.localPosition = new Vector3(-0.025f, 0.03f, -0.045f);
		gun.transform.localRotation = Quaternion.identity;
		gun.transform.localEulerAngles = new Vector3(-20, 50, 20);
	
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
