using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RendOrImg: MonoBehaviour
{
    #region Константы и переменные
    public Sprite sprite;

    public float iconSize;
    
    public GameObject icon;
    
    private Canvas canvas;
    private GameObject canvasGO;

    private GameObject mother;
    
    private const float baseScale = 0.015f;
    
    private float rendererDistance;
    
    private Renderer objectRenderer;
    private Renderer iconRenderer;

    private int maxResolution = 512;

    private float focusZoom = 1f;

    public Image img;

    public bool activeImage = true;
    
    #endregion
    void Start()
    {
        
        iconSize = Mathf.Clamp(iconSize, 1f, 300f);
        // Создание канваса
        canvasGO = new GameObject("Canvas" + gameObject.name);
        canvasGO.SetActive(false);
        canvasGO.transform.SetParent(transform);
        canvasGO.transform.localPosition = Vector3.zero;
        canvas = canvasGO.AddComponent<Canvas>();
        // canvasGO.AddComponent<ClickDetector>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(maxResolution, maxResolution);

        canvasGO.AddComponent<GraphicRaycaster>();
        
    
        // Создание иконки
        icon = new GameObject("icon" + gameObject.name);
        icon.transform.SetParent(canvasGO.transform);
        
        img = icon.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.transform.localPosition = Vector3.zero;
        icon.layer = 6;
        icon.AddComponent<BoxCollider2D>();
        
        RectTransform rectTransform = icon.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(iconSize, iconSize); // размер в пикселях
        
        // mother = gameObject.transform.parent.gameObject.transform.parent.gameObject;

        objectRenderer = gameObject.GetComponentInChildren<Renderer>();
        if (objectRenderer != null)
        {
            objectRenderer.enabled = false;
        }

        // if (!gameObject.GetComponent<MovableObject>().isFlagship && gameObject.GetComponent<GhostTrace>() == null)
        // {
        //     activeImage = false;
        // }
    }
    
    void Update()
    {
        if (activeImage)
        {
            if (canvasGO.activeSelf == false)
            {
                canvasGO.SetActive(true);
            }
            canvasGO.transform.position = gameObject.transform.position;
            canvasGO.transform.localRotation = Camera.main.transform.rotation;
            canvasGO.transform.localScale = Vector3.one*baseScale/Mathf.Pow(focusZoom/CameraController.Zoom(), 1.0f);
        }
        else
        {
            canvasGO.SetActive(false);
        }
   }

    void OnDestroy()
    {
        Debug.LogWarning("Cabooom");
        Destroy(canvasGO);
        Destroy(icon);
    }
    
    public void SetIconRotation(float angleDegrees)
    {
        if (icon != null)
        {
            // Поворачиваем GameObject иконки вокруг оси Z
            icon.transform.localRotation = Quaternion.Euler(0, 0, angleDegrees);
            // Debug.Log("SetIconRotation called with angle: " + angleDegrees);
        }
    }
    
    // RendOrImg.cs
    public float GetCurrentRotation()
    {
        if (icon != null)
            return icon.transform.localRotation.eulerAngles.z;
        return 0f;
    }
    
    
}