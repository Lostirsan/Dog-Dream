using UnityEngine;

namespace Game.Gameplay
{
    public class TeleportOnTrigger : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private string playerRootName = "Player";

        private void OnTriggerEnter(Collider other)
        {
            // Works with CharacterController since it is a Collider.
            // We match either the Player root itself or any child under it.
            Transform t = other.transform;
            while (t != null)
            {
                if (t.name == playerRootName)
                {
                    TeleportPlayer(t.gameObject);
                    return;
                }
                t = t.parent;
            }
        }

        private void TeleportPlayer(GameObject playerRoot)
        {
            if (target == null)
                return;

            // If the player uses CharacterController, disable briefly to avoid interference.
            var cc = playerRoot.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerRoot.transform.position = target.position;
            playerRoot.transform.rotation = target.rotation;

            if (cc != null) cc.enabled = true;
        }

        public void SetTarget(Transform newTarget) => target = newTarget;
    }
}
