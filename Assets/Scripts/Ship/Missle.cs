using AstroFleet;
using UnityEngine;

public class Missle : MovableObject
{
    public GameObject targetGO;
    public Sprite explosionSprite;
    public Sprite gameOverSprite;   // спрайт для поражения
    public Sprite victorySprite;    // (опционально) спрайт для победы
    private MovableObject target;
    private float alfa = Mathf.PI * 0.2f;
    

    [Header("Maneuver Detection")]
    [Tooltip("Минимальный угол поворота цели (в градусах) для активации маневра ракеты")]
    public float turnAngleThreshold = 5f;

    [Tooltip("Сколько кадров помнить направление скорости цели")]
    public int directionHistoryFrames = 3;

    [Header("Maneuver Response")]
    [Tooltip("Насколько сдвигаем точку назад при маневре цели")]
    public float backwardOffsetMultiplier = 0.5f;

    [Tooltip("Насколько сдвигаем точку вбок при маневре цели")]
    public float lateralOffsetMultiplier = 0.3f;

    [Header("Arming & Safety")]
    public float armingTime = 0.5f;

    private float spawnTime;

    private Vector3[] targetVelocityHistory;
    private int historyIndex = 0;
    private bool historyInitialized = false;

    public void Initialize(Vector3 startPosition, GameObject targetObject, Vector3 startSpeed)
    {
        setTarget(targetObject);
        this.transform.position = startPosition;
        this.speed = startSpeed;
        spawnTime = Time.time;

        targetVelocityHistory = new Vector3[directionHistoryFrames];
        for (int i = 0; i < directionHistoryFrames; i++)
        {
            targetVelocityHistory[i] = Vector3.forward;
        }
    }

    public void setTarget(GameObject newtarget)
    {
        if (newtarget != null)
        {
            target = newtarget.GetComponent<MovableObject>();
            targetGO = newtarget;
        }
    }

    public override void Start()
    {
        base.Start();
        if (targetGO != null)
        {
            target = targetGO.GetComponent<MovableObject>();
        }
        fullSpeed = true;

        targetVelocityHistory = new Vector3[directionHistoryFrames];
        for (int i = 0; i < directionHistoryFrames; i++)
        {
            targetVelocityHistory[i] = Vector3.forward;
        }
    }

    private bool IsTargetManeuvering(out float turnAngle, out Vector3 turnDirection)
    {
        turnAngle = 0f;
        turnDirection = Vector3.zero;

        if (target == null || !historyInitialized) return false;

        Vector3 currentDir = target.speed.sqrMagnitude > 0.1f ? target.speed.normalized : Vector3.forward;

        Vector3 averageDir = Vector3.zero;
        for (int i = 0; i < directionHistoryFrames; i++)
        {
            averageDir += targetVelocityHistory[i];
        }
        averageDir /= directionHistoryFrames;
        averageDir = averageDir.normalized;

        turnAngle = Vector3.Angle(currentDir, averageDir);

        if (turnAngle > 0.1f)
        {
            turnDirection = (currentDir - averageDir).normalized;
        }

        return turnAngle > turnAngleThreshold;
    }

    void calculateTargetPoint()
    {
        if (target == null || targetGO == null) return;

        Vector3 myPos = transform.position;
        Vector3 targetPos = target.getLightPositionFrom(transform.position);
        Vector3 targetVel = target.speed;

        float dt = TimeController.dt();
        Vector3 targetAcc = (dt > 0) ? target.dv / dt : Vector3.zero;

        float dist = Vector3.Distance(myPos, targetPos);
        float relativeSpeed = Mathf.Max((speed - targetVel).magnitude, 1f);
        float t = dist / relativeSpeed;

        float maxAccInUnity = maxAcceleration / Defines.DistScale;

        for (int i = 0; i < 8; i++)
        {
            Vector3 predictedTargetPos = targetPos + targetVel * t + 0.5f * targetAcc * t * t;
            Vector3 toPredicted = predictedTargetPos - myPos;
            float predictedDist = toPredicted.magnitude;

            float a = maxAccInUnity;
            float v0 = speed.magnitude;
            float discriminant = v0 * v0 + 2 * a * predictedDist;

            if (discriminant >= 0 && a > 0)
            {
                t = (-v0 + Mathf.Sqrt(discriminant)) / a;
            }
            else
            {
                t = predictedDist / Mathf.Max(v0, 0.1f);
            }
        }

        Vector3 finalInterceptPoint = targetPos + targetVel * t + 0.5f * targetAcc * t * t;

        float turnAngle;
        Vector3 turnDirection;

        if (IsTargetManeuvering(out turnAngle, out turnDirection))
        {
            Vector3 missileDir = speed.sqrMagnitude > 0.1f ? speed.normalized : Vector3.forward;

            Vector3 backwardOffset = -missileDir * backwardOffsetMultiplier;
            Vector3 lateralOffset = turnDirection * lateralOffsetMultiplier;

            finalInterceptPoint += backwardOffset + lateralOffset;
        }

        setTargetWaypoint(finalInterceptPoint);

        if (target != null)
        {
            Vector3 currentDir = target.speed.sqrMagnitude > 0.1f ? target.speed.normalized : Vector3.forward;
            targetVelocityHistory[historyIndex] = currentDir;
            historyIndex = (historyIndex + 1) % directionHistoryFrames;
            historyInitialized = true;
        }
    }

    private void selfDestruct()
    {
        Destroy(gameObject);
    }

    public override void Update()
    {
        if (float.IsNaN(transform.position.x) || float.IsNaN(speed.x))
        {
            Debug.LogError("<color=red>[Missile]</color> Physics broke (NaN detected)! Destroying.");
            Destroy(gameObject);
            return;
        }

        if (targetGO == null)
        {
            base.Update();
            selfDestruct();
            return;
        }

        calculateTargetPoint();
        base.Update();
        
        if (ROI != null && da.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(da.y, da.x) * Mathf.Rad2Deg - 90f;
            ROI.SetIconRotation(angle);
        }

        float distToTarget = Vector3.Distance(transform.position, target.getLightPositionFrom(transform.position));
        
        if (distToTarget < Defines.EPSD * 2)
        {
            MovableObject targetMO = targetGO?.GetComponent<MovableObject>();
            if (targetMO != null)
            {
                if (targetMO.isFlagship)
                {
                    Debug.Log("Флагман уничтожен! Поражение.");
                    if (gameOverSprite != null)
                    {
                        Vector3 screenCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
                        GameOverDisplay.Show(gameOverSprite); 
                    }
                }
                else if (targetMO.isEnemy)
                {
                    EnemyCounter.Count--;
                    Debug.Log($"Осталось врагов: {EnemyCounter.Count}");
                    if (EnemyCounter.Count <= 0 && victorySprite != null)
                    {   
                        Debug.Log("Все враги уничтожены! Победа.");
                        GameOverDisplay.Show(victorySprite);
                    }
                }
            }
            Explosion.Spawn(transform.position, explosionSprite, 1f);
            Destroy(targetGO);
            selfDestruct();
        }
    }
}
