using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CameraZoom : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction zoomAction;
    private CinemachineVirtualCamera virtualCamera;
    private float zoomRange = 40f;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        zoomAction = playerInput.actions["Zoom"];
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
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
}
