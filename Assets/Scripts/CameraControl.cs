using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private PlayerInput playerInput;
    private InputAction zoomAction;
    private InputAction changeAction;
    private CinemachineVirtualCamera virtualCamera;
    private float zoomRange = 40f;
    private bool thirdPersonCamera;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        zoomAction = playerInput.actions["Zoom"];
        changeAction = playerInput.actions["ChangeCamera"];
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        thirdPersonCamera = true;
    }

    void Update()
    {
        zoom();
    }

    private void zoom() //zoom by fov with MMB
    {
        float scroll = zoomAction.ReadValue<float>();
        if(scroll > 0){
            zoomRange -= 2;
        }
        else if(scroll < 0){
            zoomRange += 2;
        }
        zoomRange = Mathf.Clamp(zoomRange, 2, 40);
        virtualCamera.m_Lens.FieldOfView = zoomRange;
    }

    private void OnEnable()
    {
        changeAction.performed += _ => change();
    }

    private void OnDisable()
    {
        changeAction.performed -= _ => change();
    }
    
    private void change() //change camera and disable render when at first person
    {
        thirdPersonCamera = !thirdPersonCamera;
        if(thirdPersonCamera){
            virtualCamera.m_Priority += 2;
            Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = true;
            }
        }
        else{
            virtualCamera.m_Priority -= 2;
            Renderer[] renderers = player.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                r.enabled = false;
            }
        }
    }
}
