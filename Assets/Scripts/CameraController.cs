using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("Движение WASD")]
    public float moveSpeed = 10f;
    
    [Header("Управление мышкой")]
    public bool enableMouseDrag = true;      // Перемещение зажатым колёсиком
    public bool enableMouseZoom = true;      // Зум колёсиком
    
    [Header("Настройки мыши")]
    public float zoomSpeed = 5f;             // Скорость зума
    public float minZoom = 0.1f;               // Минимальный зум
    public float maxZoom = 30f;              // Максимальный зум
    
    public float rotateSpeed = 100f;
    
    private static Camera cam;
    private Vector3 dragOrigin;
    private bool isDragging = false;
    
    private float scrollInput;

    private Vector2 mouseDelta;
    private Vector2 moveInput;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }
    
    private Vector2 GetMoveInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return Vector2.zero;
        
        float horizontal = 0f;
        float vertical = 0f;
        
        // WASD или стрелки
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal = -1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal = 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical = 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical = -1f;
        
        // Нормализуем диагональное движение
        Vector2 input = new Vector2(horizontal, vertical);
        if (input.magnitude > 1f)
            input.Normalize();
        
        return input;
    }

    public static float Zoom()
    {
        return cam.orthographicSize;
    }
    
    void Update()
    {
        GetInputFromNewSystem();
        
        // WASD
        Vector2 moveInput = GetMoveInput();
        
        Vector3 move = transform.right * moveInput.x + transform.up * moveInput.y;
        move *= moveSpeed * Time.deltaTime * Mathf.Pow(2*cam.orthographicSize/(minZoom+maxZoom), 1);
        transform.position += move;
        
        // перетаскивание
        if (enableMouseDrag)
        {
            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                dragOrigin = GetMouseWorldPosition();
                isDragging = true;
            }
            
            if (Mouse.current.middleButton.isPressed && isDragging)
            {
                Vector3 currentPos = GetMouseWorldPosition();
                Vector3 difference = dragOrigin - currentPos;
                transform.position += Time.deltaTime * moveSpeed * difference * Mathf.Pow(2*cam.orthographicSize/(minZoom+maxZoom), 1);
            }
            
            if (Mouse.current.middleButton.wasReleasedThisFrame)
            {
                isDragging = false;
            }
        }
        // Зум
        if (enableMouseZoom && cam != null)
        {
            float newSize = cam.orthographicSize - scrollInput*zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        if (keyboard.qKey.isPressed)
        {
            transform.Rotate(0, 0, rotateSpeed*Time.deltaTime);
        } else if (keyboard.eKey.isPressed)
        {
            transform.Rotate(0, 0, -rotateSpeed*Time.deltaTime);
        }
    }
    
    private void GetInputFromNewSystem()
    {
        // WASD
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            float horizontal = 0f;
            float vertical = 0f;
            
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal = -1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal = 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical = 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical = -1f;
            
            moveInput = new Vector2(horizontal, vertical);
        }
        
        // Зум колёсиком
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            scrollInput = mouse.scroll.ReadValue().y / 120f;
            mouseDelta = mouse.delta.ReadValue();
        }
    }
    private Vector3 GetMouseWorldPosition()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return Vector3.zero;
        
        Vector3 mousePoint = mouse.position.ReadValue();
        mousePoint.z = -transform.position.z;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }
    //
    // void HandleClick()
    // {
    //     // Получаем позицию мыши в мировых координатах
    //     Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    //     Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
    //     
    //     // Делаем рейкаст
    //     RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
    //     
    //     if (hit.collider != null)
    //     {
    //         // Проверяем есть ли на объекте нужный компонент
    //         ClickableObject2D clickable = hit.collider.GetComponent<ClickableObject2D>();
    //         if (clickable != null)
    //         {
    //             // Снимаем выделение с предыдущего
    //             if (currentSelected != null && currentSelected != clickable)
    //             {
    //                 currentSelected.SetSelected(false);
    //             }
    //             
    //             // Выделяем новый объект
    //             clickable.SetSelected(true);
    //             currentSelected = clickable;
    //             
    //             Debug.Log($"Выбран объект: {hit.collider.gameObject.name}");
    //         }
    //     }
    //     else
    //     {
    //         // Клик по пустому месту - снимаем выделение
    //         if (currentSelected != null)
    //         {
    //             currentSelected.SetSelected(false);
    //             currentSelected = null;
    //         }
    //     }
    // }
}