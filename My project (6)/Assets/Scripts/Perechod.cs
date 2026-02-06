using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.FlappyBird
{
public class  Perechod  : MonoBehaviour
    {
        [SerializeField] private string sceneToLoad = "GameScene 1";
        [SerializeField] private string playerRootName = "Player";

        private void OnTriggerEnter(Collider other)
        {
            Transform t = other.transform;
            while (t != null)
            {
                if (t.name == playerRootName)
                {
                    LoadScene();
                    return;
                }
                t = t.parent;
            }
        }

        private void LoadScene()
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}


