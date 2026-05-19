using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProgressionBar : MonoBehaviour
{
    

    [Header("UI Reference")]
    public Image fillBar;

    [Header("Cameras")]
    public Camera cam1;
    public Camera cam2;
    public Camera cam3;
    public Camera cam4;
    public Camera cam5;

    [Header("Flash Settings")]
    public float flashDuration = 0.25f;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float drainRate = 0.01f;

    [Header("Script")]
    public SweetSpotTrigger hitScript;
    public BeatManager beatManagerScript;

    private float currentFill = .25f;
    private int currentCamIndex = 0;
    private Camera[] cameras;

    private Color defaultColor;
    private Coroutine flashCoroutine;

    void Start()
    {
        cameras = new Camera[] { cam1, cam2, cam3, cam4, cam5 };
        currentFill = .25f;
        currentCamIndex = 0;
        defaultColor = fillBar.color;
        UpdateBar();
        SetActiveCamera(0);
    }

    void Update()
    {
        if (currentFill > 0f && beatManagerScript.isInDowntime == false)
        {
            currentFill -= drainRate * Time.deltaTime;
            currentFill = Mathf.Clamp01(currentFill);
            UpdateBar();
        }

        if (currentFill <= 0f)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (currentFill >= .99f)
        {
            beatManagerScript.NextSong();
            currentFill = .25f;
            UpdateBar();
            currentCamIndex = (currentCamIndex + 1) % cameras.Length;
            SetActiveCamera(currentCamIndex);
        }
    }

    public void AddFill()
    {
        currentFill += 0.12f;
        UpdateBar();
        FlashBar(new Color(0, .3f, 0));
    }

    public void MinusFill()
    {
        currentFill -= 0.07f;
        UpdateBar();
        FlashBar(new Color(.3f, 0, 0));
        CameraShake.Instance.Shake();
    }

    private void FlashBar(Color flashColor)
    {
        if (flashCoroutine != null)
            CameraShake.Instance.StopCoroutine(flashCoroutine);
        flashCoroutine = CameraShake.Instance.StartCoroutine(FlashRoutine(flashColor));
    }

    private IEnumerator FlashRoutine(Color flashColor)
    {
        fillBar.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        fillBar.color = defaultColor;
        flashCoroutine = null;
    }

    private void UpdateBar()
    {
        fillBar.fillAmount = currentFill;
    }

    private void SetActiveCamera(int index)
    {
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null)
                cameras[i].enabled = (i == index);
        }
    }
}