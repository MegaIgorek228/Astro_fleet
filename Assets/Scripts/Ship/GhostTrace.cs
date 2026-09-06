using UnityEngine;
using System.Collections.Generic;
using AstroFleet;

public class GhostTrace : MonoBehaviour
{
    private List<Vector4> waypoints;
    private List<float> waypointAngles;
    private bool isActive = false;
    private GameObject origin;

    public void setOrigin(GameObject setOrigin)
    {
        origin = setOrigin;
    }

    public GameObject getOrigin()
    {
        return origin;
    }
    private RendOrImg rend;
    private Sprite explosionSprite;
    
    public void TakeOver(List<Vector4> wp, List<float> angles, RendOrImg origRend, Sprite expSprite)
    {
        waypoints = new List<Vector4>(wp);
        waypointAngles = new List<float>(angles);
        explosionSprite =  expSprite;
        isActive = true;
        
        gameObject.SetActive(true);
        
        rend = GetComponent<RendOrImg>();
        if (rend != null && origRend != null)
        {
            rend.sprite = origRend.sprite;
            rend.iconSize = origRend.iconSize;
        }
    }

    void Update()
    {
        if (!isActive) return;

        // Если все точки из "буфера обработаны, то исчезаем
        if (waypoints == null || waypoints.Count == 0)
        {
            Explosion.Spawn(transform.position, explosionSprite, 1f);
            Destroy(gameObject);
            return;
        }
        
        if (waypoints[0].z <= TimeController.globalTime)
        {
            while (waypoints.Count > 1 && waypoints[1].z <= TimeController.globalTime)
            {
                waypoints.RemoveAt(0); 
                if (waypointAngles.Count > 0) waypointAngles.RemoveAt(0);
            }
            
            transform.position = new Vector3(waypoints[0].x, waypoints[0].y, 0);
            
            if (waypoints.Count > 0 && waypoints[0].z <= TimeController.globalTime)
            {
                transform.position = new Vector3(waypoints[0].x, waypoints[0].y, 0);

                if (rend != null && waypointAngles.Count > 0)
                {
                    rend.SetIconRotation(waypointAngles[0]);
                }

                waypoints.RemoveAt(0);
                if (waypointAngles.Count > 0) waypointAngles.RemoveAt(0); // <-- синхронное удаление
            }
            
            // waypoints.RemoveAt(0);
            // waypointAngles.RemoveAt(0);
        }
    }
}