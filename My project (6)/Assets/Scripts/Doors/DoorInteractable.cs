using UnityEngine;

namespace Game.Doors
{
    /// <summary>
    /// Simple hinge-like door: toggles between closed and open by rotating around local Y.
    /// Assumes the GameObject pivot is reasonably close to the hinge.
    /// </summary>
    public class DoorInteractable : MonoBehaviour
    {
        [Header("Motion")]
        [SerializeField] private Transform rotateTarget;
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float openSpeed = 6f;

        [Header("State")]
        [SerializeField] private bool isOpen;

        private Quaternion _closedLocalRotation;
        private Quaternion _openLocalRotation;

        private Transform Target => rotateTarget != null ? rotateTarget : transform;

        private void Awake()
        {
            CacheRotations();
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

        public void Toggle() => isOpen = !isOpen;
        public void Open() => isOpen = true;
        public void Close() => isOpen = false;

        // Used by editor/setup scripts.
        public void SetRotateTarget(Transform t)
        {
            rotateTarget = t;
            CacheRotations();
        }
    }
}
