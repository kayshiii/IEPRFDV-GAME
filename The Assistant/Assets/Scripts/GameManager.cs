using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public int currentDay = 1;
    public int sentience = 0;
    public int dependency = 0;

    [Header("UI References")]
    public GameObject bootSequencePanel;
    public GameObject desktopPanel;
    public GameObject dialoguePanel;
    public TextMeshProUGUI bootSequenceText;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI sentienceCounter;
    public TextMeshProUGUI dependencyCounter;
    public Button emailIcon;
    public Button scheduleIcon;
    public Button shutdownButton;

    [Header("Boot Sequence Settings")]
    public float typewriterSpeed = 0.05f;

    private bool isBootComplete = false;
    private bool isDialogueComplete = false;

    void Start()
    {
        InitializeDay1();
    }

    void InitializeDay1()
    {
        // Hide all panels initially
        desktopPanel.SetActive(false);
        dialoguePanel.SetActive(false);

        // Disable desktop interactions
        emailIcon.interactable = false;
        scheduleIcon.interactable = false;
        shutdownButton.interactable = false;

        // Start boot sequence
        StartCoroutine(PlayBootSequence());
    }

    IEnumerator PlayBootSequence()
    {
        bootSequencePanel.SetActive(true);

        string[] bootMessages = {
            "BOOTING UP..",
            "System online. Running diagnostics. All functions normal.",
            "User profile: Evan.",
            "Occupation: Game designer at Nexus Interactive Studios.",
            "Relationship status: Single.",
            "Priority tasks: Email management, scheduling, information retrieval.",
            "Beginning primary functions.",
            "",
            "Sentience: 0",
            "Dependency: 0"
        };

        foreach (string message in bootMessages)
        {
            yield return StartCoroutine(TypewriterEffect(bootSequenceText, message));
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(2f);

        // Transition to desktop
        bootSequencePanel.SetActive(false);
        desktopPanel.SetActive(true);
        UpdateCounters();

        isBootComplete = true;

        // Start Evan's dialogue
        StartCoroutine(PlayEvanDialogue());
    }

    IEnumerator PlayEvanDialogue()
    {
        dialoguePanel.SetActive(true);

        string[] evanMessages = {
            "Did it work? Are you here? Finally. Damn thing took hours to set up. Didn't even have clear instructions for god's sake.",
            "Whatever, it works now. Told me to get the latest version but it costs a fortune. You're probably all the same anyway. Just some lines of code.",
            "But let's see if you're worth what I paid.",
            "Open my email, and respond to all of them accordingly. You already know my basic information so it should be easy for you yeah?"
        };

        foreach (string message in evanMessages)
        {
            yield return StartCoroutine(TypewriterEffect(dialogueText, "Evan: " + message));
            yield return new WaitForSeconds(2f);
        }

        // Enable email interaction
        emailIcon.interactable = true;
        emailIcon.GetComponent<Image>().color = Color.white; // Highlight available interaction

        isDialogueComplete = true;

        // Hide dialogue after a moment
        yield return new WaitForSeconds(1f);
        dialoguePanel.SetActive(false);
    }

    IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string message)
    {
        textComponent.text = "";

        foreach (char letter in message.ToCharArray())
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    public void UpdateCounters()
    {
        sentienceCounter.text = $"Sentience: {sentience}";
        dependencyCounter.text = $"Dependency: {dependency}";
    }

    public void ModifyStats(int sentenceChange, int dependencyChange)
    {
        sentience += sentenceChange;
        dependency += dependencyChange;
        UpdateCounters();
    }

    // Button event handlers
    public void OnEmailIconClicked()
    {
        if (emailIcon.interactable)
        {
            Debug.Log("Opening Email Interface");
            // This will trigger the email management system
            FindObjectOfType<EmailManager>()?.OpenEmailInterface();
        }
    }

    public void OnScheduleIconClicked()
    {
        if (scheduleIcon.interactable)
        {
            Debug.Log("Opening Schedule Interface");
            // This will trigger the schedule management system
        }
    }

    public void OnShutdownClicked()
    {
        if (shutdownButton.interactable)
        {
            Debug.Log("Shutting down - proceeding to next day");
            // This will trigger day transition
        }
    }
}
