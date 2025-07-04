using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DayData", menuName = "Game/DayData")]
public class DayData : ScriptableObject
{
    public int dayNumber;
    public List<Email> emails;
    public List<string> scheduleItems;
    public string[] bootDialogue;
    public string[] postEmailDialogue;
    public string[] endOfDayDialogue;
}
