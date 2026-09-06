using System;
using UnityEngine;
using AstroFleet;
using System.Collections.Generic;
public class WayTraicing : MonoBehaviour
{
    private static GameObject TC;
    private static GameObject FSH;
    private List<Vector4> waypoints;
    public GameObject visibleObject;
    private List<float> waypointAngles;
    public Sprite explosionSprite;

    void wayTraising()
    {
        if (waypoints[0].z <= TimeController.globalTime)
        {
            if (waypoints.Count > 1 && waypoints[1].z <= TimeController.globalTime)
            {
                visibleObject.SetActive(true);
                waypoints.RemoveAt(0);
                if (waypointAngles.Count > 0) waypointAngles.RemoveAt(0);
                wayTraising();
                return;
            }
            if (waypointAngles.Count > 0)
            {
                RendOrImg rend = visibleObject.GetComponent<RendOrImg>();
                if (rend != null)
                {
                    rend.SetIconRotation(waypointAngles[0]);
                }
            }
            
            visibleObject.transform.position = new Vector3(waypoints[0].x, waypoints[0].y, 0);
            resetImage();
            return;
        }
    }

    public Vector3 getLightPositionFrom(Vector3 position)
    {
        int c = waypoints.Count;
        for (int i = c-1; i > 0; i--)
        {
            float d = (new Vector3(waypoints[i].x, waypoints[i].y, 0) - position).magnitude * Defines.DistScale;
            if (d / Defines.LightSpeed <= TimeController.globalTime - waypoints[i].w)
            {
                return new Vector3(waypoints[i].x, waypoints[i].y, 0);
            }
        }
        return new Vector3(waypoints[0].x, waypoints[0].y, 0);
    }
        
        
        // for (int i = Mathf.Max(activeIndex - 5, 0); i < Mathf.Min(activeIndex + 150, waypoints.Count); i++) 
        // {
        //     if (waypoints[i].z >= TimeController.globalTime)
        //     {
        //         
        //         visibleObject.transform.position = new Vector3(waypoints[i].x, waypoints[i].y,0);
        //         activeIndex = i;
        //         resetImage();
        //         return;
        //     }
        // } // Ищем около прошлого индекса, если не нашли, ищем во всём массиве
        // for (int i = 0; i < waypoints.Count; i++) 
        // {
        //     if (waypoints[i].z >= TimeController.globalTime)
        //     {
        //         
        //         visibleObject.transform.position = new Vector3(waypoints[i].x, waypoints[i].y,0);
        //         activeIndex = i;
        //         resetImage();
        //         return;
        //     }
        // }
   


    void resetImage()
    {
        RendOrImg rend = visibleObject.GetComponent<RendOrImg>();
        RendOrImg rend2 = gameObject.GetComponent<RendOrImg>();
        rend.sprite = rend2.sprite;
        rend.iconSize = rend2.iconSize;
    }

    public Transform getLOTransform()
    {
        return visibleObject.transform;
    }
    
    void Start()
    {
        TC = GameObject.Find("TraicingContainer");
        FSH = GameObject.Find("Flagship");
        waypoints = new List<Vector4>();
        visibleObject = new GameObject("LO" + gameObject.name);
        visibleObject.transform.SetParent(TC.transform);
        visibleObject.SetActive(false);
        

        RendOrImg rend = visibleObject.AddComponent<RendOrImg>();
        RendOrImg rend2 = gameObject.GetComponent<RendOrImg>();
        if (rend2 == null)
        {
            Debug.LogError("Traicing object must have a Rend Or Img");
        }
        rend.sprite = rend2.sprite;
        rend.iconSize = rend2.iconSize;
        visibleObject.transform.position = transform.position;
        
        visibleObject.AddComponent<GhostTrace>();
        GhostTrace ghost = visibleObject.GetComponent<GhostTrace>();
        ghost.setOrigin(gameObject);
        waypointAngles = new List<float>();
    }

    void Update()
    {
        if (FSH == null || FSH.Equals(null))
        {
            return;
        }
        if (TimeController.globalTicks % Defines.SIT == 0 && TimeController.dt() != 0f)
        {
            float ta = Vector3.Distance(FSH.transform.position, transform.position)*Defines.DistScale/Defines.LightSpeed + TimeController.globalTime;  // activation time
            float angle = GetComponent<RendOrImg>().GetCurrentRotation();
            // Debug.Log($"Saved angle: {angle} at time {TimeController.globalTime}");
            waypoints.Add(new Vector4(transform.position.x, transform.position.y, ta, TimeController.globalTime));
            waypointAngles.Add(angle);
            wayTraising();
        }
    }

    private void OnDestroy()
    {
        // Когда корабль умирает, передаем накопленные точки призраку
        if (visibleObject != null)
        {
            GhostTrace ghost = visibleObject.GetComponent<GhostTrace>();
            if (ghost != null)
            {
                ghost.TakeOver(waypoints, waypointAngles, GetComponent<RendOrImg>(), explosionSprite);
            }
        }
    }
}
