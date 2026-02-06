using System.Collections;
using UnityEngine;

public class DogDialogAfter60 : MonoBehaviour
{
    [TextArea(3, 5)]
    public string dialogMessage;

    public GameObject dogDialogUI;

    private void Start()
    {
        StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(60f);

        if (dogDialogUI == null) yield break;

        var dogDialogComponent = dogDialogUI.GetComponent<Game.UI.DogDialogUI>();

        dogDialogUI.SetActive(true);

        if (dogDialogComponent != null)
        {
            dogDialogComponent.SetTextAndStartTyping(dialogMessage);
        }
    }
}
