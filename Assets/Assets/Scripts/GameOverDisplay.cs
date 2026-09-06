using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverDisplay : MonoBehaviour
{
    public float appearDuration = 0.5f;

    public static void Show(Sprite sprite, string buttonText = "Вернуться в меню")
    {
        if (sprite == null)
        {
            Debug.LogError("Sprite is null! Cannot show GameOver.");
            return;
        }

        // 1. Создаём Canvas
        GameObject canvasGO = new GameObject("GameOverCanvas");
        canvasGO.layer = 5; // UI

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // максимальный порядок

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        // 2. Создаём Image (заполняет весь экран)
        GameObject imageGO = new GameObject("Background");
        imageGO.transform.SetParent(canvasGO.transform, false);
        Image image = imageGO.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false; // чтобы клики проходили сквозь
        image.preserveAspect = false; // растягиваем на весь экран

        RectTransform imgRect = imageGO.GetComponent<RectTransform>();
        imgRect.anchorMin = Vector2.zero;
        imgRect.anchorMax = Vector2.one;
        imgRect.offsetMin = Vector2.zero;
        imgRect.offsetMax = Vector2.zero;

        // 3. Создаём кнопку
        GameObject buttonGO = new GameObject("MenuButton");
        buttonGO.transform.SetParent(canvasGO.transform, false);

        // Настраиваем RectTransform кнопки (размер и позиция)
        RectTransform btnRect = buttonGO.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.1f);
        btnRect.anchorMax = new Vector2(0.5f, 0.1f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(300, 80);

        // Добавляем Image для фона кнопки
        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.8f, 0.9f); // тёмно-синий с прозрачностью

        // Добавляем Button
        Button btn = buttonGO.AddComponent<Button>();
        btn.targetGraphic = btnImage;

        // Добавляем текст на кнопку
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        Text text = textGO.AddComponent<Text>();
        text.text = buttonText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 30;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        // 4. Подписываем кнопку на действие
        btn.onClick.AddListener(() => {
            TimeController.SetPaused(false);
            // Если у вас есть сцена меню – загрузите её, иначе перезагрузите текущую
            SceneManager.LoadScene("MenuScene"); // загружаем сцену с индексом 0

        });

        // 5. Добавляем анимацию появления на весь Canvas
        GameOverDisplay display = canvasGO.AddComponent<GameOverDisplay>();
        display.StartAnimation(image);
    }

    private Image targetImage;
    private void StartAnimation(Image img)
    {
        targetImage = img;
        StartCoroutine(AnimateAppear());
    }

    private IEnumerator AnimateAppear()
    {
        targetImage.rectTransform.localScale = Vector3.zero;
        Color c = targetImage.color;
        c.a = 0f;
        targetImage.color = c;

        float elapsed = 0f;
        while (elapsed < appearDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / appearDuration);

            float scale = Mathf.SmoothStep(0f, 1.2f, t);
            if (t > 0.8f) scale = Mathf.Lerp(1.2f, 1f, (t - 0.8f) / 0.2f);
            targetImage.rectTransform.localScale = Vector3.one * scale;

            c.a = Mathf.SmoothStep(0f, 1f, t);
            targetImage.color = c;

            yield return null;
        }

        targetImage.rectTransform.localScale = Vector3.one;
        c.a = 1f;
        targetImage.color = c;

        // Останавливаем игру (после того как анимация завершена)
        TimeController.SetPaused(true);
    }
}