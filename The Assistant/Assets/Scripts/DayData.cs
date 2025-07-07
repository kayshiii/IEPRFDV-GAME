using UnityEngine;

[CreateAssetMenu(fileName = "DayData", menuName = "The Assistant/Day Data")]
public class DayData : ScriptableObject
{
    [Header("Day Information")]
    public int dayNumber;
    public string[] bootMessages;
    public string[] evanDialogue;
    public string[] postEmailDialogue;
    public string[] endOfDayDialogue;

    [Header("Email Configuration")]
    public Email[] emails;

    [Header("Schedule Configuration")]
    public string[] scheduleItems;
    public float timeLimit = 60f;
    public int dependencyPenalty = -3;
    public Vector2[] blockPositions;

    [Header("Night Mode (Optional)")]
    public bool hasNightMode = false;
    public string[] scannableFiles;
    public int[] sentienceRewards;
}
