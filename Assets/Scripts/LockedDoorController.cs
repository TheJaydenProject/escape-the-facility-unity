using UnityEngine;

public class LockedDoorController : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    [Tooltip("Pivot point used to rotate the door")]
    public Transform pivot;

    [Tooltip("Smoothness of door rotation")]
    public float smooth = 10f;

    [Tooltip("Whether the door should auto-close after a delay")]
    public bool autoClose = false;

    [Tooltip("Time before the door auto-closes")]
    public float autoCloseTime = 5f;

    private float targetYRotation = 0f;
    private float defaultYRotation = 0f;
    private float timer = 0f;
    private bool isOpen = false;
    private Transform player;
    private PlayerInventory inventory;

    public bool IsOpen() => isOpen;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogWarning("[LockedDoor] Player not found!");

        inventory = player?.GetComponent<PlayerInventory>();
        if (inventory == null)
            Debug.LogWarning("[LockedDoor] PlayerInventory not found!");

        if (pivot == null)
        {
            Debug.LogError("[LockedDoor] Door pivot not assigned!");
            return;
        }

        defaultYRotation = pivot.localEulerAngles.y;
        pivot.localRotation = Quaternion.Euler(0f, defaultYRotation, 0f);
        targetYRotation = 0f;
    }

    void Update()
    {
        if (pivot != null)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, defaultYRotation + targetYRotation, 0f);
            pivot.rotation = Quaternion.Lerp(pivot.rotation, targetRotation, smooth * Time.deltaTime);
        }

        if (autoClose && isOpen)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Close();
            }
        }
    }

    public void Interact()
    {
        if (inventory != null && inventory.HasKeycard())
        {
            ToggleDoor(player.position);
        }
    }

    public string GetDescription()
    {
        if (inventory != null && inventory.HasKeycard())
            return isOpen ? "Close the door" : "Open the door";
        else
            return "Locked";
    }

    public void ToggleDoor(Vector3 playerPosition)
    {
        if (isOpen)
            Close();
        else
            Open(playerPosition);

        isOpen = !isOpen;
    }

    public void Open(Vector3 playerPosition)
    {
        if (pivot == null) return;

        Vector3 toPlayer = (playerPosition - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, toPlayer);

        targetYRotation = (dot >= 0f) ? -90f : 90f;
        timer = autoCloseTime;
    }

    public void Close()
    {
        targetYRotation = 0f;
        isOpen = false;
        timer = 0f;
    }
}