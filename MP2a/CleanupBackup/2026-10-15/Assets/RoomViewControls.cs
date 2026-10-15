using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class RoomViewControls : MonoBehaviour
{
    public Camera overviewCamera;
    public Camera standingCamera;
    public CharacterController walker;
    public DesktopBuildInput buildInput;
    public float walkSpeed = 2f;
    public float lookSensitivity = 0.15f;

    private bool standingView;
    private float pitch;
    private float verticalSpeed;

    private void Start()
    {
        // Use the installed rig in both the simulator and a real headset.
        // Headset connection is not required for the VR camera to take priority.
        foreach (XROrigin rig in FindObjectsByType<XROrigin>())
        {
            Camera vrCamera = rig.Camera;
            if (vrCamera == null || !vrCamera.gameObject.activeInHierarchy)
                continue;

            DisableDesktopCamera(overviewCamera);
            DisableDesktopCamera(standingCamera);
            vrCamera.enabled = true;
            vrCamera.tag = "MainCamera";
            AudioListener listener = vrCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = true;
            if (buildInput != null) buildInput.viewCamera = vrCamera;
            enabled = false;
            return;
        }

        SetView(false);
    }

    private void DisableDesktopCamera(Camera desktopCamera)
    {
        if (desktopCamera == null) return;
        desktopCamera.enabled = false;
        desktopCamera.tag = "Untagged";
        AudioListener listener = desktopCamera.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = false;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;
        if (keyboard.tabKey.wasPressedThisFrame)
            SetView(!standingView);
        if (!standingView || walker == null)
            return;

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.middleButton.isPressed)
        {
            Vector2 delta = mouse.delta.ReadValue() * lookSensitivity;
            walker.transform.Rotate(0f, delta.x, 0f);
            pitch = Mathf.Clamp(pitch - delta.y, -75f, 75f);
            standingCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        Vector3 direction = Vector3.zero;
        if (keyboard.wKey.isPressed) direction += walker.transform.forward;
        if (keyboard.sKey.isPressed) direction -= walker.transform.forward;
        if (keyboard.dKey.isPressed) direction += walker.transform.right;
        if (keyboard.aKey.isPressed) direction -= walker.transform.right;

        if (walker.isGrounded && verticalSpeed < 0f)
            verticalSpeed = -1f;
        verticalSpeed += Physics.gravity.y * Time.unscaledDeltaTime;
        // Allow inspecting the room during the paused building phase too.
        walker.Move((direction.normalized * walkSpeed + Vector3.up * verticalSpeed) * Time.unscaledDeltaTime);
    }

    private void SetView(bool standing)
    {
        standingView = standing;
        overviewCamera.enabled = !standing;
        standingCamera.enabled = standing;
        overviewCamera.GetComponent<AudioListener>().enabled = !standing;
        standingCamera.GetComponent<AudioListener>().enabled = standing;
        buildInput.viewCamera = standing ? standingCamera : overviewCamera;
    }
}
