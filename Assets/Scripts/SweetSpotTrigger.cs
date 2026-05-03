using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles hit detection for all three beat types:
///   Red  — original hitAction  (e.g. bound to D or mouse click)
///   Blue — blueHitAction       (bound to Z)
///   Key  — keyHitAction        (bound to Space)
///
/// Wire up the three InputActions in the Inspector (or via a PlayerInput component).
/// Make sure the tags "Beat", "BeatBlue", "BeatKey" exist in Unity's Tag Manager.
/// </summary>
public class SweetSpotTrigger : MonoBehaviour
{
    [Header("Input Actions")]
    public InputAction hitAction;       // Red  — e.g. D / left-click
    public InputAction blueHitAction;   // Blue — Z
    public InputAction keyHitAction;    // Key  — Space

    [Header("References")]
    public ProgressionBar progressBarScript;

    // One tracked collider per beat type so two types can overlap the zone
    private Collider2D redBeatInZone = null;
    private Collider2D blueBeatInZone = null;
    private Collider2D keyBeatInZone = null;

    // -----------------------------------------------------------------------
    void OnEnable()
    {
        hitAction.Enable();
        blueHitAction.Enable();
        keyHitAction.Enable();

        hitAction.performed += OnHitRed;
        blueHitAction.performed += OnHitBlue;
        keyHitAction.performed += OnHitKey;
    }

    void OnDisable()
    {
        hitAction.performed -= OnHitRed;
        blueHitAction.performed -= OnHitBlue;
        keyHitAction.performed -= OnHitKey;

        hitAction.Disable();
        blueHitAction.Disable();
        keyHitAction.Disable();
    }

    
    void OnHitRed(InputAction.CallbackContext ctx) => ProcessHit(ref redBeatInZone, "Red");
    void OnHitBlue(InputAction.CallbackContext ctx) => ProcessHit(ref blueBeatInZone, "Blue");
    void OnHitKey(InputAction.CallbackContext ctx) => ProcessHit(ref keyBeatInZone, "Key");

    void ProcessHit(ref Collider2D beatSlot, string label)
    {
        if (beatSlot != null)
        {
            progressBarScript.AddFill();
            Destroy(beatSlot.gameObject);
            beatSlot = null;
            Debug.Log($"Hit! [{label}]");
        }
        else
        {
            Debug.Log($"Empty press [{label}]");
            progressBarScript.MinusFill();
        }
    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Beat"))
            redBeatInZone = collision;
        else if (collision.CompareTag("BeatBlue"))
            blueBeatInZone = collision;
        else if (collision.CompareTag("BeatKey"))
            keyBeatInZone = collision;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Beat") && collision == redBeatInZone)
        {
            redBeatInZone = null;
            Debug.Log("Miss [Red]");
            progressBarScript.MinusFill();
        }
        else if (collision.CompareTag("BeatBlue") && collision == blueBeatInZone)
        {
            blueBeatInZone = null;
            Debug.Log("Miss [Blue]");
            progressBarScript.MinusFill();
        }
        else if (collision.CompareTag("BeatKey") && collision == keyBeatInZone)
        {
            keyBeatInZone = null;
            Debug.Log("Miss [Key]");
            progressBarScript.MinusFill();
        }
    }
}