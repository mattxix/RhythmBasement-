
using UnityEngine;

public class Beat : MonoBehaviour
{
    public float speed = 5f;
    private RectTransform rt;

    void Start()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        rt.anchoredPosition += Vector2.left * speed * Time.deltaTime;
    }

}
