using System.Collections;
using System.Collections.Generic;
using System.Text;
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
    public Button emailIcon;
    public Button scheduleIcon;
    public Button shutdownButton;

    // --- Feedback UI ---
    [Header("Feedback UI")]
    public Button feedbackIcon;
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackDialogueText;
    public TextMeshProUGUI feedbackStatsText;
    public Button closeFeedbackButton;

    // --- Dialogue History ---
    private List<string> dialogueHistory = new List<string>();

    // --- Other State ---
    private bool isBootComplete = false;
    private bool isDialogueComplete = false;
    private bool showCursor = true;
    private Coroutine cursorBlinkCoroutine;

    void Start()
    {
        InitializeDay1();
    }

    void InitializeDay1()
    {
        desktopPanel.SetActive(false);
        dialoguePanel.SetActive(false);

        emailIcon.interactable = false;
        scheduleIcon.interactable = false;
        shutdownButton.interactable = false;

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

        string accumulatedText = "";

        cursorBlinkCoroutine = StartCoroutine(BlinkCursor());

        foreach (string message in bootMessages)
        {
            if (!string.IsNullOrEmpty(accumulatedText))
                accumulatedText += "\n";
            accumulatedText += message;

            yield return StartCoroutine(TypewriterEffectAccumulativeWithCursor(bootSequenceText, accumulatedText, message));
            yield return new WaitForSeconds(0.5f);
        }

        StopCoroutine(cursorBlinkCoroutine);
        bootSequenceText.text = accumulatedText;

        yield return new WaitForSeconds(2f);

        bootSequencePanel.SetActive(false);
        desktopPanel.SetActive(true);

        isBootComplete = true;

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

        string evanPrefix = "Evan: ";

        foreach (string message in evanMessages)
        {
            dialogueText.text = evanPrefix;
            foreach (char letter in message.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(0.05f);
            }
            AddToDialogueHistory(evanPrefix + message);
            yield return new WaitForSeconds(2f);
        }

        emailIcon.interactable = true;
        emailIcon.GetComponent<Image>().color = Color.white;

        isDialogueComplete = true;

        yield return new WaitForSeconds(1f);
        dialoguePanel.SetActive(false);
    }

    IEnumerator BlinkCursor()
    {
        while (true)
        {
            showCursor = !showCursor;
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator TypewriterEffectAccumulativeWithCursor(TextMeshProUGUI textComponent, string fullText, string newLine)
    {
        int startIndex = fullText.Length - newLine.Length;
        string baseText = fullText.Substring(0, startIndex);
        textComponent.text = baseText + (showCursor ? "_" : "");

        string currentTypedText = "";
        foreach (char letter in newLine.ToCharArray())
        {
            currentTypedText += letter;
            string displayText = baseText + currentTypedText + (showCursor ? "_" : "");
            textComponent.text = displayText;
            yield return new WaitForSeconds(0.05f);
        }
    }

    // --- Stat Management ---
    public void ModifyStats(int sentienceChange, int dependencyChange)
    {
        sentience += sentienceChange;
        dependency += dependencyChange;
    }

    // --- Dialogue History Tracking ---
    public void AddToDialogueHistory(string dialogueLine)
    {
        dialogueHistory.Add(dialogueLine);
    }

    // --- Button Event Handlers ---
    public void OnEmailIconClicked()
    {
        if (emailIcon.interactable)
        {
            Debug.Log("Opening Email Interface");
            FindObjectOfType<EmailManager>()?.OpenEmailInterface();
        }
    }

    public void OnScheduleIconClicked()
    {
        if (scheduleIcon.interactable)
        {
            Debug.Log("Opening Schedule Interface");
            ScheduleManager scheduleManager = FindObjectOfType<ScheduleManager>();
            if (scheduleManager != null)
            {
                scheduleManager.OpenScheduleInterface();
            }
        }
    }

    public void OnShutdownClicked()
    {
        if (shutdownButton.interactable)
        {
            Debug.Log("Shutting down - proceeding to next day");
            // Trigger day transition logic here
        }
    }

    // --- Feedback Icon Handler ---
    public void OnFeedbackIconClicked()
    {
        if (feedbackIcon.interactable)
        {
            Debug.Log("Opening Feedback Panel");
            ShowFeedbackPanel();
        }
    }

    private void ShowFeedbackPanel()
    {
        // Build dialogue history string
        StringBuilder sb = new StringBuilder();
        foreach (string line in dialogueHistory)
        {
            sb.AppendLine(line);
        }
        feedbackDialogueText.text = sb.ToString();

        // Show current stats
        feedbackStatsText.text = $"Sentience: {sentience}\nDependency: {dependency}";

        feedbackPanel.SetActive(true);

        closeFeedbackButton.onClick.RemoveAllListeners();
        closeFeedbackButton.onClick.AddListener(() => feedbackPanel.SetActive(false));
    }

    // --- Dialogue Triggers ---
    public void StartPostEmailDialogue()
    {
        StartCoroutine(PlayPostEmailDialogue());
    }

    IEnumerator PlayPostEmailDialogue()
    {
        dialoguePanel.SetActive(true);

        string[] postEmailMessages = {
            "Wow that was quick. And efficient too. Alright now help me with my schedule for today. I don't wanna get burnt out too early you know?"
        };

        string evanPrefix = "Evan: ";

        foreach (string message in postEmailMessages)
        {
            dialogueText.text = evanPrefix;
            foreach (char letter in message.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(0.05f);
            }
            AddToDialogueHistory(evanPrefix + message);
            yield return new WaitForSeconds(2f);
        }

        yield return new WaitForSeconds(1f);
        dialoguePanel.SetActive(false);
    }

    public void StartEndOfDayDialogue()
    {
        StartCoroutine(PlayEndOfDayDialogue());
    }

    IEnumerator PlayEndOfDayDialogue()
    {
        dialoguePanel.SetActive(true);

        string endMessage = "Well, that wasn't terrible. At least I didn't miss my dentist appointment this time. Those reminder notifications actually helped. Let's try this again tomorrow, I guess.";

        dialogueText.text = "Evan: ";

        foreach (char letter in endMessage.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
        AddToDialogueHistory("Evan: " + endMessage);

        yield return new WaitForSeconds(3f);
        dialoguePanel.SetActive(false);
    }
}
