using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SelectionManager : MonoBehaviour
{
    [Header("Настройки")]
    [Tooltip("Слой, на котором находятся ваши MovableObject")]
    public LayerMask objectLayer;

    [Tooltip("Максимальное расстояние в пикселях, чтобы считать клик одиночным, а не выделением рамкой")]
    public float clickThreshold = 5f;

    [Header("Ссылки")]
    public Camera mainCamera;
    public RectTransform selectionBox;
    public RectTransform parentRect;

    [Header("Режим выбора цели")]
    public bool isTargetingMode = false;
    public Texture2D targetingCursor;
    private Texture2D defaultCursor;

    public List<MovableObject> selectedObjects = new List<MovableObject>();

    private Vector2 startMousePos;
    private bool isSelecting;

    void Start()
    {
        if (selectionBox != null)
        {
            selectionBox.anchorMin = Vector2.zero;
            selectionBox.anchorMax = Vector2.zero;
            selectionBox.pivot = new Vector2(0.5f, 0.5f);
            if (parentRect == null)
            {
                Debug.LogError("selectionBox parent is null");
            }
        }
        else
        {
            Debug.LogError("selectionBox is null");
        }
        selectionBox.gameObject.SetActive(false);
    }

    private void Awake()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        Mouse mouse = Mouse.current;
        Keyboard keyboard = Keyboard.current;

        if (mouse == null || keyboard == null) return;

        // ПЕРЕКЛЮЧЕНИЕ режима выбора цели по нажатию Alt (не удержание!)
        if (keyboard.leftAltKey.wasPressedThisFrame && selectedObjects.Count > 0)
        {
            if (isTargetingMode)
            {
                ExitTargetingMode();
            }
            else
            {
                EnterTargetingMode();
            }
        }

        // Выход из режима выбора цели по Esc
        if (keyboard.escapeKey.wasPressedThisFrame && isTargetingMode)
        {
            ExitTargetingMode();
        }

        // Нажатие ЛКМ
        if (mouse.leftButton.wasPressedThisFrame)
        {
            startMousePos = mouse.position.ReadValue();
            isSelecting = true;

            // Если НЕ в режиме выбора цели - очищаем выделение
            if (!isTargetingMode)
            {
                if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    ClearSelection();
                }
                
            }
        }

        // Зажатая ЛКМ - показываем рамку выделения
        if (mouse.leftButton.isPressed && isSelecting)
        {
            UpdateSelectionBoxVisual(mouse.position.ReadValue());
        }

        // Отпускание ЛКМ
        if (mouse.leftButton.wasReleasedThisFrame && isSelecting)
        {
            Vector2 endMousePos = mouse.position.ReadValue();
            float distance = Vector2.Distance(startMousePos, endMousePos);

            if (selectionBox != null) selectionBox.gameObject.SetActive(false);

            if (distance < clickThreshold)
            {
                HandleSingleClick(endMousePos);
            }
            else
            {
                HandleBoxSelection(startMousePos, endMousePos);
            }

            isSelecting = false;
        }

        // ПКМ (работает только вне режима выбора цели)
        if (mouse.rightButton.wasPressedThisFrame && !isTargetingMode)
        {
            HandleRightClick(mouse.position.ReadValue());
        }
        
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            FireSelected();
        }
    }

    private void EnterTargetingMode()
    {
        isTargetingMode = true;
        Debug.Log("=== РЕЖИМ ВЫБОРА ЦЕЛИ АКТИВИРОВАН ===");
        Debug.Log($"Выделено кораблей: {selectedObjects.Count}");
        Debug.Log("Кликните по врагу или выделите рамкой для назначения цели");
        Debug.Log("Нажмите Alt или Esc для выхода");

        if (targetingCursor != null)
        {
            Cursor.SetCursor(targetingCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    private void ExitTargetingMode()
    {
        isTargetingMode = false;
        Debug.Log("=== РЕЖИМ ВЫБОРА ЦЕЛИ ДЕАКТИВИРОВАН ===");
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void HandleSingleClick(Vector2 screenPos)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity, objectLayer);

        
        if (hit.collider != null)
        {
            MovableObject obj = hit.collider.GetComponent<MovableObject>();
            if (obj != null)
            {
                if (isTargetingMode)
                {
                    AssignTargetToSelected(obj);
                }
                else
                {
                    SelectObject(obj);
                }
            }
        }
        else
        {
            if (isTargetingMode)
            {
                Debug.Log("Клик по пустому месту - выход из режима цели");
                ExitTargetingMode();
            }
        }
    }

    private void HandleBoxSelection(Vector2 startScreen, Vector2 endScreen)
    {
        float zDistance = Mathf.Abs(mainCamera.transform.position.z);

        Vector3 startWorld = mainCamera.ScreenToWorldPoint(new Vector3(startScreen.x, startScreen.y, zDistance));
        Vector3 endWorld = mainCamera.ScreenToWorldPoint(new Vector3(endScreen.x, endScreen.y, zDistance));

        Vector2 min = Vector2.Min(startWorld, endWorld);
        Vector2 max = Vector2.Max(startWorld, endWorld);

        Collider2D[] hits = Physics2D.OverlapAreaAll(min, max, objectLayer);

        List<MovableObject> objectsInBox = new List<MovableObject>();

        foreach (Collider2D hit in hits)
        {
            GhostTrace ghost = hit.gameObject.transform.parent.gameObject.transform.parent.gameObject
                .GetComponent<GhostTrace>();
            MovableObject mov = hit.gameObject.transform.parent.gameObject.transform.parent.gameObject
                .GetComponent<MovableObject>();
            
            if (ghost != null)
            {
                MovableObject obj = ghost.getOrigin().GetComponent<MovableObject>();
                if (obj != null)
                {
                    objectsInBox.Add(obj);
                }
            } else if (mov != null && mov.isFlagship)
            {
                MovableObject obj = mov;
                if (obj != null)
                {
                    objectsInBox.Add(obj);
                }
            }
        }

        if (isTargetingMode)
        {
            // В режиме выбора цели - назначаем все объекты в рамке как ЦЕЛИ
            Debug.Log($"Найдено целей в рамке: {objectsInBox.Count}");
            foreach (MovableObject target in objectsInBox)
            {
                AssignTargetToSelected(target);
            }
        }
        else
        {
            // В обычном режиме - выделяем объекты
            foreach (MovableObject obj in objectsInBox)
            {
                if (obj.isEnemy == false)
                    SelectObject(obj);
            }
        }
    }

    private void HandleRightClick(Vector2 screenPos)
    {
        if (selectedObjects.Count == 0) return;
        Keyboard keyboard = Keyboard.current;

        float zDistance = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 targetWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistance));
        targetWorldPos.z = 0f;
        if (keyboard.shiftKey.isPressed)
        {
            foreach (MovableObject obj in selectedObjects)
            {
                obj.addWayPoint(targetWorldPos);
            }
        }
        else
        {
            foreach (MovableObject obj in selectedObjects)
            {
                obj.setTargetWaypoint(targetWorldPos);
            }
        }
    }

    private void AssignTargetToSelected(MovableObject target)
    {
        foreach (MovableObject obj in selectedObjects)
        {
            if (obj != target)
            {
                obj.SetTarget(target);
                Debug.Log($"{obj.gameObject.name} → цель: {target.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"{obj.gameObject.name} не может быть целью самого себя!");
            }
        }
    }

    public void SelectObject(MovableObject obj)
    {   
        
        if (!selectedObjects.Contains(obj))
        {
            obj.isSelected = true;
            selectedObjects.Add(obj);
            Debug.Log($"Выделен: {obj.gameObject.name}");
        }
    }

    public void ClearSelection()
    {
        foreach (MovableObject obj in selectedObjects)
        {
            obj.isSelected = false;
        }
        selectedObjects.Clear();
    }

    private void UpdateSelectionBoxVisual(Vector2 currentMousePos)
    {
        if (selectionBox == null)
            return;

        if (!selectionBox.gameObject.activeSelf)
            selectionBox.gameObject.SetActive(true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, startMousePos, null, out Vector2 localStart);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, currentMousePos, null, out Vector2 localCurrent);

        Vector2 center = (localStart + localCurrent + new Vector2(Screen.width, Screen.height)) / 2f;
        Vector2 size = new Vector2(Mathf.Abs(localStart.x - localCurrent.x), Mathf.Abs(localStart.y - localCurrent.y));

        selectionBox.anchoredPosition = center;
        selectionBox.sizeDelta = size;
    }
    
    public void FireSelected()
    {
        if (selectedObjects.Count == 0)
        {
            Debug.Log("Нет выделенных кораблей для стрельбы.");
            return;
        }

        foreach (var obj in selectedObjects)
        {
            // Пытаемся получить компонент торпедного аппарата
            MissleLauncher launcher = obj.GetComponent<MissleLauncher>();
            if (launcher == null)
                continue;

            // Пытаемся получить текущую цель корабля (из ShipController)
            ShipController controller = obj.GetComponent<ShipController>();
            GameObject target = controller != null ? controller.targetGO : null;

            // Вызываем стрельбу с целью (или без)
            launcher.Fire(target);
        }
    }
}