using System.Collections;
using UnityEngine;

public class CellHighlighter : MonoBehaviour
{
    Color glowColor = Color.blue;  // Цвет свечения
    float glowDuration = 1.5f;       // Длительность одной фазы свечения (туда и обратно)
    int glowRepeats = 2;             // Сколько раз клетка будет мигать

    private Material cellMaterial;
    private Color originalEmission;
    private bool isGlowing = false;

    void Start()
    {
        cellMaterial = GetComponent<Renderer>().material;
        originalEmission = cellMaterial.GetColor("_EmissionColor");
    }

    public void HighlightWithGlow()
    {
        if (!isGlowing)
        {
            StartCoroutine(GlowEffect());
        }
    }

    private IEnumerator GlowEffect()
    {
        isGlowing = true;
        cellMaterial.EnableKeyword("_EMISSION");

        for (int i = 0; i < glowRepeats; i++)
        {
            // Плавное включение свечения
            yield return AnimateGlow(originalEmission, glowColor, glowDuration / 2);
            // Плавное выключение свечения
            yield return AnimateGlow(glowColor, originalEmission, glowDuration / 2);
        }

        cellMaterial.DisableKeyword("_EMISSION");
        isGlowing = false;
    }

    private IEnumerator AnimateGlow(Color fromColor, Color toColor, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            cellMaterial.SetColor("_EmissionColor", Color.Lerp(fromColor, toColor, elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cellMaterial.SetColor("_EmissionColor", toColor);
    }
}
