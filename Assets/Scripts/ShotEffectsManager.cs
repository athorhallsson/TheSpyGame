using System.Collections;
using UnityEngine;

public class ShotEffectsManager : MonoBehaviour 
{
	[SerializeField] ParticleSystem muzzleFlash;
	[SerializeField] AudioSource gunAudio;
	[SerializeField] GameObject impactPrefab;
	[SerializeField] GameObject hitPrefab;

	ParticleSystem impactEffect;
	ParticleSystem hitEffect;

	public void Initialize() {
		impactEffect = Instantiate(impactPrefab).GetComponent<ParticleSystem>();
		hitEffect = Instantiate(hitPrefab).GetComponent<ParticleSystem>();
	}
		
	public void PlayShotEffects() {
		if(muzzleFlash.isPlaying) muzzleFlash.Stop();
		muzzleFlash.Play();
		gunAudio.Stop();
		gunAudio.Play();
	}
		
	public void PlayImpactEffect(Vector3 impactPosition) {
		impactEffect = Instantiate(impactPrefab).GetComponent<ParticleSystem>();
		impactEffect.transform.position = impactPosition;   
		impactEffect.Stop();
		impactEffect.Play();
	}

	public void PlayHitEffect(Vector3 hitPosition) {
		hitEffect = Instantiate(hitPrefab).GetComponent<ParticleSystem>();
		hitEffect.transform.position = hitPosition;   
		hitEffect.Stop();
		hitEffect.Play();
	}
}