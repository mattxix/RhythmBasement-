using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.1f;

    private Coroutine shakeCoroutine;

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Shake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        Camera[] allCams = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Vector3[] originalPositions = new Vector3[allCams.Length];

        for (int i = 0; i < allCams.Length; i++)
            originalPositions[i] = allCams[i].transform.position;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            Vector3 offset = (Vector3)Random.insideUnitCircle * shakeMagnitude;

            for (int i = 0; i < allCams.Length; i++)
                if (allCams[i] != null)
                    allCams[i].transform.position = originalPositions[i] + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < allCams.Length; i++)
            if (allCams[i] != null)
                allCams[i].transform.position = originalPositions[i];

        shakeCoroutine = null;
    }
}