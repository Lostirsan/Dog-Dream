using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TriggerTextPlayer : MonoBehaviour
{
    public Text uiText;
    [TextArea] public string message;
    public AudioSource audioSource;
    public float totalTime = 2f;

    private bool played = false;

    private void OnTriggerEnter(Collider other)
    {
        if (played) return;
        if (!other.CompareTag("Player")) return;

        played = true;

        if (audioSource != null)
            audioSource.Play();

        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        uiText.text = "";
        float delay = totalTime / message.Length;

        foreach (char c in message)
        {
            uiText.text += c;
            yield return new WaitForSeconds(delay);
        }
    }
}
