using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]

public class TurretController : MonoBehaviour
{
    [SerializeField] private bool hasLimitedTraverse = false;
    [SerializeField] private Transform gun;
    [SerializeField] private Transform turretBase;
    [SerializeField] private Transform barrel;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletParent;
    [SerializeField] private float LeftLimit = 120f;
    [SerializeField] private float RightLimit = 120f;
    [SerializeField] private float MaxElevation = 60f;
    [SerializeField] private float MaxDepression = 10;

    
    private Transform cameraTransform;
    private Vector3 AimPosition = Vector3.zero;
    private PlayerInput playerInput;
    private InputAction shootAction;

    private float aimedThreshold = 5f;
    private float limitedTraverseAngle = 0f;
    private float TraverseSpeed = 60f;
    private float ElevationSpeed = 30f;
    private float angleToTarget = 0f;
    private float elevation = 0f;
    private float maxRange = 3000f;
    private float bulletHitMissDisatance = 200f;
    private float timeSinceLastShot = 0f;
    private float fireRate = 30f;

    private int playerLayer;

    private bool hasBarrels = false;
    private bool isAimed = false;
    private bool IsIdle = false;
    private bool isBarrelAtRest = false;
    private bool isBaseAtRest = false;

    public bool HasLimitedTraverse { get { return hasLimitedTraverse; } }
    public bool IsTurretAtRest { get { return isBarrelAtRest && isBaseAtRest; } }
    public bool IsAimed { get { return isAimed; } }
    public float AngleToTarget { get { return IsIdle ? 999f : angleToTarget; } }

    private void Awake()
        {
            cameraTransform = Camera.main.transform;
            playerInput = GetComponent<PlayerInput>();
            hasBarrels = gun != null;
            if (turretBase == null)
                Debug.LogError(name + ": TurretAim requires an assigned TurretBase!");
            shootAction = playerInput.actions["Shoot"];
            playerLayer = 1 << 7;
            playerLayer = ~playerLayer;
        }
    
    private bool CanShoot() => timeSinceLastShot > 1f / (fireRate / 60f);   //gun reload speed

    private void OnEnable()
    {
        shootAction.performed += _ => ShootGun();
    }

    private void OnDisable()
    {
        shootAction.performed -= _ => ShootGun();
    }
    //fire gun
    private void ShootGun()
    {
        if(isAimed && CanShoot()){
            RaycastHit hit;
            GameObject bullet = GameObject.Instantiate(bulletPrefab, barrel.position, Quaternion.identity, bulletParent);
            BulletController bulletController = bullet.GetComponent<BulletController>();
            if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity, playerLayer)){      //shoot bullet, ignore certain layer
                bulletController.target = hit.point;
                bulletController.hit = true;
            }
            else{
                bulletController.target = cameraTransform.position + cameraTransform.forward * bulletHitMissDisatance;  //if raycast not hit anything shoot air
                bulletController.hit = false;
            }
            timeSinceLastShot = 0;
        }
    }

    private void Update()
    {
        if (IsIdle)
        {
            if (!IsTurretAtRest)
                RotateTurretToIdle();
            isAimed = false;
        }
        else
        {
            //get aiming direction
            RaycastHit target;
            if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out target, Mathf.Infinity, playerLayer)){
                AimPosition = target.point;
            }
            else{
                AimPosition = cameraTransform.position + cameraTransform.forward * maxRange;
            }
            
            RotateBaseToFaceTarget(AimPosition);

            if (hasBarrels)
                RotateBarrelsToFaceTarget(AimPosition);

            angleToTarget = GetTurretAngleToTarget(AimPosition);
            isAimed = angleToTarget < aimedThreshold;

            isBarrelAtRest = false;
            isBaseAtRest = false;
        }
        timeSinceLastShot += Time.deltaTime;
    }
    
    private float GetTurretAngleToTarget(Vector3 targetPosition)    //get turret angle for isAimed
    {
        float angle = 999f;

        if (hasBarrels)
        {
            angle = Vector3.Angle(targetPosition - gun.position, gun.forward);
        }
        else
        {
            Vector3 flattenedTarget = Vector3.ProjectOnPlane(
                targetPosition - turretBase.position,
                turretBase.up);

            angle = Vector3.Angle(
                flattenedTarget - turretBase.position,
                turretBase.forward);
        }

        return angle;
    }
    
    private void RotateTurretToIdle()   //rotate turret to idle
    {
        // Rotate the base to its default position.
        if (hasLimitedTraverse)
        {
            limitedTraverseAngle = Mathf.MoveTowards(
                limitedTraverseAngle, 0f,
                TraverseSpeed * Time.deltaTime);

            if (Mathf.Abs(limitedTraverseAngle) > Mathf.Epsilon)
                turretBase.localEulerAngles = Vector3.up * limitedTraverseAngle;
            else
                isBaseAtRest = true;
        }
        else
        {
            turretBase.rotation = Quaternion.RotateTowards(
                turretBase.rotation,
                turretBase.rotation,
                TraverseSpeed * Time.deltaTime);

            isBaseAtRest = Mathf.Abs(turretBase.localEulerAngles.y) < Mathf.Epsilon;
        }

        if (hasBarrels)
        {
            elevation = Mathf.MoveTowards(elevation, 0f, ElevationSpeed * Time.deltaTime);
            if (Mathf.Abs(elevation) > Mathf.Epsilon)
                gun.localEulerAngles = Vector3.right * -elevation;
            else
                isBarrelAtRest = true;
        }
        else // Barrels automatically at rest if there are no barrels.
            isBarrelAtRest = true;
    }
    
    private void RotateBarrelsToFaceTarget(Vector3 targetPosition)  //rotate barrel to target
    {
        Vector3 localTargetPos = turretBase.InverseTransformDirection(targetPosition - gun.position);
        Vector3 flattenedVecForBarrels = Vector3.ProjectOnPlane(localTargetPos, Vector3.up);

        float targetElevation = Vector3.Angle(flattenedVecForBarrels, localTargetPos);
        targetElevation *= Mathf.Sign(localTargetPos.y);

        targetElevation = Mathf.Clamp(targetElevation, -MaxDepression, MaxElevation);
        elevation = Mathf.MoveTowards(elevation, targetElevation, ElevationSpeed * Time.deltaTime);

        if (Mathf.Abs(elevation) > Mathf.Epsilon)
            gun.localEulerAngles = Vector3.right * -elevation;
    }
    
    private void RotateBaseToFaceTarget(Vector3 targetPosition) //rotate turret to target
    {
        Vector3 turretUp = transform.up;

        Vector3 vecToTarget = targetPosition - turretBase.position;
        Vector3 flattenedVecForBase = Vector3.ProjectOnPlane(vecToTarget, turretUp);

        if (hasLimitedTraverse)
        {
            Vector3 turretForward = transform.forward;
            float targetTraverse = Vector3.SignedAngle(turretForward, flattenedVecForBase, turretUp);
            targetTraverse = Mathf.Clamp(targetTraverse, -LeftLimit, RightLimit);
            limitedTraverseAngle = Mathf.MoveTowards(
                limitedTraverseAngle,
                targetTraverse,
                TraverseSpeed * Time.deltaTime);

            if (Mathf.Abs(limitedTraverseAngle) > Mathf.Epsilon)
                turretBase.localEulerAngles = Vector3.up * limitedTraverseAngle;
        }
        else
        {
            turretBase.rotation = Quaternion.RotateTowards(
                Quaternion.LookRotation(turretBase.forward, turretUp),
                Quaternion.LookRotation(flattenedVecForBase, turretUp),
                TraverseSpeed * Time.deltaTime);
        }
    }
}
