using UnityEngine;

// Attach this to the escape trigger collider GameObject
public class EscapeZoneRelay : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEscapeZone(true);
                GameManager.Instance.ShowEscapePrompt(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEscapeZone(false);
                GameManager.Instance.ShowEscapePrompt(false);
            }
        }
    }
}

