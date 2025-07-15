using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ScheduleBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Block Settings")]
    public string scheduleText;
    public string BlockName;
    public bool IsPlaced { get; private set; }

    [Header("Visual Components")]
    public TextMeshProUGUI blockText;
    public Image blockImage;
    public CanvasGroup canvasGroup;

    private ScheduleManager scheduleManager;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private bool[,] blockShape;
    private Canvas canvas;

    public Vector2 startPos;

    GameManager gameManager;

    void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (blockImage == null)
            blockImage = GetComponent<Image>();

        if (blockText == null)
            blockText = GetComponentInChildren<TextMeshProUGUI>();            
    }

    public void Initialize(string text, ScheduleManager manager)
    {
        scheduleText = text;
        scheduleManager = manager;
        originalPosition = rectTransform.anchoredPosition;

        // Set up block shape based on schedule item
        SetupBlockShape();

        // Update text display
        if (blockText != null)
            blockText.text = scheduleText;
    }

    void SetupBlockShape()
    {
        // --- DAY 1 ---
        if (BlockName.Contains("1a"))
        {
            blockShape = new bool[,] { { true, true, true, true }, { true, false, false, false } };
        }
        else if (BlockName.Contains("1b"))
        {
            blockShape = new bool[,] { { true, true }, { true, true } };
        }
        else if (BlockName.Contains("1c"))
        {
            blockShape = new bool[,] { { true, false, false, false }, { true, true, true, true } };
        }
        else if (BlockName.Contains("1d"))
        {
            blockShape = new bool[,] { { false, true }, { true, true }, { true, true }, { false, true } };
        }
        // --- DAY 2 ---
        else if (BlockName.Contains("2a"))
        {
            blockShape = new bool[,] { { true, false, false, false }, { true, true, true, true }, };
        }
        else if (BlockName.Contains("2b"))
        {
            blockShape = new bool[,] { { false, true, false }, { false, true, false }, { true, true, true } }; // good
        }
        else if (BlockName.Contains("2c"))
        {
            blockShape = new bool[,] { { true, true, true, true } };
        }
        else if (BlockName.Contains("2d"))
        {
            blockShape = new bool[,] { { true, true, true, true }, { false, false, false, true }, { true, true, true, true } }; // u block
        } 
        else if (BlockName.Contains("2e"))
        {
            blockShape = new bool[,] { { true, true, true }, { true, false, false }, { true, false, false } }; //grocery
        }
        // --- DAY 3 ---
        else if (BlockName.Contains("3a"))
        {
            blockShape = new bool[,] { { true, false, false, false }, { true, true, true, true }, { true, false, false, false }, }; 
        }
        else if (BlockName.Contains("3b"))
        {
            blockShape = new bool[,] { { true, true, true, true } };
        }
        else if (BlockName.Contains("3d"))
        {
            blockShape = new bool[,] { { false, false, false, true, true }, { true, true, true, true, true } };
        }
    }

    public bool[,] GetShape()
    {
        return blockShape;
    }

    public void SetPlaced(bool placed)
    {
        IsPlaced = placed;

        // Visual feedback for placed state
        /*if (canvasGroup != null)
        {
            canvasGroup.alpha = placed ? 0.8f : 1f;
        }*/
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsPlaced)
            scheduleManager.RemoveBlock(this);

        // Make block semi-transparent while dragging
        /*if (canvasGroup != null)
            canvasGroup.alpha = 0.8f;*/

        // Clear any placement preview
        scheduleManager.ClearPlacementPreview();
    }

    public void OnDrag(PointerEventData eventData)
    {
        //rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // Move block so its center follows the cursor
        Vector2 cursorWorldPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out cursorWorldPos);

        rectTransform.anchoredPosition = cursorWorldPos;

        // Calculate grid position based on your grid asset
        RectTransform gridAsset = scheduleManager.gridVisualAsset;
        float cellWidth = gridAsset.rect.width / scheduleManager.currentGridWidth;
        float cellHeight = gridAsset.rect.height / scheduleManager.currentGridHeight;

        Vector2 gridPos = gridAsset.anchoredPosition;
        float startX = gridPos.x - (gridAsset.rect.width / 2f);
        float startY = gridPos.y + (gridAsset.rect.height / 2f);

        int gridX = Mathf.FloorToInt((cursorWorldPos.x - startX) / cellWidth);
        int gridY = Mathf.FloorToInt((startY - cursorWorldPos.y) / cellHeight);

        bool canPlace = scheduleManager.CanPlaceBlock(this, gridX, gridY);
        scheduleManager.ShowPlacementPreview(this, gridX, gridY, canPlace);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Clear placement preview
        scheduleManager.ClearPlacementPreview();

        // Restore alpha
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // Calculate grid position (same logic as OnDrag)
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            scheduleManager.gridContainer.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out localPoint);

        RectTransform gridAsset = scheduleManager.gridVisualAsset;
        float cellWidth = gridAsset.rect.width / scheduleManager.currentGridWidth;
        float cellHeight = gridAsset.rect.height / scheduleManager.currentGridHeight;

        Vector2 gridPos = gridAsset.anchoredPosition;
        float startX = gridPos.x - (gridAsset.rect.width / 2f);
        float startY = gridPos.y + (gridAsset.rect.height / 2f);

        int gridX = Mathf.FloorToInt((localPoint.x - startX) / cellWidth);
        int gridY = Mathf.FloorToInt((startY - localPoint.y) / cellHeight);

        if (scheduleManager.CanPlaceBlock(this, gridX, gridY))
        {
            scheduleManager.PlaceBlock(this, gridX, gridY);

            // Snap to the EXACT position where visual feedback was shown
            SnapToVisualFeedbackPosition(gridX, gridY);
        }
        else
        {
            // Return to original position
            rectTransform.anchoredPosition = startPos;
        }
    }

    void SnapToVisualFeedbackPosition(int gridX, int gridY)
    {
        // Get the position of the top-left cell where visual feedback appeared
        GridCell topLeftCell = scheduleManager.GetGridCell(gridX, gridY);
        if (topLeftCell != null)
        {
            Vector2 cellPosition = topLeftCell.GetComponent<RectTransform>().anchoredPosition;

            // For multi-cell blocks, calculate the center position across all highlighted cells
            bool[,] shape = GetShape();
            int shapeWidth = shape.GetLength(0);
            int shapeHeight = shape.GetLength(1);

            // Calculate the visual center of the highlighted area
            float cellWidth = scheduleManager.gridVisualAsset.rect.width / scheduleManager.currentGridWidth;
            float cellHeight = scheduleManager.gridVisualAsset.rect.height / scheduleManager.currentGridHeight;

            // Offset to center the block across its occupied cells (matching visual feedback)
            float offsetX = (shapeWidth - 1) * cellWidth / 2f;
            float offsetY = -(shapeHeight - 1) * cellHeight / 2f;

            /*Vector2 finalPosition = new Vector2(
                cellPosition.x + offsetX,
                cellPosition.y + offsetY);//- 70f
            rectTransform.anchoredPosition = finalPosition;*/
            if (gameManager.currentDay == 1)
            {
                Vector2 finalPosition = new Vector2(
                cellPosition.x + offsetX,
                cellPosition.y + offsetY //- 70f

            );
                rectTransform.anchoredPosition = finalPosition;
            }
            else if (gameManager.currentDay == 2)
            {
                Vector2 finalPosition = new Vector2(
                cellPosition.x + offsetX,
                cellPosition.y + offsetY - 35f

            );
                rectTransform.anchoredPosition = finalPosition;
            }
            else if (gameManager.currentDay == 3)
            {
                Vector2 finalPosition = new Vector2(
                cellPosition.x + offsetX,
                cellPosition.y + offsetY - 30f

            );
                rectTransform.anchoredPosition = finalPosition;
            }
            else
            {
                Vector2 finalPosition = new Vector2(
                cellPosition.x + offsetX,
                cellPosition.y + offsetY //- 70f

            );
                rectTransform.anchoredPosition = finalPosition;
            }


        }
    }

}
