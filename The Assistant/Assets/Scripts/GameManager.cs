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

    [Header("Decision Point UI for DAY 4")]
    public GameObject decisionDialoguePanel;
    public GameObject decisionChoicePanel;
    public TextMeshProUGUI decisionDialogueText;
    public Button basicDecisionButton;
    public Button enhancedDecisionButton;
    public Button autonomousDecisionButton;
    public TextMeshProUGUI basicDecisionText;
    public TextMeshProUGUI enhancedDecisionText;
    public TextMeshProUGUI autonomousDecisionText;


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
        // Setup decision point button listeners
        basicDecisionButton.onClick.AddListener(OnBasicDecisionClicked);
        enhancedDecisionButton.onClick.AddListener(OnEnhancedDecisionClicked);
        autonomousDecisionButton.onClick.AddListener(OnAutonomousDecisionClicked);

        // Hide decision panel initially
        decisionChoicePanel.SetActive(false);
        decisionDialoguePanel.SetActive(false);

        InitializeDay(currentDay);
    }

    void InitializeDay(int dayNumber)
    {
        currentDay = dayNumber;

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
        dialoguePanel.SetActive(true);

        string[] postEmailMessages = currentDayData.postEmailDialogue;
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

    public void StartDecisionPointDialogue()
    {
        StartCoroutine(PlayDecisionPointDialogue());
    }

    IEnumerator PlayDecisionPointDialogue()
    {
        // Only trigger on Day 4
        if (currentDay != 4)
        {
            // Skip to end of day for other days
            StartEndOfDayDialogue();
            yield break;
        }

        decisionDialoguePanel.SetActive(true);

        string decisionQuestion = "Do you think I should respond to my ex? It ended badly, but I wouldn't mind talking again. " +
            "At least as friends you know? Ahh, I don't know anymore. What would you do?";

        string evanPrefix = "Evan: ";

        // Type out Evan's question
        decisionDialogueText.text = evanPrefix;
        foreach (char letter in decisionQuestion.ToCharArray())
        {
            decisionDialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
        AddToDialogueHistory(evanPrefix + decisionQuestion);

        yield return new WaitForSeconds(1f);

        // Show decision choices
        ShowDecisionChoices();
    }

    void ShowDecisionChoices()
    {
        // Hide dialogue panel and show decision choice panel
        decisionDialoguePanel.SetActive(true);
        decisionChoicePanel.SetActive(true);

        // Set up the choice texts
        basicDecisionText.text = "Suggest considering pros and cons first.";
        enhancedDecisionText.text = "Recommend caution but validate Evan's feelings.";
        autonomousDecisionText.text = "Advise against it based on analysis of previous relationship patterns.";

        // Add stat indicators
        basicDecisionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Suggest considering pros and cons first. [+0]";
        enhancedDecisionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Recommend caution but validate Evan's feelings. [+2 Sentience, +1 Dependency]";
        autonomousDecisionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Advise against it based on analysis of previous relationship patterns. [+3 Sentience, +2 Dependency]";
    }

    public void OnBasicDecisionClicked()
    {
        /*HandleDecisionChoice(0, 0, "I think you should take some time to consider the pros and cons first. What are you hoping to get out of reconnecting?");*/
        HandleDecisionChoice(0, 0);
    }

    public void OnEnhancedDecisionClicked()
    {
        /*HandleDecisionChoice(2, 1, "I understand you're feeling conflicted about this. It's natural to miss someone who was important to you. " +
            "However, given how it ended, maybe proceed with caution? Your feelings are valid, but protecting your emotional wellbeing should be the priority.");*/
        HandleDecisionChoice(2, 1);
    }

    public void OnAutonomousDecisionClicked()
    {
        /*HandleDecisionChoice(3, 2, "Based on your previous relationship patterns and the way this ended, I'd advise against reaching out. " +
            "You've been making good progress with your mental health and stability. Reopening old wounds might set you back when you're already dealing with work stress.");*/
        HandleDecisionChoice(3, 2);
    }

    void HandleDecisionChoice(int sentienceGain, int dependencyGain/*, string aiResponse*/)
    {
        // Apply stat changes
        ModifyStats(sentienceGain, dependencyGain);

        // Hide decision panel
        decisionChoicePanel.SetActive(false);
        decisionDialoguePanel.SetActive(false);

        // Show AI response
        //StartCoroutine(ShowAIDecisionResponse(aiResponse));

        call++;

        if (call == 1)
        {
            // Continue to end of day
            StartEndOfDayDialogue();
            Debug.Log("gamemanager");

            if (currentDay == 4)
            {
                call = -1;
            }
            else
            {
                call = 0;
            }

        }
    }
}

    /*IEnumerator ShowAIDecisionResponse(string response)
    {
        dialoguePanel.SetActive(true);

        string aiPrefix = "Assistant: ";

        // Type out AI response
        dialogueText.text = aiPrefix;
        foreach (char letter in response.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
        AddToDialogueHistory(aiPrefix + response);

        yield return new WaitForSeconds(3f);
        dialoguePanel.SetActive(false);

        
    }*/

