using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerShooting : NetworkBehaviour
{
	[SerializeField] float shotReloadTime;
	[SerializeField] float gunRaiseTime;
	[SerializeField] ShotEffectsManager shotEffects;
	[SerializeField] GameObject gun;

	private NetworkAnimator parentPlayerAnim;
	float elapsedTime;
	float elapsedTimeRaise;
	bool canShoot;

	private Text instructions;

	private Player player;

	bool gunRaised = false;

	private float shotCooldown;
	private float raiseCooldown;

	void Start() {
		instructions = GameObject.FindGameObjectWithTag("GunInstructionsText").GetComponent<Text>();
		parentPlayerAnim = GetComponentInParent<NetworkAnimator>();
		shotEffects.Initialize ();
		player = GetComponentInParent <Player>();
		elapsedTime = shotCooldown;
		if (isLocalPlayer) {
			canShoot = true;
		}
	}

	void SetShotCooldown() {
	}

	void Update() {
		if (!canShoot) {
			return;
		}

		shotCooldown -= Time.deltaTime;
		raiseCooldown -= Time.deltaTime;

		if (Input.GetKeyDown(KeyCode.Q)) {
			if (raiseCooldown < 0.0f) {
				raiseCooldown = gunRaiseTime;
				shotCooldown = gunRaiseTime;

				if (!gunRaised) {
					gunRaised = true;
					CmdDrawGun();
				} else {
					gunRaised = false;
					CmdHolsterGun();
				}
			}
		}

		if (Input.GetButtonDown ("Fire1")) {
			if (gunRaised) {
				if (shotCooldown < 0.0f) {
					raiseCooldown = shotReloadTime;
					shotCooldown = shotReloadTime;
					CmdFireShot();
				}
			} else {
				ShowInstructions();
			}

		}
	}

	void ShowInstructions() {
		instructions.text = "Press Q to draw the gun";
		Invoke ("HideInstructions", 1.5f);
	}

	void HideInstructions() {
		instructions.text = "";
	}

	[Command]
	void CmdDrawGun() {
		RpcDrawGun();
	}

	[Command]
	void CmdHolsterGun() {
		RpcHolsterGun();
	}

	[Command]
	void CmdFireShot() {
		FireGun();
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
		MakeBotsPanic(origin);
	}

	void MakeBotsPanic(Vector3 origin) {
		GameObject[] objects = GameObject.FindGameObjectsWithTag("Bot");
		foreach (GameObject obj in objects) {
			Bot bot = obj.GetComponent<Bot>();
			if (bot != null && bot.enabled) {
				StartCoroutine(bot.Panic (origin));
			}
		}
	}

	void OnDrawGizmosSelected() {
		Transform camTransform = this.transform.FindChild("FirstPersonCharacter");
		Debug.DrawRay(camTransform.position, camTransform.forward * 3f, Color.red, 1f);
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
		if (isLocalPlayer) {
			this.parentPlayerAnim.animator.SetBool ("Shooting", true);
		}
		player.RotateAndAim ();
		UpdateGunParent();
		gun.SetActive(true);
	}

	[ClientRpc]
	void RpcHolsterGun() {
		if (isLocalPlayer) {
			this.parentPlayerAnim.animator.SetBool ("Shooting", false);
		}
		player.RotateBack ();
		Invoke ("HideGun", raiseCooldown);
	}

	void HideGun() {
		gun.SetActive(false);
	}

	[ClientRpc]
	void RpcProcessShotEffects(Vector3 point, bool hit) {
		if (isLocalPlayer) {
			this.parentPlayerAnim.animator.SetTrigger("Shoot");
		}

		shotEffects.PlayShotEffects ();

		if (hit) {
			shotEffects.PlayImpactEffect (point);
		} else {
			shotEffects.PlayHitEffect (point);
		}
	}

	[ClientRpc]
	void RpcProcessMissEffects() {
		if (isLocalPlayer) {
			this.parentPlayerAnim.animator.SetTrigger("Shoot");
		}

		shotEffects.PlayShotEffects ();
	}
}
