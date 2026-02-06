using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMech : MonoBehaviour 
{
	private const string DefaultOpenClipResourcePath = "Audio/creaking-door-open";

	public Vector3 OpenRotation, CloseRotation;

	public float rotSpeed = 1f;

	public bool doorBool;

	[Header("Audio")]
	public AudioSource audioSource;
	public AudioClip openClip;
	public bool playSoundOnOpen = true;
	public bool playSoundOnClose = true;
	[Tooltip("Seconds to skip from the start of the clip (useful if the mp3 has leading silence).")]
	public float openSoundSkipSeconds = 0f;
	[Tooltip("If > 0, plays the sound and waits this many seconds before the door starts opening (sound leads animation).")]
	public float openSoundLeadSeconds = 0f;

	private Coroutine pendingStateRoutine;

	void Start()
	{
		doorBool = false;
		EnsureAudioWired();
	}
		
	void OnTriggerStay(Collider col)
	{
		if(col.gameObject.tag == ("Player") && Input.GetKeyDown(KeyCode.E))
		{
			var wasOpen = doorBool;
			var wantsOpen = !doorBool;

			if (pendingStateRoutine != null)
			{
				StopCoroutine(pendingStateRoutine);
				pendingStateRoutine = null;
			}

			// Sound leads opening animation: play sound now, open a moment later.
			if (!wasOpen && wantsOpen && playSoundOnOpen && openSoundLeadSeconds > 0f)
			{
				PlayOpenSound();
				pendingStateRoutine = StartCoroutine(SetDoorBoolAfterDelay(true, openSoundLeadSeconds));
				return;
			}

			doorBool = wantsOpen;

			if (!wasOpen && doorBool && playSoundOnOpen)
				PlayOpenSound();
			else if (wasOpen && !doorBool && playSoundOnClose)
				PlayOpenSound();
		}
	}

	private IEnumerator SetDoorBoolAfterDelay(bool value, float delay)
	{
		yield return new WaitForSeconds(delay);
		doorBool = value;
		pendingStateRoutine = null;
	}

	private void EnsureAudioWired()
	{
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();

		if (audioSource == null)
			audioSource = gameObject.AddComponent<AudioSource>();

		audioSource.playOnAwake = false;
		audioSource.spatialBlend = 1f;

		if (openClip == null)
			openClip = Resources.Load<AudioClip>(DefaultOpenClipResourcePath);
	}

	private void PlayOpenSound()
	{
		if (audioSource == null || openClip == null)
			return;

		audioSource.clip = openClip;

		if (openSoundSkipSeconds > 0f)
			audioSource.time = Mathf.Clamp(openSoundSkipSeconds, 0f, Mathf.Max(0f, openClip.length - 0.01f));
		else
			audioSource.time = 0f;

		audioSource.Play();
	}

	void Update()
	{
		if (doorBool)
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (OpenRotation), rotSpeed * Time.deltaTime);
		else
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (CloseRotation), rotSpeed * Time.deltaTime);	
	}

}

