using System;
using System.Linq;
using AstroFleet;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using Vector2 = System.Numerics.Vector2;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class MovableObject : MonoBehaviour
{
    public int ID;
    public string tag; // Ship, Missle
    private bool isBraking = false;
    public Vector3 acceleration;
    public Vector3 targetPoint;
    public bool hasTargetPoint = false;
    public bool isSelected = false;
    public bool fullSpeed = false;
    public Vector3 speed = Vector3.zero;
    public Vector3 dv = Vector3.zero;
    public Vector3 da = Vector3.zero;
    private WayTraicing WT;
    private Transform linkTransform;
    public float maxAcceleration = 0f;
    public Vector3 direction = Vector3.zero;
    public LineRenderer lineRenderer;
    public System.Action OnSelected;
    public System.Action OnDeselected;
    public Color wayColor = Color.white;
    private List<Vector3> waypoints = new List<Vector3>();
    public bool isEnemy = false;
    public bool isFlagship = false;
    public GameObject Ghost;

    public RendOrImg ROI;
    private float baseLineSize = 0.0025f;
    private float targetZoomSize = 1f;

    private Camera mainCamera;

    [Header("Цель для атаки")]
    public MovableObject targetMO;
    public bool showTargetLine = true;
    public Color targetLineColor = Color.red;

    public virtual void Start()
    {
        ROI = GetComponent<RendOrImg>();
        mainCamera = Camera.main;

        WT = GetComponent<WayTraicing>();

        linkTransform = this.transform;
        if (WT != null)
        {
            
            linkTransform = WT.getLOTransform();
            // Debug.LogWarning($"ASDASDASD: {gameObject.name} linkTransform: {linkTransform.gameObject.name}");
            Ghost = WT.visibleObject;
        }

        Enemy en = gameObject.GetComponent<Enemy>();
        if (en != null)
        {
            isEnemy = true;
        }
    }

    public Vector3 getThrust(Vector3 ag, Vector3 d)
    {
        Vector3 ac = d * maxAcceleration;
        float angle = 0;
        int sgn = 1;
        float sw = 9f;
        if (speed.magnitude > Defines.EPSS)
        {
            angle = Vector3.SignedAngle(speed, d, Vector3.forward);
            if (Mathf.Abs(angle) > 90f)
            {
                ac = maxAcceleration * (-speed.normalized * sw + d.normalized).normalized - ag;
            }
            else
            {
                ac = maxAcceleration * ((Quaternion.AngleAxis(2 * angle, Vector3.forward) * speed.normalized * sw) + d.normalized).normalized - ag;
            }
        }
        return ac;
    }

    public Vector3 getLightPositionFrom(Vector3 position)
    {
        WayTraicing obj = gameObject.GetComponent<WayTraicing>();
        if (obj != null)
        {
            return obj.getLightPositionFrom(position);
        }
        Debug.LogError("Movable Object must have a WayTraicing component");
        return transform.position;
    }

    public Vector3 getStopThrust(Vector3 ag, Vector3 d)
    {
        float brakingDistance = (speed.magnitude * speed.magnitude) / (2 * maxAcceleration) * Defines.DistScale * 1.0f;

        if (d.magnitude <= brakingDistance)
        {
            isBraking = true;
        }

        if (!isBraking)
        {
            return getThrust(ag, d);
        }
        else
        {
            float maxBrakeAccel = maxAcceleration;
            Vector3 brakeForce = -speed.normalized * maxBrakeAccel;
            Vector3 toTarget = d.normalized;
            Vector3 lateralCorrection = toTarget - Vector3.Project(toTarget, -speed.normalized);
            if (lateralCorrection.magnitude > 0.001f)
                lateralCorrection = lateralCorrection.normalized * maxBrakeAccel * 0.3f;

            Vector3 totalThrust = brakeForce + lateralCorrection - ag;

            if (totalThrust.magnitude > maxBrakeAccel)
                totalThrust = totalThrust.normalized * maxBrakeAccel;

            return totalThrust;
        }
    }

    public void updateAcceleration()
    {
        Vector3 pos = position();
        acceleration = GravityManager.getGravityAcceleration(pos);
        da = Vector3.zero;
        if (waypoints.Count > 0)
        {
            targetPoint = waypoints[0];
            if (Vector3.Distance(pos, targetPoint) < Defines.EPSD)
            {
                waypoints.RemoveAt(0);
                isBraking = false;
            }
            else if (fullSpeed)
            {
                da = getThrust(acceleration, targetPoint - pos);
            }
            else
            {
                da = getStopThrust(acceleration, targetPoint - pos);
            }
            if (!isEnemy)
                updateWayLine();
            da = da.normalized * Mathf.Clamp(da.magnitude, -maxAcceleration, maxAcceleration);
            acceleration += da;
        }
        acceleration /= Defines.DistScale;
    }

    public void resetWayPoints()
    {
        waypoints.Clear();
        isBraking = false;
    }

    public void setTargetWaypoint(Vector3 waypoint)
    {
        resetWayPoints();
        waypoints.Add(waypoint);
    }

    public void addWayPoint(Vector3 point)
    {
        waypoints.Add(point);
    }

    public void SetTarget(MovableObject newTarget)
    {
        if (newTarget == this)
        {
            Debug.LogWarning($"{gameObject.name} не может выбрать себя целью!");
            return;
        }
        targetMO = newTarget;
        Debug.Log($"[{gameObject.name}] назначил цель: {targetMO.gameObject.name}");
    }

    // Очистка цели
    public void ClearTarget()
    {
        targetMO = null;
    }

    private void updateWayLine()
    {
        if (lineRenderer == null)
        {
            Debug.LogWarning("No line renderer");
            return;
        }
        int totalPoints = waypoints.Count + 1;
        lineRenderer.positionCount = totalPoints;
        // Debug.Log($"Pos link transform: {gameObject.name}:  {linkTransform.position}");
        lineRenderer.SetPosition(0, linkTransform.position - transform.position);
        for (int i = 0; i < waypoints.Count; i++)
        {
            lineRenderer.SetPosition(i + 1, waypoints[i] - transform.position);
        }
        var width = baseLineSize / targetZoomSize * CameraController.Zoom();
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
    }

    public void move()
    {
        float dt = TimeController.dt();
        dv = acceleration * dt;
        transform.position += speed * dt + dv * dt / 2;
        speed += dv;
    }

    public Vector3 position()
    {
        return transform.position;
    }

    public virtual void Update()
    {
        direction = speed.normalized;
        if (Mathf.Abs((position() - targetPoint).magnitude) <= 1e-6f && speed.magnitude < 0.001)
        {
            hasTargetPoint = false;
        }
        updateAcceleration();
        move();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // 1. Вектор скорости (Зеленый)
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + speed * 2f);

        // 2. Тяга двигателей (Красный)
        if (da.magnitude > 0.01f) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + da.normalized * Mathf.Min(da.magnitude, 5f)); 
        }

        // 3. Гравитация (Желтый)
        Vector3 grav = GravityManager.getGravityAcceleration(transform.position);
        if (grav.magnitude > 0.01f) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + grav.normalized * Mathf.Min(grav.magnitude, 3f));
        }

        // 4. Ускорение (Синий)
        if (acceleration.magnitude > 0.01f) {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + acceleration * Mathf.Min(acceleration.magnitude, 3f));
        }

        // 5. Линия к цели (Магента)
        if (targetMO != null && showTargetLine)
        {
            Gizmos.color = targetLineColor;
            Gizmos.DrawLine(transform.position, targetMO.getLightPositionFrom(transform.position));
        }
    }
#endif
}