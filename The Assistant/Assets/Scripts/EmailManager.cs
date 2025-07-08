using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class EmailChoice
{
    public string choiceText;
    public int sentienceModifier;
    public int dependencyModifier;
    public string resultDescription;
}

[System.Serializable]
public enum EmailType
{
    Important,
    Work,
    Personal,
    Spam
}

[System.Serializable]
public class Email
{
    public string sender;
    public string subject;
    public string content;
    public EmailType emailType;
    public EmailChoice basicResponse;
    public EmailChoice enhancedResponse;
    public EmailChoice autonomousDecision;
}

public class EmailManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject emailInterface;
    public GameObject emailListPanel;
    public GameObject emailDetailPanel;
    public Transform emailListContent;
    public GameObject emailItemPrefab;

    [Header("Email Detail UI")]
    public TextMeshProUGUI senderText;
    public TextMeshProUGUI subjectText;
    public TextMeshProUGUI contentText;
    public Button basicResponseButton;
    public Button enhancedResponseButton;
    public Button autonomousDecisionButton;
    public TextMeshProUGUI basicResponseText;
    public TextMeshProUGUI enhancedResponseText;
    public TextMeshProUGUI autonomousDecisionText;

    [Header("Navigation")]
    public Button backToListButton;
    public Button closeEmailButton;

    [Header("Filter UI")]
    public Button importantFilterButton;
    public Button workFilterButton;
    public Button personalFilterButton;
    public Button spamFilterButton;
    public Button allFilterButton;
    public TextMeshProUGUI currentFilterText;

    [Header("Completion State")]
    private bool hasCompletedEmails = false;
    public GameObject completionMessagePanel; // UI panel to show completion message
    public TextMeshProUGUI completionMessageText; // Text component for the message

    private EmailType currentFilter = EmailType.Important;
    private bool showAllEmails = false;
    private List<Email> currentDayEmails;
    private Email currentEmail;
    private GameManager gameManager;
    private bool emailsAttempted = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        // Setup button listeners
        basicResponseButton.onClick.AddListener(() => HandleEmailChoice(currentEmail.basicResponse));
        enhancedResponseButton.onClick.AddListener(() => HandleEmailChoice(currentEmail.enhancedResponse));
        autonomousDecisionButton.onClick.AddListener(() => HandleEmailChoice(currentEmail.autonomousDecision));
        backToListButton.onClick.AddListener(ShowEmailList);
        closeEmailButton.onClick.AddListener(CloseEmailInterface);

        // Setup filter button listeners
        importantFilterButton.onClick.AddListener(() => SetEmailFilter(EmailType.Important));
        workFilterButton.onClick.AddListener(() => SetEmailFilter(EmailType.Work));
        personalFilterButton.onClick.AddListener(() => SetEmailFilter(EmailType.Personal));
        spamFilterButton.onClick.AddListener(() => SetEmailFilter(EmailType.Spam));
        allFilterButton.onClick.AddListener(() => ShowAllEmails());

        UpdateFilterText();
        emailInterface.SetActive(false);
    }

    public void ResetForNewDay()
    {
        if (currentDayEmails != null)
            currentDayEmails.Clear();

        // Reset completion states for new day
        hasCompletedEmails = false;
        emailsAttempted = false;
    }

    public void OpenEmailInterface()
    {
        // Check if emails have already been completed/attempted
        if (hasCompletedEmails || emailsAttempted)
        {
            emailInterface.SetActive(true);
            ShowCompletionMessage();
            return;
        }

        // Get current day's emails from GameManager
        DayData dayData = gameManager.CurrentDayData;
        if (dayData != null)
        {
            currentDayEmails = new List<Email>(dayData.emails);
        }

        emailInterface.SetActive(true);
        ShowEmailList();
    }

    void ShowCompletionMessage()
    {
        // Hide the main email panels
        emailListPanel.SetActive(false);
        emailDetailPanel.SetActive(false);
        completionMessagePanel.SetActive(true);

        if (hasCompletedEmails)
        {
            completionMessageText.text = "All emails have been processed for today!";
        }
        else
        {
            completionMessageText.text = "Email processing session completed. Check back tomorrow!";
        }

        // Auto-hide after 3 seconds
        StartCoroutine(HideCompletionMessage());
    }

    IEnumerator HideCompletionMessage()
    {
        yield return new WaitForSeconds(3f);
        completionMessagePanel.SetActive(false);
        emailInterface.SetActive(false);
    }

    void ShowEmailList()
    {
        emailListPanel.SetActive(true);
        emailDetailPanel.SetActive(false);

        // Clear existing email items
        for (int i = emailListContent.childCount - 1; i >= 0; i--)
        {
            Destroy(emailListContent.GetChild(i).gameObject);
        }

        List<Email> filteredEmails = GetFilteredEmails();

        // Create email list items for filtered emails
        for (int i = 0; i < filteredEmails.Count; i++)
        {
            GameObject emailItem = Instantiate(emailItemPrefab, emailListContent);

            // Position adjustment
            RectTransform rectTransform = emailItem.GetComponent<RectTransform>();
            Vector2 currentPos = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(currentPos.x, currentPos.y - (150f * i));

            // Setup email item display
            TextMeshProUGUI senderText = emailItem.transform.Find("SenderText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI subjectText = emailItem.transform.Find("SubjectText").GetComponent<TextMeshProUGUI>();
            Button emailButton = emailItem.GetComponent<Button>();

            senderText.text = filteredEmails[i].sender;
            subjectText.text = filteredEmails[i].subject;

            AddEmailTypeIndicator(emailItem, filteredEmails[i].emailType);

            // Add click listener
            Email emailRef = filteredEmails[i];
            emailButton.onClick.AddListener(() => ShowEmailDetail(emailRef));
        }
    }

    void AddEmailTypeIndicator(GameObject emailItem, EmailType emailType)
    {
        Image indicator = emailItem.transform.Find("TypeIndicator")?.GetComponent<Image>();
        if (indicator == null) return;

        switch (emailType)
        {
            case EmailType.Important:
                indicator.color = Color.red;
                break;
            case EmailType.Work:
                indicator.color = Color.blue;
                break;
            case EmailType.Personal:
                indicator.color = Color.green;
                break;
            case EmailType.Spam:
                indicator.color = Color.gray;
                break;
        }
    }

    void ShowEmailDetail(Email email)
    {
        currentEmail = email;
        emailDetailPanel.SetActive(true);

        // Populate email details
        senderText.text = email.sender;
        subjectText.text = email.subject;
        contentText.text = email.content;

        // Setup choice buttons
        basicResponseText.text = email.basicResponse.choiceText;
        enhancedResponseText.text = email.enhancedResponse.choiceText;
        autonomousDecisionText.text = email.autonomousDecision.choiceText;

        // Add stat indicators to buttons
        UpdateChoiceButtonText(basicResponseButton, email.basicResponse);
        UpdateChoiceButtonText(enhancedResponseButton, email.enhancedResponse);
        UpdateChoiceButtonText(autonomousDecisionButton, email.autonomousDecision);
    }

    void UpdateChoiceButtonText(Button button, EmailChoice choice)
    {
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        string statText = "";

        if (choice.sentienceModifier > 0)
            statText += $" [+{choice.sentienceModifier} Sentience]";
        if (choice.dependencyModifier > 0)
            statText += $" [+{choice.dependencyModifier} Dependency]";

        buttonText.text = choice.choiceText + statText;
    }

    void HandleEmailChoice(EmailChoice choice)
    {
        gameManager.ModifyStats(choice.sentienceModifier, choice.dependencyModifier);

        Debug.Log($"Choice made: {choice.choiceText}");
        Debug.Log($"Result: {choice.resultDescription}");

        currentDayEmails.Remove(currentEmail);

        if (currentDayEmails.Count == 0)
        {
            // All emails processed - mark as completed
            hasCompletedEmails = true;
            emailsAttempted = true;
            CloseEmailInterface();
            EnableScheduleInteraction();
        }
        else
        {
            ShowEmailList();
        }
    }

    public void SetEmailFilter(EmailType filterType)
    {
        currentFilter = filterType;
        showAllEmails = false;
        UpdateFilterButtonVisuals();
        UpdateFilterText();
        ShowEmailList();
    }

    public void ShowAllEmails()
    {
        showAllEmails = true;
        UpdateFilterButtonVisuals();
        UpdateFilterText();
        ShowEmailList();
    }

    void UpdateFilterButtonVisuals()
    {
        // Reset all button colors
        importantFilterButton.GetComponent<Image>().color = Color.white;
        workFilterButton.GetComponent<Image>().color = Color.white;
        personalFilterButton.GetComponent<Image>().color = Color.white;
        spamFilterButton.GetComponent<Image>().color = Color.white;
        allFilterButton.GetComponent<Image>().color = Color.white;

        /*// Highlight active filter
        if (showAllEmails)
        {
            allFilterButton.GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            switch (currentFilter)
            {
                case EmailType.Important:
                    importantFilterButton.GetComponent<Image>().color = Color.yellow;
                    break;
                case EmailType.Work:
                    workFilterButton.GetComponent<Image>().color = Color.yellow;
                    break;
                case EmailType.Personal:
                    personalFilterButton.GetComponent<Image>().color = Color.yellow;
                    break;
                case EmailType.Spam:
                    spamFilterButton.GetComponent<Image>().color = Color.yellow;
                    break;
            }
        }*/
    }

    void UpdateFilterText()
    {
        if (currentFilterText == null) return;

        if (showAllEmails)
        {
            currentFilterText.text = "Showing: All Emails";
        }
        else
        {
            string filterName = "";
            switch (currentFilter)
            {
                case EmailType.Important:
                    filterName = "Important";
                    break;
                case EmailType.Work:
                    filterName = "Work";
                    break;
                case EmailType.Personal:
                    filterName = "Personal";
                    break;
                case EmailType.Spam:
                    filterName = "Spam";
                    break;
            }
            currentFilterText.text = $"Showing: {filterName}";
        }
    }

    List<Email> GetFilteredEmails()
    {
        if (showAllEmails)
            return currentDayEmails;

        return currentDayEmails.Where(email => email.emailType == currentFilter).ToList();
    }

    void EnableScheduleInteraction()
    {
        gameManager.scheduleIcon.interactable = true;
        gameManager.scheduleIcon.GetComponent<Image>().color = Color.white;

        if (gameManager.currentDay == 1)
        {
            gameManager.StartPostEmailDialogue();
            Debug.Log("All emails processed! Schedule interaction now available.");
        }
        else
        Debug.Log("All emails processed! Schedule interaction now available.");
    }

    public void CloseEmailInterface()
    {
        emailInterface.SetActive(false);
    }
}
