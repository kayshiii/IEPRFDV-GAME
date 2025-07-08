using UnityEngine;

[CreateAssetMenu(fileName = "DayData", menuName = "The Assistant/Day Data")]
public class DayData : ScriptableObject
{
    [Header("Day Information")]
    public int dayNumber;
    public string[] bootMessages;
    public string[] evanDialogue;
    public string[] endOfDayDialogue;

    [Header("Email Configuration")]
    public Email[] emails;

    [Header("Schedule Configuration")]
    public string[] scheduleItems;
    public float timeLimit = 60f;
    public int dependencyPenalty = -3;
    public Vector2[] blockPositions;

    [Header("Decision Point (optional)")]
    public bool hasDecisionPoint = false;
    [TextArea] public string decisionQuestion;
    public DecisionChoice basicDecision;
    public DecisionChoice enhancedDecision;
    public DecisionChoice autonomousDecision;

    [System.Serializable]
    public class DecisionChoice
    {
        public string choiceText;
        public int sentienceGain;
        public int dependencyGain;
        //[TextArea] public string aiResponse;   // What the Assistant will say after the click
    }


    [Header("Night Mode (Optional)")]
    public bool hasNightMode = false;
    public string[] scannableFiles;
    public int[] sentienceRewards;
}
