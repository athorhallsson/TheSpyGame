using System.Collections;
using System.Collections.Generic;
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

	private Player player;

	bool shooting = false;

	void Start() {
		
		parentPlayerAnim = GetComponentInParent<NetworkAnimator>();
		shotEffects.Initialize ();
		player = GetComponentInParent <Player>();
		elapsedTime = shotCooldown;
		if (isLocalPlayer) {
			canShoot = true;
		}
	}

	void Update() {
		if (!canShoot) {
			return;
		}

		elapsedTime += Time.deltaTime;

		if (elapsedTime > shotCooldown && Input.GetKeyDown (KeyCode.Q)) {
			shooting = !shooting;
			elapsedTime = 0f;
			this.parentPlayerAnim.animator.SetBool ("Shooting", shooting);
			if (shooting) {
				CmdDrawGun();
			} else {
				CmdHolsterGun();
			}

		}

		if (Input.GetButtonDown ("Fire1")) {
			if (elapsedTime > shotCooldown && shooting) {
				elapsedTime = 0f;
				CmdFireShot();
			}
		}
	}

	[Command]
	void CmdDrawGun() {
		RpcDrawGun();
	}

	[Command]
	void CmdHolsterGun() {
		RpcHolsterGun();
	}


	private void FireGun() {
		Transform camTransform = this.transform.FindChild("FirstPersonCharacter");
		Vector3 origin = camTransform.position + (0.25f * camTransform.forward);
		Vector3 direction = camTransform.forward;

		RaycastHit hit;
		Ray ray = new Ray (origin, direction);
		Debug.DrawRay (ray.origin, ray.direction * 3f, Color.red, 1f);

		bool result = Physics.Raycast (ray, out hit, 50f);

		if (result) {
			PlayerHealth enemy = hit.transform.GetComponentInParent<PlayerHealth> ();

			if (enemy != null) {
				enemy.TakeDamage ();
				RpcProcessShotEffects (hit.point, true);
			} else {
				RpcProcessShotEffects (hit.point, false);
			}
		} else {
			RpcProcessMissEffects ();
		}

		// Make bots panic
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Bot");
		foreach (GameObject obj in objects) {
			Bot bot = obj.GetComponent<Bot>();
			if (bot != null && bot.enabled) {
				bot.Panic (hit.point);
			}
		}
	}

	void OnDrawGizmosSelected() {
		Transform camTransform = this.transform.FindChild("FirstPersonCharacter");
		Debug.DrawRay(camTransform.position, camTransform.forward * 3f, Color.red, 1f);
	}

	[Command]
	void CmdFireShot() {
		FireGun();
	}

	private void UpdateGunParent() {
		Transform hand = this.GetComponent<Player>().model.transform.Find("master/Reference/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand");
		gun.transform.parent = hand;

//		gun.transform.localPosition = new Vector3(0f, -0.01f, -0.05f);
//		gun.transform.localRotation = Quaternion.identity;
//		gun.transform.localEulerAngles = new Vector3(10, 60, 5);
		gun.transform.localPosition = new Vector3(-0.025f, 0.03f, -0.045f);
		gun.transform.localRotation = Quaternion.identity;
		gun.transform.localEulerAngles = new Vector3(-20, 50, 20);

		gun.GetComponentInChildren<ParticleSystem> ().transform.localPosition = new Vector3 (-0.197f, -0.075f, -0.221f);
		gun.GetComponentInChildren<ParticleSystem> ().transform.localEulerAngles = new Vector3 (180, 50, 0);

	}

	[ClientRpc]
	void RpcDrawGun() {
		player.RotateAndAim ();
		UpdateGunParent();
		gun.SetActive(true);
	}

	[ClientRpc]
	void RpcHolsterGun() {
		player.RotateBack ();
		this.parentPlayerAnim.animator.SetBool("Shooting", false);
		Invoke ("HideGun", 1f);
	}

	void HideGun() {
		gun.SetActive(false);
	}

	[ClientRpc]
	void RpcProcessShotEffects(Vector3 point, bool hit) {
		this.parentPlayerAnim.animator.SetTrigger("Shoot");
		shotEffects.PlayShotEffects ();

		if (hit) {
			shotEffects.PlayImpactEffect (point);
		} else {
			shotEffects.PlayHitEffect (point);
		}
	}

	[ClientRpc]
	void RpcProcessMissEffects() {
		this.parentPlayerAnim.animator.SetTrigger("Shoot");
		shotEffects.PlayShotEffects ();
	}
}
