/*using UnityEngine;
using UnityEngine.UI;

public class ScheduleGridSlot : MonoBehaviour
{
    [Header("Slot Properties")]
    public Vector2Int gridPosition;
    public Vector2Int slotSize = Vector2Int.one;

    private ScheduleBlock currentBlock;
    private Image slotImage;
    private Color originalColor;
    private Color highlightColor = Color.yellow;

    void Start()
    {
        slotImage = GetComponent<Image>();
        originalColor = slotImage.color;
    }

    public bool CanAcceptBlock(ScheduleBlock block)
    {
        if (currentBlock != null) return false;

        //Vector2Int blockSize = block.GetBlockSize();
        //return blockSize.x <= slotSize.x && blockSize.y <= slotSize.y;
    }

    public void PlaceBlock(ScheduleBlock block)
    {
        currentBlock = block;
        slotImage.color = highlightColor;
    }

    public void RemoveBlock()
    {
        currentBlock = null;
        slotImage.color = originalColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ScheduleBlock block = other.GetComponent<ScheduleBlock>();
        if (block != null && CanAcceptBlock(block))
        {
            slotImage.color = highlightColor;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (currentBlock == null)
        {
            slotImage.color = originalColor;
        }
    }
}
*/