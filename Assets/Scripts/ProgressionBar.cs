using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ProgressionBar : MonoBehaviour
{
    [Header("UI Reference")]
    public Image fillBar;

    [Header("Cameras")]
    public Camera cam1;
    public Camera cam2;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float drainRate = 0.01f;

    [Header("Script")]
    public SweetSpotTrigger hitScript;
    public BeatManager beatManagerScript;

    private float currentFill = .25f; 

    void Start()
    {
        currentFill = .25f;
        UpdateBar();
        cam1.enabled = true;
        cam2.enabled = false;
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
        if(currentFill >= .99f)
        {
            beatManagerScript.NextSong();
            currentFill = .25f;
            UpdateBar();
            cam1.enabled = false;
            cam2.enabled = true;
        }
    }

    
    public void AddFill()
    {
        
        currentFill += 0.12f;
        //currentFill = Mathf.Clamp01(currentFill); 
        UpdateBar();
    }
    public void MinusFill()
    {
       
        currentFill -= 0.07f;
        //currentFill = Mathf.Clamp01(currentFill); 
        UpdateBar();
    }

    private void UpdateBar()
    {
        fillBar.fillAmount = currentFill;
    }
}
