using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SweetSpotTrigger : MonoBehaviour
{
    [Header("Input Actions")]
    public InputAction hitAction;
    public InputAction blueHitAction;
    public InputAction keyHitAction;

    [Header("References")]
    public ProgressionBar progressBarScript;

    [Header("Hit Animation")]
    public float hitScaleMultiplier = 2f;
    public float hitAnimDuration = 0.2f;

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
            Debug.Log($"Hit! [{label}]");

            GameObject beatObj = beatSlot.gameObject;
            beatSlot = null;

            // Disable collider immediately so it can't be hit or exited again
            beatObj.GetComponent<Collider2D>().enabled = false;

            StartCoroutine(HitAnimation(beatObj));
        }
        else
        {
            Debug.Log($"Empty press [{label}]");
            progressBarScript.MinusFill();
        }
    }

    private IEnumerator HitAnimation(GameObject beatObj)
    {
        Vector3 originalScale = beatObj.transform.localScale;
        Vector3 targetScale = Vector3.zero;


        SpriteRenderer sr = beatObj.GetComponent<SpriteRenderer>();
        Color originalColor = sr != null ? sr.color : Color.white;

        // Stop movement
        Beat beat = beatObj.GetComponent<Beat>();
        if (beat != null) beat.frozen = true;

        float elapsed = 0f;

        while (elapsed < hitAnimDuration)
        {
            float t = elapsed / hitAnimDuration;

            beatObj.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            if (sr != null)
            {
                Color c = originalColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(beatObj);
    }

    // -----------------------------------------------------------------------
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