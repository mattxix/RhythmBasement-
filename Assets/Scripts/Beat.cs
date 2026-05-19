using UnityEngine;

public class Beat : MonoBehaviour
{
    public float speed = 5f;
    public BeatType beatType = BeatType.Red;
    private RectTransform rt;

    [HideInInspector] public bool frozen = false;

    void Start()
    {
        rt = GetComponent<RectTransform>();

        gameObject.tag = beatType switch
        {
            BeatType.Blue => "BeatBlue",
            BeatType.Key => "BeatKey",
            _ => "Beat",
        };
    }

    void Update()
    {
        if (frozen) return;
        rt.anchoredPosition += Vector2.left * speed * Time.deltaTime;
    }
}