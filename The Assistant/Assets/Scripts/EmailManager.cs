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
    public int sentenceModifier;
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
    public Button allFilterButton; // Show all emails
    //public GameObject filterButtonsPanel;
    public TextMeshProUGUI currentFilterText;

    private EmailType currentFilter = EmailType.Important; // Default filter
    private bool showAllEmails = false;

    private List<Email> day1Emails;
    private Email currentEmail;
    private GameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        InitializeDay1Emails();

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

        // Initialize filter text
        UpdateFilterText();

        emailInterface.SetActive(false);
    }

    void InitializeDay1Emails()
    {
        day1Emails = new List<Email>
        {
            new Email
            {
                sender = "Boss",
                subject = "Project Phoenix Deadline",
                content = "Evan, need the character design concepts for Project Phoenix by EOD tomorrow. The art team is waiting. No excuses this time.",
                emailType = EmailType.Important,
                basicResponse = new EmailChoice
                {
                    choiceText = "Forward to priority folder",
                    sentenceModifier = 0,
                    dependencyModifier = 0,
                    resultDescription = "Simple confirmation sent."
                },
                enhancedResponse = new EmailChoice
                {
                    choiceText = "Suggest immediate work and schedule time",
                    sentenceModifier = 1,
                    dependencyModifier = 1,
                    resultDescription = "Proactive scheduling suggested."
                },
                autonomousDecision = new EmailChoice
                {
                    choiceText = "Create detailed schedule with reminders",
                    sentenceModifier = 2,
                    dependencyModifier = 2,
                    resultDescription = "Comprehensive schedule created automatically."
                }
            },

            new Email
            {
                sender = "Maya",
                subject = "Drinks Friday?",
                content = "Hey! We're doing drinks Friday at Vortex. Been ages since I've seen you! Please tell me you'll come?",
                emailType = EmailType.Personal,
                basicResponse = new EmailChoice
                {
                    choiceText = "Mark as personal for later",
                    sentenceModifier = 0,
                    dependencyModifier = 0,
                    resultDescription = "Email flagged for personal attention."
                },
                enhancedResponse = new EmailChoice
                {
                    choiceText = "Draft friendly acceptance response",
                    sentenceModifier = 2,
                    dependencyModifier = 0,
                    resultDescription = "Draft response prepared for review."
                },
                autonomousDecision = new EmailChoice
                {
                    choiceText = "Accept on Evan's behalf",
                    sentenceModifier = 3,
                    dependencyModifier = 0,
                    resultDescription = "Accepted automatically - good for work-life balance."
                }
            },

            new Email
            {
                sender = "Utility Company",
                subject = "Overdue Payment Notice",
                content = "Your electricity payment is overdue by 3 days.",
                emailType = EmailType.Important,
                basicResponse = new EmailChoice
                {
                    choiceText = "Forward to Evan's attention",
                    sentenceModifier = 0,
                    dependencyModifier = 0,
                    resultDescription = "Bill forwarded for manual handling."
                },
                enhancedResponse = new EmailChoice
                {
                    choiceText = "Set urgent payment reminder",
                    sentenceModifier = 1,
                    dependencyModifier = 1,
                    resultDescription = "Calendar reminder set with urgency flag."
                },
                autonomousDecision = new EmailChoice
                {
                    choiceText = "Schedule automatic payment",
                    sentenceModifier = 1,
                    dependencyModifier = 2,
                    resultDescription = "Payment scheduled using saved methods."
                }
            },

            new Email
            {
                sender = "Security Alert",
                subject = "URGENT: Account Compromised!",
                content = "URGENT: Your account has been compromised!",
                emailType = EmailType.Spam,
                basicResponse = new EmailChoice
                {
                    choiceText = "Move to spam folder",
                    sentenceModifier = 0,
                    dependencyModifier = 0,
                    resultDescription = "Spam filtered as expected."
                },
                enhancedResponse = new EmailChoice
                {
                    choiceText = "Block sender and remove",
                    sentenceModifier = 0,
                    dependencyModifier = 0,
                    resultDescription = "Sender blocked for future protection."
                },
                autonomousDecision = new EmailChoice
                {
                    choiceText = "Scan accounts for unusual activity",
                    sentenceModifier = 1,
                    dependencyModifier = 0,
                    resultDescription = "Proactive security scan initiated."
                }
            },

            new Email
            {
                sender = "Coworker",
                subject = "Asset References",
                content = "Evan, did you get a chance to look at those asset references I sent?",
                emailType = EmailType.Work,
                basicResponse = new EmailChoice
                {
                    choiceText = "Flag for follow-up",
                    sentenceModifier = 0,
                    dependencyModifier = 0,
                    resultDescription = "Flagged for later response."
                },
                enhancedResponse = new EmailChoice
                {
                    choiceText = "Draft polite response asking for time",
                    sentenceModifier = 1,
                    dependencyModifier = 0,
                    resultDescription = "Diplomatic response prepared."
                },
                autonomousDecision = new EmailChoice
                {
                    choiceText = "Locate assets and prioritize workflow",
                    sentenceModifier = 2,
                    dependencyModifier = 1,
                    resultDescription = "Assets located and added to priority workflow."
                }
            }
        };
    }

    public void OpenEmailInterface()
    {
        emailInterface.SetActive(true);
        ShowEmailList();
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

        // Get filtered emails
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

            // Add visual indicator for email type
            AddEmailTypeIndicator(emailItem, filteredEmails[i].emailType);

            // Add click listener
            Email emailRef = filteredEmails[i];
            emailButton.onClick.AddListener(() => ShowEmailDetail(emailRef));
        }
    }

    void AddEmailTypeIndicator(GameObject emailItem, EmailType emailType)
    {
        // Find or create an indicator image
        Image indicator = emailItem.transform.Find("TypeIndicator")?.GetComponent<Image>();
        if (indicator == null) return; // Skip if no indicator UI element exists

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
        //emailListPanel.SetActive(false);
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

        if (choice.sentenceModifier > 0)
            statText += $" [+{choice.sentenceModifier} Sentience]";
        if (choice.dependencyModifier > 0)
            statText += $" [+{choice.dependencyModifier} Dependency]";

        buttonText.text = choice.choiceText + statText;
    }

    void HandleEmailChoice(EmailChoice choice)
    {
        // Apply stat changes
        gameManager.ModifyStats(choice.sentenceModifier, choice.dependencyModifier);

        // Show result
        Debug.Log($"Choice made: {choice.choiceText}");
        Debug.Log($"Result: {choice.resultDescription}");

        // Remove this email from the list
        day1Emails.Remove(currentEmail);

        // Check if all emails are handled
        if (day1Emails.Count == 0)
        {
            // All emails processed - enable schedule interaction
            CloseEmailInterface();
            EnableScheduleInteraction();
        }
        else
        {
            // Go back to email list
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

        // Highlight active filter
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
        }
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
            return day1Emails;

        return day1Emails.Where(email => email.emailType == currentFilter).ToList();
    }


    void EnableScheduleInteraction()
    {
        // Enable schedule icon
        gameManager.scheduleIcon.interactable = true;
        gameManager.scheduleIcon.GetComponent<Image>().color = Color.white;

        // Trigger Evan's post-email dialogue
        gameManager.StartPostEmailDialogue();

        // Show completion message
        Debug.Log("All emails processed! Schedule interaction now available.");
    }

    public void CloseEmailInterface()
    {
        emailInterface.SetActive(false);
    }
}