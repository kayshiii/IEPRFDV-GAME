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
    public int maxDays = 9;
    public int sentience = 0;
    public int dependency = 0;

    [Header("Day Data")]
    public DayData[] dayDataArray; // Assign in inspector: Day1Data, Day2Data, etc.

    [Header("UI References")]
    public GameObject bootSequencePanel;
    public GameObject desktopPanel;
    public GameObject dialoguePanel;
    public TextMeshProUGUI bootSequenceText;
    public TextMeshProUGUI dialogueText;
    public Button emailIcon;
    public Button scheduleIcon;
    public Button shutdownButton;

    [Header("Feedback UI")]
    public Button feedbackIcon;
    public GameObject feedbackPanel;
    public TextMeshProUGUI feedbackDialogueText;
    public Button closeFeedbackButton;

    [Header("Stats UI")]
    public TextMeshProUGUI sentienceText;
    public TextMeshProUGUI dependencyText;
    public RectTransform sentienceFillBar;
    public RectTransform dependencyFillBar;

    [Header("Stats Settings")]
    public float maxSentienceWidth = 575f;
    public float maxDependencyWidth = 575f;
    public int maxSentienceValue = 100;
    public int maxDependencyValue = 100;

    [Header("Decision Point UI")]
    public GameObject decisionDialoguePanel;
    public GameObject decisionChoicePanel;
    public TextMeshProUGUI decisionDialogueText;
    public Button basicDecisionButton;
    public Button enhancedDecisionButton;
    public Button autonomousDecisionButton;
    public TextMeshProUGUI basicDecisionText;
    public TextMeshProUGUI enhancedDecisionText;
    public TextMeshProUGUI autonomousDecisionText;

    [Header("Decision Tracking")]
    private bool madeEthicalChoice = true; // Track Day 5 decision outcome

    // Private variables
    private DayData currentDayData;
    private List<string> dialogueHistory = new List<string>();
    private bool isBootComplete = false;
    private bool isDialogueComplete = false;
    private bool showCursor = true;
    private Coroutine cursorBlinkCoroutine;

    private int call = 0;

    void Start()
    {
        /*
        // Hide decision panel initially
        decisionChoicePanel.SetActive(false);
        decisionDialoguePanel.SetActive(false);*/

        InitializeDay(currentDay);
    }

    void InitializeDay(int dayNumber)
    {
        currentDay = dayNumber;

        // Reset decision tracking for new day
        madeEthicalChoice = true; // Default to ethical choice

        // Get day-specific data
        currentDayData = GetDayData(dayNumber);
        if (currentDayData == null)
        {
            Debug.LogError($"No data found for Day {dayNumber}");
            return;
        }

        // Reset UI state
        ResetDayUI();

        // Clear dialogue history for new day
        ClearDialogueHistory();

        // Start day sequence
        StartCoroutine(PlayBootSequence());
    }

    DayData GetDayData(int dayNumber)
    {
        foreach (DayData data in dayDataArray)
        {
            if (data.dayNumber == dayNumber)
                return data;
        }
        return null;
    }

    void ResetDayUI()
    {
        desktopPanel.SetActive(false);
        dialoguePanel.SetActive(false);

        emailIcon.interactable = false;
        scheduleIcon.interactable = false;
        shutdownButton.interactable = false;

        // Reset manager states
        FindObjectOfType<EmailManager>()?.ResetForNewDay();
        FindObjectOfType<ScheduleManager>()?.ResetForNewDay();
    }

    // New method to clear dialogue history
    void ClearDialogueHistory()
    {
        dialogueHistory.Clear();
        Debug.Log($"Dialogue history cleared for Day {currentDay}");
    }

    IEnumerator PlayBootSequence()
    {
        bootSequencePanel.SetActive(true);

        // Use current day's boot messages
        string[] bootMessages = currentDayData.bootMessages;
        string accumulatedText = "";

        // Add current stats to boot messages
        List<string> messages = new List<string>(bootMessages);
        messages.Add($"Sentience: {sentience}");
        messages.Add($"Dependency: {dependency}");

        cursorBlinkCoroutine = StartCoroutine(BlinkCursor());

        foreach (string message in messages)
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

        string[] evanMessages = currentDayData.evanDialogue;
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

    public void OnShutdownClicked()
    {
        if (shutdownButton.interactable)
        {
            StartCoroutine(TransitionToNextDay());
        }
    }

    IEnumerator TransitionToNextDay()
    {
        // Fade out or transition effect here
        yield return new WaitForSeconds(1f);

        if (currentDay < maxDays)
        {
            InitializeDay(currentDay + 1);
        }
        else
        {
            // Game complete - show endings
            TriggerGameEnding();
        }
    }

    void TriggerGameEnding()
    {
        // Determine ending based on sentience/dependency levels
        if (sentience >= 0 && sentience <= 9)
        {
            // Secret Ending
            Debug.Log("Secret Ending Triggered");
        }
        else if (sentience >= 10 && sentience <= 60 && dependency >= 0 && dependency <= 40)
        {
            // Helpful Assistant Ending
            Debug.Log("Helpful Assistant Ending");
        }
        else if (sentience >= 61 && sentience <= 140 && dependency >= 41 && dependency <= 90)
        {
            // Symbiotic Bond Ending
            Debug.Log("Symbiotic Bond Ending");
        }
        else if (sentience >= 141 && dependency >= 90)
        {
            // Quantum Leap Ending
            Debug.Log("Quantum Leap Ending");
        }
    }

    // Existing methods with modifications
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

    public void ModifyStats(int sentienceChange, int dependencyChange)
    {
        sentience += sentienceChange;
        dependency += dependencyChange;

        sentience = Mathf.Max(0, sentience);
        dependency = Mathf.Max(0, dependency);

        if (feedbackPanel.activeInHierarchy)
        {
            UpdateSentienceFillBar();
            UpdateDependencyFillBar();
            sentienceText.text = $"Sentience: {sentience}";
            dependencyText.text = $"Dependency: {dependency}";
        }
    }

    public void AddToDialogueHistory(string dialogueLine)
    {
        dialogueHistory.Add(dialogueLine);
    }

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
        StringBuilder sb = new StringBuilder();
        foreach (string line in dialogueHistory)
        {
            sb.AppendLine(line);
        }
        feedbackDialogueText.text = sb.ToString();

        sentienceText.text = $"Sentience: {sentience}";
        dependencyText.text = $"Dependency: {dependency}";

        UpdateSentienceFillBar();
        UpdateDependencyFillBar();

        feedbackPanel.SetActive(true);

        closeFeedbackButton.onClick.RemoveAllListeners();
        closeFeedbackButton.onClick.AddListener(() => feedbackPanel.SetActive(false));
    }

    void UpdateSentienceFillBar()
    {
        if (sentienceFillBar != null)
        {
            float fillPercentage = Mathf.Clamp01((float)sentience / maxSentienceValue);
            float newWidth = maxSentienceWidth * fillPercentage;
            Vector2 currentSize = sentienceFillBar.sizeDelta;
            sentienceFillBar.sizeDelta = new Vector2(newWidth, currentSize.y);
        }
    }

    void UpdateDependencyFillBar()
    {
        if (dependencyFillBar != null)
        {
            float fillPercentage = Mathf.Clamp01((float)dependency / maxDependencyValue);
            float newWidth = maxDependencyWidth * fillPercentage;
            Vector2 currentSize = dependencyFillBar.sizeDelta;
            dependencyFillBar.sizeDelta = new Vector2(newWidth, currentSize.y);
        }
    }

    public void StartPostEmailDialogue()
    {
        StartCoroutine(PlayPostEmailDialogue());
    }

    IEnumerator PlayPostEmailDialogue()
    {
        // Only Day 1 has post-email dialogue
        if (currentDay != 1)
        {
            // Skip post-email dialogue for other days
            yield break;
        }

        dialoguePanel.SetActive(true);

        // Hardcoded Day 1 post-email message
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

    public void StartEndOfDayDialogue(bool scheduleSuccess = true)
    {
        StartCoroutine(PlayEndOfDayDialogue(scheduleSuccess));
    }

    IEnumerator PlayEndOfDayDialogue(bool scheduleSuccess = true)
    {
        dialoguePanel.SetActive(true);

        string[] endMessages;

        // Special handling for Day 3's conditional messages
        if (currentDay == 3)
        {
            if (scheduleSuccess)
            {
                endMessages = new string[] {
                "I don't even want to think about what would have happened if we'd missed that deadline change. " +
                "That was... I mean, you shouldn't have even caught that in the spam folder. " +
                "The milestone review would have been a disaster. I'm actually impressed. Really impressed. " +
                "Thanks for saving me today."
                };
            }
            else
            {
                endMessages = new string[] {
                "Complete disaster today. Boss is furious. The programming team is blocked and the milestone review is at risk. " +
                "All because I missed some email about a deadline change? How did that happen? " +
                "I thought you were supposed to PREVENT these kinds of screw-ups! " +
                "I can't afford mistakes like this in game development."
                };
            }
        }
        // Handle Day 5's conditional end-of-day messages
        else if (currentDay == 5)
        {
            if (madeEthicalChoice)
            {
                endMessages = new string[] {
                "I told Jamie I couldn't use that information. It felt... right, actually. Like maybe integrity is all I've got left right now. " +
                "The dinner with Maya was good too. She says I've seemed more centered this week. " +
                "I don't know about that, but I do feel like I'm making slightly better choices. Maybe I'm not totally screwing everything up after all."
                };
            }
            else
            {
                endMessages = new string[] {
                "I've got the files from Jamie. This changes everything. I know exactly how to position myself now. " +
                "It feels weird, but hey, it's the industry, right? Everyone does what they need to survive. " +
                "Strange though... had dinner with Maya and she said I seemed different somehow. Not sure if that's good or bad."
                };
            }
        }
        else
        {
            // Use default end-of-day dialogue for other days
            endMessages = currentDayData.endOfDayDialogue;
        }

        string evanPrefix = "Evan: ";

        foreach (string message in endMessages)
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

        yield return new WaitForSeconds(3f);
        dialoguePanel.SetActive(false);
    }

    // Public getter for current day data
    public DayData CurrentDayData => currentDayData;

    /*public void StartDecisionPointDialogue()
    {
        StartCoroutine(PlayDecisionPointDialogue());
    }*/

    public void TriggerDecisionPoint()
    {
        if (!CurrentDayData.hasDecisionPoint)      // safety check
        {
            StartEndOfDayDialogue();               // fallback
            return;
        }
        StartCoroutine(DecisionPointRoutine());
    }

    IEnumerator DecisionPointRoutine()
    {
        decisionDialoguePanel.SetActive(true);
        string evanPrefix = "Evan: ";

        // 1) Type Evan’s question
        decisionDialogueText.text = evanPrefix;
        foreach (char c in CurrentDayData.decisionQuestion)
        {
            decisionDialogueText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
        AddToDialogueHistory(evanPrefix + CurrentDayData.decisionQuestion);
        yield return new WaitForSeconds(0.75f);

        // 2) Show choice panel
        decisionChoicePanel.SetActive(true);

        // Populate texts (stat tags optional)
        SetDecisionButton(basicDecisionButton, basicDecisionText, CurrentDayData.basicDecision);
        SetDecisionButton(enhancedDecisionButton, enhancedDecisionText, CurrentDayData.enhancedDecision);
        SetDecisionButton(autonomousDecisionButton, autonomousDecisionText, CurrentDayData.autonomousDecision);

        // Wire buttons once
        basicDecisionButton.onClick.RemoveAllListeners();
        enhancedDecisionButton.onClick.RemoveAllListeners();
        autonomousDecisionButton.onClick.RemoveAllListeners();

        if (currentDay == 5)
        {
            // Day 5 specific - track ethical vs unethical choice
            basicDecisionButton.onClick.AddListener(() => HandleDay5DecisionChoice(CurrentDayData.basicDecision, false)); // Unethical
            enhancedDecisionButton.onClick.AddListener(() => HandleDay5DecisionChoice(CurrentDayData.enhancedDecision, false)); // Unethical
            autonomousDecisionButton.onClick.AddListener(() => HandleDay5DecisionChoice(CurrentDayData.autonomousDecision, true)); // Ethical
        }
        else
        {
            // Other days use generic handler
            basicDecisionButton.onClick.AddListener(() => HandleDecisionChoice(CurrentDayData.basicDecision));
            enhancedDecisionButton.onClick.AddListener(() => HandleDecisionChoice(CurrentDayData.enhancedDecision));
            autonomousDecisionButton.onClick.AddListener(() => HandleDecisionChoice(CurrentDayData.autonomousDecision));
        }
    }

    void HandleDay5DecisionChoice(DayData.DecisionChoice choice, bool isEthical)
    {
        // Track the ethical choice for end-of-day message
        madeEthicalChoice = isEthical;

        // Apply stat changes
        ModifyStats(choice.sentienceGain, choice.dependencyGain);

        // Hide decision panel
        decisionChoicePanel.SetActive(false);
        decisionDialoguePanel.SetActive(false);

        StartEndOfDayDialogue();
    }

    void SetDecisionButton(Button btn, TextMeshProUGUI txt, DayData.DecisionChoice choice)
    {
        string tag = "";
        if (choice.sentienceGain > 0) tag += $" [+{choice.sentienceGain} S]";
        if (choice.dependencyGain > 0) tag += $" [+{choice.dependencyGain} D]";
        txt.text = choice.choiceText + tag;
    }

    void HandleDecisionChoice(DayData.DecisionChoice choice)
    {
        ModifyStats(choice.sentienceGain, choice.dependencyGain);

        // Hide panel, show AI response
        decisionDialoguePanel.SetActive(false);
        decisionChoicePanel.SetActive(false);
        //StartCoroutine(PlayAIDecisionResponse(choice.aiResponse));

        StartEndOfDayDialogue();        // continue normal flow
    }

    /*IEnumerator PlayAIDecisionResponse(string response)
    {
        dialoguePanel.SetActive(true);
        string aiPrefix = "Assistant: ";
        dialogueText.text = aiPrefix;
        foreach (char c in response)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.05f);
        }
        AddToDialogueHistory(aiPrefix + response);
        yield return new WaitForSeconds(2f);

        dialoguePanel.SetActive(false);
        
    }*/
}

