using System.Collections;
using UnityEngine;
using TMPro;

public class DogDialogOnStart : MonoBehaviour
{
    [Header("Dialog Settings")]
    [TextArea(3, 5)]
    public string dialogMessage = "I love you";

    [Header("References")]
    public GameObject dogDialogUI;
    public TextMeshProUGUI dialogText;

    [Header("Start Settings")]
    public bool delayedTrigger = false;
    public float delaySeconds = 2f;

    private void Start()
    {
        if (delayedTrigger)
        {
            StartCoroutine(DelayedShowRoutine());
        }
        else
        {
            ShowDialog();
        }
    }

    private IEnumerator DelayedShowRoutine()
    {
        yield return new WaitForSeconds(delaySeconds);
        ShowDialog();
    }

    private void ShowDialog()
    {
        if (dogDialogUI == null)
        {
            Debug.LogError("DogDialogOnStart: dogDialogUI is not assigned!");
            return;
        }

        var dogDialogComponent = dogDialogUI.GetComponent<Game.UI.DogDialogUI>();

        dogDialogUI.SetActive(true);

        if (dogDialogComponent != null)
        {
            dogDialogComponent.SetTextAndStartTyping(dialogMessage);
        }
    }
}
