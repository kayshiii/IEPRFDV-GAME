using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    //public TextMeshProUGUI instructionTitleText;
    public TextMeshProUGUI instructionContentText;
    public TextMeshProUGUI dayInfoText; // Shows current day and time limit

    [Header("Grid Settings")]
    public int gridWidth = 4;
    public int gridHeight = 5;

    [Header("Block Prefabs")]
    public GameObject[] blockPrefabs;

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

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
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
        // Set title
        //instructionTitleText.text = "Schedule Organization";

        // Set day info with time limit
        dayInfoText.text = $"Day {gameManager.currentDay} - Time Limit: {currentDayData.timeLimit} seconds";

        // Set instruction content
        string instructions = "HOW TO PLAY:\n\n" +
                            "• Drag the schedule blocks from the sides\n" +
                            "• Drop them into the grid to organize your day\n" +
                            "• All blocks must fit within the grid boundaries\n" +
                            "• Blocks cannot overlap with each other\n" +
                            "• Complete the puzzle before time runs out\n\n" +
                            "SCHEDULE ITEMS:\n";

        // Add current day's schedule items to instructions
        string[] scheduleItems = currentDayData.scheduleItems;
        for (int i = 0; i < scheduleItems.Length; i++)
        {
            instructions += $"• {scheduleItems[i]}\n";
        }

        instructions += "\nPress BEGIN when you're ready to start!";

        instructionContentText.text = instructions;
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
        // Clear any existing interactive cells
        foreach (Transform child in gridContainer)
        {
            if (child.name.Contains("InteractiveCell"))
                Destroy(child.gameObject);
        }

        grid = new GridCell[gridWidth, gridHeight];

        float gridWidth_pixels = gridVisualAsset.rect.width;
        float gridHeight_pixels = gridVisualAsset.rect.height;
        float cellWidth = gridWidth_pixels / gridWidth;
        float cellHeight = gridHeight_pixels / gridHeight;

        Vector2 gridPosition = gridVisualAsset.anchoredPosition;
        float startX = gridPosition.x - (gridWidth_pixels / 2f) + (cellWidth / 2f);
        float startY = gridPosition.y + (gridHeight_pixels / 2f) - (cellHeight / 2f);

        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
            {
                GameObject cellObj = Instantiate(interactiveCellPrefab, gridContainer);
                cellObj.name = $"InteractiveCell_{col}_{row}";

                RectTransform cellRect = cellObj.GetComponent<RectTransform>();
                cellRect.sizeDelta = new Vector2(cellWidth, cellHeight);

                float posX = startX + (col * cellWidth);
                float posY = startY - (row * cellHeight);
                cellRect.anchoredPosition = new Vector2(posX, posY);

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

        string[] scheduleItems = currentDayData.scheduleItems;

        for (int i = 0; i < scheduleItems.Length; i++)
        {
            GameObject blockObj = Instantiate(blockPrefabs[i], blockContainer);
            ScheduleBlock block = blockObj.GetComponent<ScheduleBlock>();

            if (block == null)
                block = blockObj.AddComponent<ScheduleBlock>();

            block.Initialize(scheduleItems[i], this);
            scheduleBlocks.Add(block);
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

        if (gridX + shapeWidth > gridWidth || gridY + shapeHeight > gridHeight)
            return false;

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
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
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

        // Check if this is Day 4 and trigger decision point
        if (gameManager.currentDay == 4)
        {
            gameManager.StartDecisionPointDialogue();
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
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
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

                    if (cellX >= 0 && cellX < gridWidth && cellY >= 0 && cellY < gridHeight)
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
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (!grid[x, y].IsOccupied)
                {
                    grid[x, y].SetVisualState(GridCell.CellState.Normal);
                }
            }
        }
    }
}
