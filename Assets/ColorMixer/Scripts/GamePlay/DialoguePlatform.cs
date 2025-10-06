using UnityEngine;
using UnityEngine.UI;

public class DialoguePlatform : MonoBehaviour
{
    [TextArea] public string dialogueText;
    public GameObject nextPlatform;
    public GameObject dialogueUI;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            ShowDialogue(dialogueText);

            if (nextPlatform)
                nextPlatform.SetActive(true);
        }
    }

    void ShowDialogue(string text)
    {
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
            Text uiText = dialogueUI.GetComponentInChildren<Text>();
            if (uiText != null)
                uiText.text = text;

            AudioManager.Instance.PlaySFX("DialogueShow");
            Invoke(nameof(HideDialogue), 3f);
        }
    }

    void HideDialogue()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);
    }
}
