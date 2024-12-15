using UnityEngine;
using UnityEngine.UI;

public class GlowEffect : MonoBehaviour
{
    public float glowSpeed = 2f;
    public float minAlpha = 0.2f;
    public float maxAlpha = 0.7f;

    private Image glowImage;

    void Start()
    {
        glowImage = GetComponent<Image>();
    }

    void Update()
    {
        // Pulse the alpha value of the glow
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f);
        Color color = glowImage.color;
        color.a = alpha;
        glowImage.color = color;
    }
}
