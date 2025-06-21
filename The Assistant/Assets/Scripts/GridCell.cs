using UnityEngine;
using UnityEngine.UI;

public class GridCell : MonoBehaviour
{
    [Header("Visual Feedback Colors")]
    public Color normalColor = new Color(0, 0, 0, 0); // Transparent
    public Color highlightColor = new Color(1f, 1f, 0f, 0.4f); // Semi-transparent yellow
    public Color occupiedColor = new Color(0f, 1f, 0f, 0.4f); // Semi-transparent green
    public Color invalidColor = new Color(1f, 0f, 0f, 0.4f); // Semi-transparent red

    private Image cellImage;
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public bool IsOccupied { get; private set; }
    public ScheduleBlock OccupiedBy { get; private set; }

    void Awake()
    {
        cellImage = GetComponent<Image>();
        if (cellImage == null)
            cellImage = gameObject.AddComponent<Image>();

        // Ensure the cell can receive raycast events for drag detection
        cellImage.raycastTarget = true;
    }

    public void Initialize(int x, int y)
    {
        GridX = x;
        GridY = y;
        SetVisualState(CellState.Normal);
    }

    public enum CellState { Normal, Highlighted, Occupied, Invalid }

    public void SetVisualState(CellState state)
    {
        switch (state)
        {
            case CellState.Normal:
                cellImage.color = normalColor;
                break;
            case CellState.Highlighted:
                cellImage.color = highlightColor;
                break;
            case CellState.Occupied:
                cellImage.color = occupiedColor;
                break;
            case CellState.Invalid:
                cellImage.color = invalidColor;
                break;
        }
    }

    public void SetOccupied(bool occupied, ScheduleBlock block)
    {
        IsOccupied = occupied;
        OccupiedBy = block;
        SetVisualState(occupied ? CellState.Occupied : CellState.Normal);
    }

    public void SetHighlight(bool highlighted)
    {
        if (!IsOccupied)
        {
            SetVisualState(highlighted ? CellState.Highlighted : CellState.Normal);
        }
    }
}
