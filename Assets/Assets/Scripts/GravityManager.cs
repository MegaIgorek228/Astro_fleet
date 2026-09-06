using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GravityManager : MonoBehaviour
{
    public static GravityObject[] gravityObjects;

    private void Start()
    {
        gravityObjects = FindObjectsOfType<GravityObject>();
    }

    public static Vector3 getGravityAcceleration(Vector3 pos)
    {
        Vector3 acceleration = Vector3.zero;
        for (int i = 0; i < gravityObjects.Length; i++)
        {
            GravityObject gravityObj = gravityObjects[i].GetComponent<GravityObject>();
            if (gravityObj != null)
            {
                acceleration += gravityObj.getAceleration(pos);
            }
        }
        return acceleration;
    }
}