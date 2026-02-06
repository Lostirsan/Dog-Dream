using UnityEngine;
using Game.Interactions;

namespace Game.Doors
{
    /// <summary>
    /// Simple hinge-like door: toggles between closed and open by rotating around local Y.
    /// Assumes the GameObject pivot is reasonably close to the hinge.
    /// </summary>
    public class DoorInteractable : MonoBehaviour, IInteractable
    {
        private const string DefaultOpenClipResourcePath = "Audio/creaking-door-open";

        [Header("Motion")]
        [SerializeField] private Transform rotateTarget;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 6f;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip openClip;
        [SerializeField] private bool playSoundOnOpen = true;
        [SerializeField] private bool playSoundOnClose = true;
        [Tooltip("Seconds to skip from the start of the clip (useful if the mp3 has leading silence).")]
        [SerializeField] private float openSoundSkipSeconds = 0f;
        [Tooltip("If > 0, plays the sound and waits this many seconds before the door starts opening (sound leads animation).")]
        [SerializeField] private float openSoundLeadSeconds = 0.1f;

        [Header("State")]
        [SerializeField] private bool isOpen;

        private Quaternion _closedLocalRotation;
        private Quaternion _openLocalRotation;

        private Coroutine _pendingStateRoutine;

        private Transform Target => rotateTarget != null ? rotateTarget : transform;

        private void Awake()
        {
            CacheRotations();
            EnsureAudioWired();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
                CacheRotations();
        }

        private void CacheRotations()
        {
            _closedLocalRotation = Target.localRotation;
            _openLocalRotation = _closedLocalRotation * Quaternion.Euler(0f, openAngle, 0f);
        }

        private void Update()
        {
            var target = isOpen ? _openLocalRotation : _closedLocalRotation;
            Target.localRotation = Quaternion.Slerp(Target.localRotation, target, 1f - Mathf.Exp(-openSpeed * Time.deltaTime));
        }

        public void Toggle() => SetOpenState(!isOpen, playSound: true);
        public void Open() => SetOpenState(true, playSound: true);
        public void Close() => SetOpenState(false, playSound: true);

        private void SetOpenState(bool open, bool playSound)
        {
            if (isOpen == open)
                return;

            if (_pendingStateRoutine != null)
            {
                StopCoroutine(_pendingStateRoutine);
                _pendingStateRoutine = null;
            }

            // If we want the sound to lead the opening animation, play sound now and delay the state switch.
            if (open && playSound && playSoundOnOpen && openSoundLeadSeconds > 0f)
            {
                PlayOpenSound();
                _pendingStateRoutine = StartCoroutine(SetStateAfterDelay(open, openSoundLeadSeconds));
                return;
            }

            isOpen = open;

            if (!playSound)
                return;

            if (isOpen && playSoundOnOpen)
                PlayOpenSound();
            else if (!isOpen && playSoundOnClose)
                PlayOpenSound();
        }

        private System.Collections.IEnumerator SetStateAfterDelay(bool open, float delay)
        {
            yield return new WaitForSeconds(delay);
            isOpen = open;
            _pendingStateRoutine = null;
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

            // Use Play() with time offset so we can skip leading silence.
            audioSource.clip = openClip;

            if (openSoundSkipSeconds > 0f)
                audioSource.time = Mathf.Clamp(openSoundSkipSeconds, 0f, Mathf.Max(0f, openClip.length - 0.01f));
            else
                audioSource.time = 0f;

            audioSource.Play();
        }

        public void Interact(GameObject interactor) => Toggle();

        // Used by editor/setup scripts.
        public void SetRotateTarget(Transform t)
        {
            rotateTarget = t;
            CacheRotations();
        }
    }
}
