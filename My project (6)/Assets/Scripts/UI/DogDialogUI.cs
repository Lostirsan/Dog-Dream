using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class DogDialogUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private Button continueButton;

        [Header("Dialog")]
        [TextArea]
        [SerializeField] private string fullText = "Базовый диалог собачки";
        [SerializeField, Min(0.001f)] private float secondsPerChar = 0.03f;

        private Coroutine _typing;
        private bool _isTyping;

        private void Awake()
        {
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnEnable()
        {
            // Show mouse while a dialog is active.
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            StartTyping();
        }

        private void OnDisable()
        {
            // Hide/lock mouse back for FPS.
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        public void StartTyping()
        {
            if (dialogText == null)
                return;

            if (_typing != null)
                StopCoroutine(_typing);

            dialogText.text = string.Empty;
            _typing = StartCoroutine(TypeRoutine());
        }

        private IEnumerator TypeRoutine()
        {
            _isTyping = true;
            if (continueButton != null)
                continueButton.interactable = false;

            for (int i = 0; i < fullText.Length; i++)
            {
                dialogText.text = fullText.Substring(0, i + 1);
                yield return new WaitForSeconds(secondsPerChar);
            }

            _isTyping = false;
            if (continueButton != null)
                continueButton.interactable = true;
        }

        private void OnContinueClicked()
        {
            if (dialogText == null)
            {
                gameObject.SetActive(false);
                return;
            }

            // If still typing, instantly complete the text.
            if (_isTyping)
            {
                if (_typing != null)
                    StopCoroutine(_typing);

                dialogText.text = fullText;
                _isTyping = false;
                if (continueButton != null)
                    continueButton.interactable = true;
                return;
            }

            // Close dialog.
            gameObject.SetActive(false);
        }
    }
}
