using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectionInfoUI : MonoBehaviour
{
    [Header("Внешний вид кнопок")]
    public Sprite buttonBackground; // ваша текстура для фона кнопки
    public Sprite buttonIcon;       // иконка слева от текста (опционально)

    private SelectionManager selectionManager;
    private CanvasGroup canvasGroup;
    private List<GameObject> currentButtons = new List<GameObject>();
    private List<MovableObject> lastSelected = new List<MovableObject>();
    private Font defaultFont;

    // Контейнер для кнопок (создаётся автоматически)
    private Transform buttonContainer;

    void Start()
    {
        selectionManager = FindObjectOfType<SelectionManager>();
        if (selectionManager == null)
        {
            Debug.LogError("SelectionManager не найден!");
            enabled = false;
            return;
        }

        // Создаём шрифт
        defaultFont = Font.CreateDynamicFontFromOSFont("Arial", 14);
        if (defaultFont == null)
            defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Создаём контейнер для кнопок (если его нет)
        Transform existing = transform.Find("ButtonContainer");
        if (existing != null)
        {
            buttonContainer = existing;
        }
        else
        {
            GameObject containerGO = new GameObject("ButtonContainer", typeof(RectTransform));
            containerGO.transform.SetParent(transform, false);
            buttonContainer = containerGO.transform;
            // Настраиваем RectTransform контейнера: растягиваем по ширине и высоте, с отступом сверху для заголовка
            RectTransform containerRect = buttonContainer.GetComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 0);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.offsetMin = new Vector2(0, 0);
            containerRect.offsetMax = new Vector2(0, 0);
        }

        // Добавляем VerticalLayoutGroup на контейнер (если нет)
        VerticalLayoutGroup layoutGroup = buttonContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = buttonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
        }
        // Настройки группы: отступ сверху = высоте кнопки (28), чтобы не наезжать на заголовок
        layoutGroup.padding = new RectOffset(5, 5, 28, 5);
        layoutGroup.spacing = 2;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandHeight = false;

        // Настраиваем CanvasGroup для управления видимостью всей панели
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    void Update()
    {
        bool selectionChanged = false;
        if (selectionManager.selectedObjects.Count != lastSelected.Count)
            selectionChanged = true;
        else
        {
            for (int i = 0; i < lastSelected.Count; i++)
                if (lastSelected[i] != selectionManager.selectedObjects[i])
                {
                    selectionChanged = true;
                    break;
                }
        }

        if (selectionChanged)
        {
            RebuildButtons();
            lastSelected.Clear();
            lastSelected.AddRange(selectionManager.selectedObjects);
        }

        bool hasSelection = selectionManager.selectedObjects.Count > 0;
        canvasGroup.alpha = hasSelection ? 1f : 0f;
        canvasGroup.interactable = hasSelection;
        canvasGroup.blocksRaycasts = hasSelection;
    }

    private void RebuildButtons()
    {
        // Удаляем старые кнопки
        foreach (var btn in currentButtons)
            Destroy(btn);
        currentButtons.Clear();

        if (selectionManager.selectedObjects.Count == 0)
            return;

        foreach (var obj in selectionManager.selectedObjects)
        {
            // Создаём кнопку внутри buttonContainer
            GameObject btnGO = new GameObject("Button_" + obj.name, typeof(RectTransform));
            btnGO.transform.SetParent(buttonContainer, false);

            RectTransform rect = btnGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
            rect.sizeDelta = new Vector2(0, 20);
            rect.pivot = new Vector2(0.5f, 0.5f);

            // --- Фон кнопки ---
            Image image = btnGO.AddComponent<Image>();
            image.sprite = (buttonBackground != null) ? buttonBackground : CreateWhiteSprite();
            image.type = Image.Type.Sliced;
            image.color = Color.white;

            Button btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = image;

            // --- Иконка (если есть) ---
            if (buttonIcon != null)
            {
                GameObject iconGO = new GameObject("Icon");
                iconGO.transform.SetParent(btnGO.transform, false);
                Image iconImage = iconGO.AddComponent<Image>();
                iconImage.sprite = buttonIcon;
                iconImage.preserveAspect = true;
                RectTransform iconRect = iconGO.GetComponent<RectTransform>();
                iconRect.anchorMin = new Vector2(0, 0.5f);
                iconRect.anchorMax = new Vector2(0, 0.5f);
                iconRect.sizeDelta = new Vector2(20, 20);
                iconRect.anchoredPosition = new Vector2(10, 0);
            }

            // --- Текст ---
            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            Text text = textGO.AddComponent<Text>();
            text.font = defaultFont;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
            text.text = obj.name;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            float leftOffset = (buttonIcon != null) ? 40 : 10;
            textRect.offsetMin = new Vector2(leftOffset, 0);
            textRect.offsetMax = new Vector2(-10, 0);

            LayoutElement layout = btnGO.AddComponent<LayoutElement>();
            layout.minHeight = 28;
            layout.flexibleWidth = 1;

            MovableObject capturedObj = obj;
            btn.onClick.AddListener(() => SelectSingle(capturedObj));

            currentButtons.Add(btnGO);
        }
    }

    private Sprite whiteSpriteCache;
    private Sprite CreateWhiteSprite()
    {
        if (whiteSpriteCache != null) return whiteSpriteCache;
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        whiteSpriteCache = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        return whiteSpriteCache;
    }

    private void SelectSingle(MovableObject target)
    {
        selectionManager.ClearSelection();
        selectionManager.SelectObject(target);
    }
}