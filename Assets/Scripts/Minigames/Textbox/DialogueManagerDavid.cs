using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueManagerDavid : MonoBehaviour
{
    public DialogueDavid[] dialogue; // The dialogue data
    public TMP_Text dialogueText; // The UI text element for the dialogue text
    public GameObject dialogueContainer; // Dialogue Container

    public int currentDialogueIndex = 0; // The index of the current piece of dialogue in the list
    private bool textFinishedDisplaying = false; // Whether the text for the current line of dialogue has finished displaying
    public bool isTalking = false;
    public bool fastDialogue = false;

    public int counter = 0;

    bool loseFunctionality = true;

    void OnEnable()
    {
        // Start the dialogue when the game object is enabled
        dialogueContainer.SetActive(true);
        StartDialogue(dialogue[counter]);
        isTalking = true;
    }

    void Update()
    {
        // Check if backspace is pressed and fastDialogue is false
        if (Input.GetKeyDown(KeyCode.Backspace) && !fastDialogue)
        {
            fastDialogue = true; // Set fastDialogue to true
        }
        else if (Input.GetKeyDown(KeyCode.Backspace) && fastDialogue)
        {
            fastDialogue = false; // Set fastDialogue to false
        }
        // If the player pressed the "submit" button
        if (Input.GetButtonDown("Submit") && !loseFunctionality || Input.GetMouseButtonDown(0) && !loseFunctionality)
        {
            UpdateDialogue();
        }
    }

    // Start the dialogue
    void StartDialogue(DialogueDavid dialogue)
    {
        DialogueDavid.DialogueLine dialogueLine = dialogue.lines[currentDialogueIndex];
        // Update the UI elements with the data from the first piece of dialogue
        UpdateDialogue();
        // Start the coroutine that updates the dialogue text
        StartCoroutine(UpdateDialogueText(dialogue.lines[currentDialogueIndex].dialogueText, dialogue.lines[currentDialogueIndex].timeBeforeText));
    }

    // Update the dialogue text one character at a time with a delay
    IEnumerator<WaitForSeconds> UpdateDialogueText(string text, float timeBeforeText)
    {

        if(timeBeforeText != 0)
        {
            loseFunctionality = true;
            dialogueContainer.SetActive(false);
            yield return new WaitForSeconds(timeBeforeText);
            dialogueContainer.SetActive(true);
        }

        textFinishedDisplaying = false; // Reset the textFinishedDisplaying variable
    
        // Clear the dialogue text
        dialogueText.text = "";

        // Get the current dialogue line
        DialogueDavid.DialogueLine dialogueLine = dialogue[counter].lines[currentDialogueIndex];

        // If the hit variable is true, shake the screen
        if (dialogueLine.hit)
        {
            Globals.SoundManager.Play("hit");
            StartCoroutine(ScreenShake(0.1f));
        }

        // If the current line has a sound effect specified, play it
        if (dialogueLine.soundEffect != null)
        {
            Globals.SoundManager.Play(dialogueLine.soundEffect.name);
        }

        // Display the text one character at a time
        foreach (char c in text)
        {
            if(c == '^')
            {
                if(fastDialogue)
                {
                    // Pause for the specified duration
                    yield return new WaitForSeconds(0.0f);
                }
                else
                {
                    // Pause for the specified duration
                    yield return new WaitForSeconds(dialogueLine.pauseDuration);
                }
            }
            else
            {
                dialogueLine = dialogue[counter].lines[currentDialogueIndex];
                dialogueText.color = dialogueLine.textColor; // Set the color of the dialogue text
                dialogueText.text += c;
                // Set the delay between characters based on the value of fastDialogue
                yield return new WaitForSeconds(fastDialogue ? dialogueLine.textSpeed / 50 : dialogueLine.textSpeed);
                if(fastDialogue)
                {

                }
                else
                {
                    Globals.SoundManager.Play("typewriter");
                }
                // If the shaking variable is true, shake the screen
                if (dialogueLine.shaking)
                {
                    StartCoroutine(ScreenShake(0.1f));
                }
            }
        }
    
        textFinishedDisplaying = true;

        UpdateDialogue();
    }

    void UpdateDialogue()
    {
        DialogueDavid.DialogueLine dialogueLine = dialogue[counter].lines[currentDialogueIndex];
        // If the player pressed the "submit" button
        if (!textFinishedDisplaying)
        {
            StopAllCoroutines(); // Stop the coroutine that updates the dialogue text
            dialogueLine = dialogue[counter].lines[currentDialogueIndex];
            // Replace all occurrences of "^" with an empty string
            dialogueText.text = dialogueLine.dialogueText.Replace("^", "");
            textFinishedDisplaying = true;
        }
        else if(textFinishedDisplaying)
        {
            // Advance to the next piece of dialogue
            currentDialogueIndex++;
            // If there is no more dialogue, disable the game object
            if (currentDialogueIndex >= dialogue[counter].lines.Count)
            {
                StartCoroutine(ADVANCE(dialogueLine));
                return;
            }

            StartCoroutine(ADVANCE2(dialogueLine));
        }
    }

    public IEnumerator<WaitForSeconds> ScreenShake(float duration)
    {
        float elapsed = 0.0f;
        float magnitude = 0.25f; // The intensity of the screen shake effect
    
        // Shake the screen while the elapsed time is less than the duration
        while (elapsed < duration)
        {
            // Generate a random offset based on the magnitude
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
    
            // Set the camera's position to the offset
            Camera.main.transform.localPosition = new Vector3(x, y, -10);
    
            // Increment the elapsed time by the time that has passed since the last frame
            elapsed += Time.deltaTime;
    
            // Wait for the next frame
            yield return null;
        }
    
        // Reset the camera's position after the shake has finished
        Camera.main.transform.localPosition = new Vector3(0, 0, -10);
    }

    public IEnumerator<WaitForSeconds> WaitForNextCode(DialogueDavid.DialogueLine dialogueLine)
    {
        yield return new WaitForSeconds(1.0f);
        dialogueContainer.SetActive(true);
        // Update the UI elements with the data from the next piece of dialogue
        dialogueLine = dialogue[counter].lines[currentDialogueIndex];
        // Start the coroutine that updates the dialogue text
        StartCoroutine(UpdateDialogueText(dialogueLine.dialogueText, dialogueLine.timeBeforeText));
    }

    public IEnumerator<WaitForSeconds> ADVANCE(DialogueDavid.DialogueLine dialogueLine)
    {
        yield return new WaitForSeconds(dialogueLine.waitTimeToAdvance);
        dialogueContainer.SetActive(false);
        gameObject.SetActive(false);
        fastDialogue = false;
        textFinishedDisplaying = false;
        isTalking = false;
        counter++;
        currentDialogueIndex = 0;
    }

    public IEnumerator<WaitForSeconds> ADVANCE2(DialogueDavid.DialogueLine dialogueLine)
    {
        yield return new WaitForSeconds(dialogueLine.waitTimeToAdvance);
        // Update the UI elements with the data from the next piece of dialogue
        dialogueLine = dialogue[counter].lines[currentDialogueIndex];
        // Start the coroutine that updates the dialogue text
        StartCoroutine(UpdateDialogueText(dialogueLine.dialogueText, dialogueLine.timeBeforeText));
    }

    public void ForceAdvance()
    {
        dialogueContainer.SetActive(false);
        gameObject.SetActive(false);
        fastDialogue = false;
        textFinishedDisplaying = false;
        isTalking = false;
        counter++;
        currentDialogueIndex = 0;
    }
}