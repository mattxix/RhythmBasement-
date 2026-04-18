using UnityEngine;
using UnityEngine.InputSystem;


public class SweetSpotTrigger : MonoBehaviour
{
    public InputAction hitAction;

    private Collider2D beatInZone = null;

    void OnEnable()
    {
        hitAction.Enable();
        hitAction.performed += OnHit;
    }

    void OnDisable()
    {
        hitAction.performed -= OnHit;
        hitAction.Disable();
    }

    void OnHit(InputAction.CallbackContext ctx)
    {
        if (beatInZone != null)
        {
            Destroy(beatInZone.gameObject);
            beatInZone = null;
            Debug.Log("Hit!");
        }
        else
        {
            Debug.Log("Empty press");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Beat"))
            beatInZone = collision;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Beat"))
        {
            beatInZone = null;
            Debug.Log("Miss");
        }
    }
}
