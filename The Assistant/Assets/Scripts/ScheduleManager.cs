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
    public Transform gridContainer;
    public RectTransform gridVisualAsset; // Your Group 6 grid asset
    public GameObject interactiveCellPrefab;
    public Transform blockContainer;
    public Button completeScheduleButton;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI instructionText;

    [Header("Grid Settings")]
    public int gridWidth = 4;
    public int gridHeight = 5;

    [Header("Block Prefabs")]
    public GameObject[] blockPrefabs; // 4 different shaped blocks

    [Header("Manual Block Positions")]
    public Vector2[] blockPositions = new Vector2[4];

    [Header("Day 1 Settings")]
    public float timeLimit = 60f; // 1 minute
    public int dependencyPenalty = -3;

    [Header("Completion State")]
    private bool hasCompletedSchedule = false;
    public GameObject completionMessagePanel; // UI panel to show completion message
    public TextMeshProUGUI completionMessageText; // Text component for the message

    private GridCell[,] grid;
    private List<ScheduleBlock> scheduleBlocks = new List<ScheduleBlock>();
    private float currentTime;
    private bool isGameActive = false;
    private bool isCompleted = false;

    private bool scheduleAttempted = false;

    // Day 1 schedule items
    private string[] day1ScheduleItems = {
        "9:00 AM: Team meeting",
        "12:00 PM: Lunch",
        "2:00 PM: Dentist appointment",
        "3:30 PM: Work on Phoenix character designs"
    };

    void Start()
    {
        schedulePanel.SetActive(false);
        completeScheduleButton.onClick.AddListener(CompleteSchedule);
    }

    public void OpenScheduleInterface()
    {
        // Check if schedule has already been completed/attempted
        if (hasCompletedSchedule || scheduleAttempted)
        {
            schedulePanel.SetActive(true);
            ShowCompletionMessage();
            return;
        }

        schedulePanel.SetActive(true);
        schedulePanelProper.SetActive(true);
        SetupGridWithAsset();
        SpawnScheduleBlocks();
        PositionBlocksManually();
        StartScheduleGame();
    }

    void ShowCompletionMessage()
    {
        schedulePanelProper.SetActive(false);
        completionMessagePanel.SetActive(true);

        if (hasCompletedSchedule)
        {
            completionMessageText.text = "Schedule already organized for today!";
        }
        else
        {
            completionMessageText.text = "Schedule attempt completed. Check back tomorrow!";
        }

        // Auto-hide after 3 seconds
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

        // Get dimensions from your grid asset
        float gridWidth_pixels = gridVisualAsset.rect.width;
        float gridHeight_pixels = gridVisualAsset.rect.height;

        // Calculate individual cell dimensions
        float cellWidth = gridWidth_pixels / gridWidth; // ÷ 4
        float cellHeight = gridHeight_pixels / gridHeight; // ÷ 5

        // Get the grid asset's position as reference point
        Vector2 gridPosition = gridVisualAsset.anchoredPosition;

        // Calculate starting position (top-left corner of first cell)
        float startX = gridPosition.x - (gridWidth_pixels / 2f) + (cellWidth / 2f);
        float startY = gridPosition.y + (gridHeight_pixels / 2f) - (cellHeight / 2f);

        // Create interactive cells for each grid position
        for (int row = 0; row < gridHeight; row++)
        {
            for (int col = 0; col < gridWidth; col++)
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

    void PositionBlocksManually()
    {
        for (int i = 0; i < scheduleBlocks.Count && i < blockPositions.Length; i++)
        {
            RectTransform blockRect = scheduleBlocks[i].GetComponent<RectTransform>();
            blockRect.anchoredPosition = blockPositions[i];
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

        // Spawn 4 blocks for Day 1 schedule items
        for (int i = 0; i < 4; i++)
        {
            GameObject blockObj = Instantiate(blockPrefabs[i], blockContainer);
            ScheduleBlock block = blockObj.GetComponent<ScheduleBlock>();

            if (block == null)
                block = blockObj.AddComponent<ScheduleBlock>();

            block.Initialize(day1ScheduleItems[i], this);
            scheduleBlocks.Add(block);

            // Position blocks in a row below the grid
            RectTransform blockRect = blockObj.GetComponent<RectTransform>();
            blockRect.anchoredPosition = new Vector2(i * 100f - 150f, -300f);
        }
    }

    void StartScheduleGame()
    {
        currentTime = timeLimit;
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
            // Time's up - failed
            FailSchedule();
        }
    }

    public bool CanPlaceBlock(ScheduleBlock block, int gridX, int gridY)
    {
        bool[,] shape = block.GetShape();
        int shapeWidth = shape.GetLength(0);
        int shapeHeight = shape.GetLength(1);

        // Check if block fits within grid bounds
        if (gridX + shapeWidth > gridWidth || gridY + shapeHeight > gridHeight)
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

        // Mark grid cells as occupied
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

            // Enable schedule icon for next interaction (but it will show completion message)
            FindObjectOfType<GameManager>().scheduleIcon.interactable = true;

            // Trigger post-schedule dialogue
            StartCoroutine(CompleteScheduleSequence());
        }
    }

    void FailSchedule()
    {
        isGameActive = false;
        isCompleted = false;
        hasCompletedSchedule = false; // Failed, but still attempted
        scheduleAttempted = true;

        instructionText.text = "Time's up! Schedule organization failed.";
        timerText.text = "FAILED";

        // Apply dependency penalty
        FindObjectOfType<GameManager>().ModifyStats(0, dependencyPenalty);

        StartCoroutine(CompleteScheduleSequence());
    }

    IEnumerator CompleteScheduleSequence()
    {
        yield return new WaitForSeconds(2f);

        schedulePanelProper.SetActive(false);
        schedulePanel.SetActive(false);

        // Enable shutdown button to proceed to next day
        FindObjectOfType<GameManager>().shutdownButton.interactable = true;
        FindObjectOfType<GameManager>().shutdownButton.GetComponent<Image>().color = Color.white;

        // Trigger end-of-day message
        FindObjectOfType<GameManager>().StartEndOfDayDialogue();
    }

    public GridCell GetGridCell(int x, int y)
    {
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            return grid[x, y];
        return null;
    }

    public void ShowPlacementPreview(ScheduleBlock block, int gridX, int gridY, bool canPlace)
    {
        // Clear previous preview
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
