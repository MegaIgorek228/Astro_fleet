using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour
{
    private float duration = 1f;
    private float scaleFactor = 0.1f; // 1 = нормальный размер, 0.25 = в 4 раза меньше

    private SpriteRenderer sr;
    private float startTime;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("Explosion requires SpriteRenderer!");
            Destroy(gameObject);
            return;
        }
        startTime = Time.time;
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed = Time.time - startTime;
            float t = elapsed / duration;
            scaleFactor = 0.1f;
            // Масштаб: от 0.5 до 2.0, умноженный на scaleFactor
            float scale = Mathf.Lerp(0.5f * scaleFactor, 2.0f * scaleFactor, t);
            transform.localScale = Vector3.one * scale;

            // Прозрачность: от 1 до 0
            Color c = sr.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            sr.color = c;

            yield return null;
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Создаёт взрыв в указанной позиции с указанным спрайтом.
    /// </summary>
    public static void Spawn(Vector3 position, Sprite explosionSprite, float duration = 1f, float scaleFactor = 1f)
    {
        GameObject go = new GameObject("Explosion");
        go.transform.position = position;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = explosionSprite;
        sr.sortingOrder = 100; // Поверх всех объектов

        Explosion exp = go.AddComponent<Explosion>();
        exp.duration = duration;
        exp.scaleFactor = scaleFactor;
    }
}