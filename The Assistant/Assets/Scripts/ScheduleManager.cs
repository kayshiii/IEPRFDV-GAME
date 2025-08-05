using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//using UnityEditor.VersionControl;

public class ScheduleManager : MonoBehaviour
{
    [Header("Schedule UI References")]
    public GameObject schedulePanel;
    public GameObject schedulePanelProper;
    public GameObject instructionPanel; // New instruction panel
    public Transform gridContainer;
    public RectTransform gridVisualAsset;
    public GameObject interactiveCellPrefab;
    public Transform blockContainer;
    public Button completeScheduleButton;
    public Button beginGameButton; // New begin button
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI instructionText;

    [Header("Instruction Screen")]
    public TextMeshProUGUI failedInfoText;
    public TextMeshProUGUI timeLimitInfoText; // Shows current day and time limit

    [Header("Day-Specific Grid Configurations")]
    public GameObject[] gridVisualAssets; // Different grid visuals for each day (9 elements)
    public int[] gridWidths = new int[8];     // Width for each day
    public int[] gridHeights = new int[8];    // Height for each day

    // Remove the old: public int gridWidth = 4; public int gridHeight = 5;
    public int currentGridWidth;
    public int currentGridHeight;
    private RectTransform currentGridVisualAsset;


    [Header("Day-Specific Block Prefabs")]
    public GameObject[] day1BlockPrefabs; // 4 blocks for Day 1
    public GameObject[] day2BlockPrefabs; // 5 blocks for Day 2
    public GameObject[] day3BlockPrefabs; // 4 blocks for Day 3
    public GameObject[] day4BlockPrefabs; // 4 blocks for Day 4
    public GameObject[] day5BlockPrefabs; // 6 blocks for Day 5
    public GameObject[] day6BlockPrefabs; // 4 blocks for Day 6
    public GameObject[] day7BlockPrefabs; // 7 blocks for Day 7
    public GameObject[] day8BlockPrefabs; // Variable based on path

    // Remove the old: public GameObject[] blockPrefabs;
    private GameObject[] currentBlockPrefabs;


    [Header("Completion State")]
    private bool hasCompletedSchedule = false;
    public GameObject completionMessagePanel;
    public TextMeshProUGUI completionMessageText;

    private GridCell[,] grid;
    private List<ScheduleBlock> scheduleBlocks = new List<ScheduleBlock>();
    private float currentTime;
    private bool isGameActive = false;
    private bool isCompleted = false;
    private bool scheduleAttempted = false;
    private GameManager gameManager;
    private DayData currentDayData;

    void SelectGridConfigurationForCurrentDay()
    {
        int currentDay = gameManager.currentDay;
        int dayIndex = currentDay - 1; // Convert to 0-based index

        // Validate day index
        if (dayIndex >= 0 && dayIndex < gridWidths.Length &&
            dayIndex < gridHeights.Length && dayIndex < gridVisualAssets.Length)
        {
            currentGridWidth = gridWidths[dayIndex];
            currentGridHeight = gridHeights[dayIndex];
            currentGridVisualAsset = gridVisualAssets[dayIndex].GetComponent<RectTransform>();

            Debug.Log($"Selected grid for Day {currentDay}: {currentGridWidth}x{currentGridHeight}");
        }
        else
        {
            // Fallback to default values
            currentGridWidth = 4;
            currentGridHeight = 5;
            //currentGridVisualAsset = gridVisualAssets[0]; // Use first grid as fallback

            Debug.LogWarning($"No grid configuration found for Day {currentDay}, using default 4x5");
        }
        
        if (gameManager.currentDay == 1)
        {
            gridVisualAssets[1].SetActive(false);
            gridVisualAssets[0].SetActive(true);
        }
        else if (gameManager.currentDay == 2)
        {
            gridVisualAssets[1].SetActive(true);
            gridVisualAssets[0].SetActive(false);
        }
        else if (gameManager.currentDay == 3)
        {
            gridVisualAssets[2].SetActive(true);
            gridVisualAssets[1].SetActive(false);
        }
        /*else if (gameManager.currentDay == 4)
        {
            gridVisualAssets[3].SetActive(true);
            gridVisualAssets[2].SetActive(false);
        }*/
        else
        {
            gridVisualAssets[2].SetActive(false);
            gridVisualAssets[0].SetActive(true);
        }
    }

    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        schedulePanel.SetActive(false);
        completeScheduleButton.onClick.AddListener(CompleteSchedule);
        beginGameButton.onClick.AddListener(BeginScheduleGame); // New button listener
    }

    public void ResetForNewDay()
    {
        hasCompletedSchedule = false;
        scheduleAttempted = false;
        isGameActive = false;
        isCompleted = false;
    }

    public void OpenScheduleInterface()
    {
        currentDayData = gameManager.CurrentDayData;

        if (hasCompletedSchedule || scheduleAttempted)
        {
            schedulePanel.SetActive(true);
            ShowCompletionMessage();
            return;
        }

        schedulePanel.SetActive(true);
        ShowInstructionScreen(); // Show instructions first
    }

    void ShowInstructionScreen()
    {
        // Hide other panels and show instruction panel
        schedulePanelProper.SetActive(false);
        completionMessagePanel.SetActive(false);
        instructionPanel.SetActive(true);

        // Set up instruction content
        SetupInstructionContent();
    }

    void SetupInstructionContent()
    {
        // Set day info with time limit
        timeLimitInfoText.text = $"TIMELIMIT: <color=#61FFA2>{currentDayData.timeLimit} SECONDS</color>";

        failedInfoText.text = $"FAILED: <color=#F70000>{currentDayData.dependencyPenalty} DEPENDENCY</color>";
    }

    void SelectBlockPrefabsForCurrentDay()
    {
        int currentDay = gameManager.currentDay;

        switch (currentDay)
        {
            case 1:
                currentBlockPrefabs = day1BlockPrefabs;
                break;
            case 2:
                currentBlockPrefabs = day2BlockPrefabs;
                break;
            case 3:
                currentBlockPrefabs = day3BlockPrefabs;
                break;
            case 4:
                currentBlockPrefabs = day4BlockPrefabs;
                break;
            case 5:
                currentBlockPrefabs = day5BlockPrefabs;
                break;
            case 6:
                currentBlockPrefabs = day6BlockPrefabs;
                break;
            case 7:
                currentBlockPrefabs = day7BlockPrefabs;
                break;
            case 8:
                currentBlockPrefabs = day8BlockPrefabs;
                break;
            default:
                currentBlockPrefabs = day1BlockPrefabs; // Fallback
                Debug.LogWarning($"No block prefabs defined for day {currentDay}, using Day 1 prefabs");
                break;
        }

        Debug.Log($"Selected {currentBlockPrefabs.Length} block prefabs for Day {currentDay}");
    }


    public void BeginScheduleGame()
    {
        // Hide instruction panel and show game panel
        instructionPanel.SetActive(false);
        schedulePanelProper.SetActive(true);

        // Prepare the game components
        SetupGridWithAsset();
        SpawnScheduleBlocks();
        PositionBlocksManually();

        // Start the actual game
        StartScheduleGame();
    }

    void ShowCompletionMessage()
    {
        schedulePanelProper.SetActive(false);
        instructionPanel.SetActive(false);
        completionMessagePanel.SetActive(true);

        if (hasCompletedSchedule)
        {
            completionMessageText.text = "Schedule already organized for today!";
        }
        else
        {
            completionMessageText.text = "Schedule attempt completed. Check back tomorrow!";
        }

        StartCoroutine(HideCompletionMessage());
    }

    IEnumerator HideCompletionMessage()
    {
        yield return new WaitForSeconds(3f);
        completionMessagePanel.SetActive(false);
        schedulePanel.SetActive(false);
    }

    void SetupGridWithAsset()
    {
        // Select grid configuration for current day
        SelectGridConfigurationForCurrentDay();

        // Clear any existing interactive cells
        foreach (Transform child in gridContainer)
        {
            if (child.name.Contains("InteractiveCell"))
                Destroy(child.gameObject);
        }

        // Initialize grid array with current day's dimensions
        grid = new GridCell[currentGridWidth, currentGridHeight];

        // Get dimensions from current day's grid asset
        float gridWidth_pixels = currentGridVisualAsset.rect.width;
        float gridHeight_pixels = currentGridVisualAsset.rect.height;

        // Calculate individual cell dimensions
        float cellWidth = gridWidth_pixels / currentGridWidth;
        float cellHeight = gridHeight_pixels / currentGridHeight;

        // Get the grid asset's position as reference point
        Vector2 gridPosition = currentGridVisualAsset.anchoredPosition;

        // Calculate starting position (top-left corner of first cell)
        float startX = gridPosition.x - (gridWidth_pixels / 2f) + (cellWidth / 2f);
        float startY = gridPosition.y + (gridHeight_pixels / 2f) - (cellHeight / 2f);

        // Create interactive cells for each grid position
        for (int row = 0; row < currentGridHeight; row++)
        {
            for (int col = 0; col < currentGridWidth; col++)
            {
                // Create interactive cell
                GameObject cellObj = Instantiate(interactiveCellPrefab, gridContainer);
                cellObj.name = $"InteractiveCell_{col}_{row}";

                // Position the cell
                RectTransform cellRect = cellObj.GetComponent<RectTransform>();
                cellRect.sizeDelta = new Vector2(cellWidth, cellHeight);

                float posX = startX + (col * cellWidth);
                float posY = startY - (row * cellHeight);
                cellRect.anchoredPosition = new Vector2(posX, posY);

                // Initialize the GridCell component
                GridCell gridCell = cellObj.GetComponent<GridCell>();
                gridCell.Initialize(col, row);
                grid[col, row] = gridCell;
            }
        }
    }


    void SpawnScheduleBlocks()
    {
        // Clear existing blocks
        foreach (Transform child in blockContainer)
        {
            Destroy(child.gameObject);
        }
        scheduleBlocks.Clear();

        // Select appropriate prefabs for current day
        SelectBlockPrefabsForCurrentDay();

        // Get current day's schedule items
        string[] scheduleItems = currentDayData.scheduleItems;

        // Ensure we have enough prefabs for the schedule items
        int itemCount = Mathf.Min(scheduleItems.Length, currentBlockPrefabs.Length);

        // Spawn blocks using day-specific prefabs
        for (int i = 0; i < itemCount; i++)
        {
            GameObject blockObj = Instantiate(currentBlockPrefabs[i], blockContainer);
            ScheduleBlock block = blockObj.GetComponent<ScheduleBlock>();

            if (block == null)
                block = blockObj.AddComponent<ScheduleBlock>();

            block.Initialize(scheduleItems[i], this);
            scheduleBlocks.Add(block);
        }

        // Handle case where we have more schedule items than prefabs
        if (scheduleItems.Length > currentBlockPrefabs.Length)
        {
            Debug.LogWarning($"Day {gameManager.currentDay}: More schedule items ({scheduleItems.Length}) than available prefabs ({currentBlockPrefabs.Length})");

            // Use the last prefab for remaining items
            for (int i = currentBlockPrefabs.Length; i < scheduleItems.Length; i++)
            {
                GameObject blockObj = Instantiate(currentBlockPrefabs[currentBlockPrefabs.Length - 1], blockContainer);
                ScheduleBlock block = blockObj.GetComponent<ScheduleBlock>();

                if (block == null)
                    block = blockObj.AddComponent<ScheduleBlock>();

                block.Initialize(scheduleItems[i], this);
                scheduleBlocks.Add(block);
            }
        }
    }


    void PositionBlocksManually()
    {
        Vector2[] positions = currentDayData.blockPositions;

        for (int i = 0; i < scheduleBlocks.Count && i < positions.Length; i++)
        {
            RectTransform blockRect = scheduleBlocks[i].GetComponent<RectTransform>();
            blockRect.anchoredPosition = positions[i];
        }
    }

    void StartScheduleGame()
    {
        currentTime = currentDayData.timeLimit;

        isGameActive = true;
        isCompleted = false;

        instructionText.text = "Drag schedule blocks to fit them in the grid!";
        completeScheduleButton.interactable = false;

        StartCoroutine(TimerCountdown());
    }

    IEnumerator TimerCountdown()
    {
        while (currentTime > 0 && isGameActive && !isCompleted)
        {
            timerText.text = $"Time: {Mathf.Ceil(currentTime)}s";
            currentTime -= Time.deltaTime;
            yield return null;
        }

        if (!isCompleted && isGameActive)
        {
            FailSchedule();
        }
    }

    public bool CanPlaceBlock(ScheduleBlock block, int gridX, int gridY)
    {
        bool[,] shape = block.GetShape();
        int shapeWidth = shape.GetLength(0);
        int shapeHeight = shape.GetLength(1);

        // Check if block fits within current grid bounds
        if (gridX + shapeWidth > currentGridWidth || gridY + shapeHeight > currentGridHeight)
            return false;

        // Check if all required cells are empty
        for (int x = 0; x < shapeWidth; x++)
        {
            for (int y = 0; y < shapeHeight; y++)
            {
                if (shape[x, y] && grid[gridX + x, gridY + y].IsOccupied)
                    return false;
            }
        }

        return true;
    }

    public void PlaceBlock(ScheduleBlock block, int gridX, int gridY)
    {
        bool[,] shape = block.GetShape();
        int shapeWidth = shape.GetLength(0);
        int shapeHeight = shape.GetLength(1);

        for (int x = 0; x < shapeWidth; x++)
        {
            for (int y = 0; y < shapeHeight; y++)
            {
                if (shape[x, y])
                {
                    grid[gridX + x, gridY + y].SetOccupied(true, block);
                }
            }
        }

        block.SetPlaced(true);
        CheckCompletion();
    }

    public void RemoveBlock(ScheduleBlock block)
    {
        // Find and clear all cells occupied by this block
        for (int x = 0; x < currentGridWidth; x++)
        {
            for (int y = 0; y < currentGridHeight; y++)
            {
                if (grid[x, y].OccupiedBy == block)
                {
                    grid[x, y].SetOccupied(false, null);
                }
            }
        }

        block.SetPlaced(false);
    }

    void CheckCompletion()
    {
        bool allPlaced = true;
        foreach (ScheduleBlock block in scheduleBlocks)
        {
            if (!block.IsPlaced)
            {
                allPlaced = false;
                break;
            }
        }

        if (allPlaced)
        {
            completeScheduleButton.interactable = true;
            instructionText.text = "All blocks placed! Click Complete to finish.";
        }
        else
        {
            completeScheduleButton.interactable = false;
        }
    }

    void CompleteSchedule()
    {
        if (!isCompleted)
        {
            isCompleted = true;
            isGameActive = false;
            hasCompletedSchedule = true;
            scheduleAttempted = true;

            instructionText.text = "Schedule completed successfully!";
            gameManager.scheduleIcon.interactable = true;
            StartCoroutine(CompleteScheduleSequence(true));
        }
    }

    void FailSchedule()
    {
        isGameActive = false;
        isCompleted = false;
        hasCompletedSchedule = false;
        scheduleAttempted = true;

        instructionText.text = "Time's up! Schedule organization failed.";
        timerText.text = "FAILED";

        gameManager.ModifyStats(0, currentDayData.dependencyPenalty);
        StartCoroutine(CompleteScheduleSequence(false));
    }

    IEnumerator CompleteScheduleSequence(bool success)
    {
        yield return new WaitForSeconds(2f);

        schedulePanelProper.SetActive(false);
        schedulePanel.SetActive(false);

        gameManager.shutdownButton.interactable = true;
        gameManager.shutdownButton.GetComponent<Image>().color = Color.white;

        // Days wirh decision points
        if (gameManager.currentDay == 4 || gameManager.currentDay == 5 || gameManager.currentDay == 6)
        {
            gameManager.TriggerDecisionPoint();
        }
        else
        {
            // For other days, go straight to end of day
            gameManager.StartEndOfDayDialogue(success);
            Debug.Log("schedmanager");
        }
    }

    public GridCell GetGridCell(int x, int y)
    {
        if (x >= 0 && x < currentGridWidth && y >= 0 && y < currentGridHeight)
            return grid[x, y];
        return null;
    }

    public void ShowPlacementPreview(ScheduleBlock block, int gridX, int gridY, bool canPlace)
    {
        ClearPlacementPreview();

        bool[,] shape = block.GetShape();
        int shapeWidth = shape.GetLength(0);
        int shapeHeight = shape.GetLength(1);

        for (int x = 0; x < shapeWidth; x++)
        {
            for (int y = 0; y < shapeHeight; y++)
            {
                if (shape[x, y])
                {
                    int cellX = gridX + x;
                    int cellY = gridY + y;

                    if (cellX >= 0 && cellX < currentGridWidth && cellY >= 0 && cellY < currentGridHeight)
                    {
                        GridCell cell = grid[cellX, cellY];
                        cell.SetVisualState(canPlace ? GridCell.CellState.Highlighted : GridCell.CellState.Invalid);
                    }
                }
            }
        }
    }

    public void ClearPlacementPreview()
    {
        for (int x = 0; x < currentGridWidth; x++)
        {
            for (int y = 0; y < currentGridHeight; y++)
            {
                if (!grid[x, y].IsOccupied)
                {
                    grid[x, y].SetVisualState(GridCell.CellState.Normal);
                }
            }
        }
    }
}
