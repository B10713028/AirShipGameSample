using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float elevateForce = 5f;
    [SerializeField] private float turningForce = 3f;
    
    private PlayerInput playerInput;
    private Rigidbody rb;
    private Transform cameraTransform;
    private InputAction moveAction;
    private InputAction elevateAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        cameraTransform = Camera.main.transform;
        moveAction = playerInput.actions["Move"];
        elevateAction = playerInput.actions["Elevate"];

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        move();
    }

    private void move()
    {
        Vector2 wasd = moveAction.ReadValue<Vector2>();
        float elevate = elevateAction.ReadValue<float>();

        //Moving
        rb.AddForce(transform.forward * moveForce * wasd.y);
        //Elevating
        rb.AddForce(transform.up * elevateForce * elevate);
        //Turning
        rb.AddTorque(transform.up * turningForce * wasd.x);
    }
}