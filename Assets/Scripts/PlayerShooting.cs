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

	void Start() {
		parentPlayerAnim = GetComponentInParent<NetworkAnimator>();
		shotEffects.Initialize ();

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
				this.parentPlayerAnim.animator.SetBool("Shooting", true);
				//this.parentPlayerAnim.SetTrigger("Shooting");
				CmdFireShot ();
			}
		}
	}

	private IEnumerator FireGun() {
		RpcDrawGun();
		yield return new WaitForSeconds(2.0f);

		Transform camTransform = this.transform.FindChild("FirstPersonCharacter");
		Vector3 origin = camTransform.position + (0.25f * camTransform.forward);
		Vector3 direction = camTransform.forward;

		RaycastHit hit;
		Ray ray = new Ray (origin, direction);
		Debug.DrawRay (ray.origin, ray.direction * 3f, Color.red, 1f);

		bool result = Physics.Raycast (ray, out hit, 50f);

		if (result) {
			PlayerHealth enemy = hit.transform.GetComponentInParent<PlayerHealth> ();
			print ("Hit:");
			print(enemy);

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
			Bot bot = obj.GetComponent<Bot>();
			if (bot != null && bot.enabled) {
				bot.Panic (hit.point);
			}
		}

		yield return new WaitForSeconds(1.5f);
		this.parentPlayerAnim.animator.SetBool("Shooting", false);
		RpcHolsterGun();
	}

	void OnDrawGizmosSelected() {
		Transform camTransform = this.transform.FindChild("FirstPersonCharacter");
		Debug.DrawRay(camTransform.position, camTransform.forward * 3f, Color.red, 1f);
	}

	[Command]
	void CmdFireShot() {
		StartCoroutine(FireGun());
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
	void RpcHolsterGun() {
		this.parentPlayerAnim.animator.SetBool("Shooting", false);
		gun.SetActive(false);
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
