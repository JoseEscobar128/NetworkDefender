using UnityEngine;
using TMPro;
using System.Collections;
using System;

public class DialogueTrigger : MonoBehaviour
{
    private bool playerInZone = false;
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;
    [SerializeField] private GameObject DialoguePanel;
    [SerializeField] private TMP_Text DialogueText;
    [SerializeField] private GameObject Icon;

    private bool dialogueStart = false;
    private int lineIndex;

    private float typingTime = 0.06f;

    void Start()
    {

    }
    void Update()
    {
        if (playerInZone && Input.GetKeyDown(KeyCode.Q))
        {
            if (!dialogueStart)
            {
                StartDialogue();
                return; // ← Importante
            }
        }

        if (dialogueStart && Input.GetKeyDown(KeyCode.Q))
        {
            // Si la línea ya terminó de escribirse, pasar a la siguiente
            if (DialogueText.text == dialogueLines[lineIndex])
            {
                NextLine();
            }
            else
            {
                // Si aún está escribiéndose, saltar el tipeo y ponerla completa
                StopAllCoroutines();
                DialogueText.text = dialogueLines[lineIndex];
            }
        }

    }



    private void StartDialogue()
    {
        dialogueStart = true;
        DialoguePanel.SetActive(true);
        lineIndex = 0;
        StartCoroutine(ShowLine());
        Icon.SetActive(false);
    }

    private IEnumerator ShowLine()
    {
        DialogueText.text = string.Empty;
        foreach (char letter in dialogueLines[lineIndex])
        {
            DialogueText.text += letter;
            yield return new WaitForSeconds(typingTime);
        }
    }

    private void NextLine()
    {
        lineIndex++;
        if (lineIndex < dialogueLines.Length)
        {
            StartCoroutine(ShowLine());
        }
        else
        {
            DialoguePanel.SetActive(false);
            dialogueStart = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = true;
            Debug.Log("Player entered dialogue zone.");
            Icon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = false;
            Debug.Log("Player exited dialogue zone.");
            Icon.SetActive(false);
        }
    }
}
