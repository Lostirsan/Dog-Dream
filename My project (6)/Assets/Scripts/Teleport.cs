using UnityEngine;

namespace Game.Gameplay
{
    public class Teleport : MonoBehaviour
    {
        [SerializeField] private string targetObjectName = "Cube (1)";
        [SerializeField] private string playerRootName = "Player";

        private Transform target;

        private void Start()
        {
            GameObject obj = GameObject.Find(targetObjectName);
            if (obj != null) target = obj.transform;
        }

        private void OnTriggerEnter(Collider other)
        {
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
            if (target == null) return;

            var cc = playerRoot.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            playerRoot.transform.position = target.position;
            playerRoot.transform.rotation = target.rotation;

            if (cc != null) cc.enabled = true;
        }
    }
}
