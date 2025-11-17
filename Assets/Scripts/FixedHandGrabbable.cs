using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FixedHandGrabbable : XRGrabInteractable
{
    public Transform leftHandAttachTransform;
    public Transform rightHandAttachTransform;

    [SerializeField] private InputActionReference thumbstickMovement;
    [SerializeField] private InputActionReference thumbstickClickAction;

    [Header("Left Hand Actions to Disable During UI Mode")]
    [SerializeField] private InputActionReference[] leftActionsToDisable;

    [Header("Right Hand Actions to Disable During UI Mode")]
    [SerializeField] private InputActionReference[] rightActionsToDisable;

    [Header("Thumbstick Events")]
    public UnityEvent onClick;
    public UnityEvent onScrollUp;
    public UnityEvent onScrollDown;
    public UnityEvent onIncrement;
    public UnityEvent onDecrement;
    
    private bool wasPressed = false;
    private bool uiModeActive = false;
    private IXRSelectInteractor uiModeInteractor; // track which interactor is in UI mode

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        if (interactorsSelecting.Count == 0)
        {
            attachTransform = args.interactorObject.handedness switch
            {
                InteractorHandedness.Left => leftHandAttachTransform,
                InteractorHandedness.Right => rightHandAttachTransform,
                _ => attachTransform
            };
        }
        else
            secondaryAttachTransform = args.interactorObject.handedness switch
            {
                InteractorHandedness.Left => leftHandAttachTransform,
                InteractorHandedness.Right => rightHandAttachTransform,
                _ => secondaryAttachTransform
            };
        base.OnSelectEntering(args);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        // If the hand that enabled UI mode is releasing, disable UI mode
        if (uiModeActive && args.interactorObject == uiModeInteractor)
        {
            DisableUIMode();
        }
        if (interactorsSelecting.Count != 0)
            attachTransform = interactorsSelecting[0].handedness switch
            {
                InteractorHandedness.Left => leftHandAttachTransform,
                InteractorHandedness.Right => rightHandAttachTransform,
                _ => attachTransform
            };

        wasPressed = false;
        base.OnSelectExited(args);
    }

    void Update()
    {
        if (!isSelected) return;

        // Only respond to thumbstick click from a selecting interactor
        if (thumbstickClickAction?.action.WasPressedThisFrame() == true) 
        {
            // Check if ANY selecting interactor's controller pressed the button
            foreach (var interactor in interactorsSelecting)
            {
                // if thumbstick was pressed and this hand is selecting, assume it was this hand
                // (InputSystem sucks and wont let us see which device triggered the thumbstick click action ugh)
                if (!uiModeActive)
                {
                    EnableUIMode(interactor);
                    break;
                }
                else if (interactor == uiModeInteractor)
                {
                    DisableUIMode();
                    break;
                }
            }
            ToggleUIMode();
        }

        // Only process thumbstick directional input in UI mode
        if (!uiModeActive) return;

        Vector2 thumbstick = thumbstickMovement.action.ReadValue<Vector2>();
        bool isPressed = thumbstick.magnitude > 0.5f;
        
        if (isPressed && !wasPressed)
        {
            if (Mathf.Abs(thumbstick.x) > Mathf.Abs(thumbstick.y))
            {
                if (thumbstick.x > 0) onScrollUp?.Invoke();
                else onScrollDown?.Invoke();
            }
            else
            {
                if (thumbstick.y > 0) onIncrement?.Invoke();
                else onDecrement?.Invoke();
            }
        }
        
        wasPressed = isPressed;
    }

    void EnableUIMode(IXRSelectInteractor interactor)
    {
        uiModeActive = true;
        uiModeInteractor = interactor;
        InputActionReference[] actionsToDisable = interactor.handedness == InteractorHandedness.Left
            ? leftActionsToDisable
            : rightActionsToDisable;

        foreach (var action in actionsToDisable)
            if (action != null & action.action != null)
                action.action.Disable();

        onClick?.Invoke();
    }

    void DisableUIMode()
    {
        if (!uiModeActive) return;

        InputActionReference[] actionsToEnable = uiModeInteractor.handedness == InteractorHandedness.Left
            ? leftActionsToDisable
            : rightActionsToDisable;

        foreach (var action in actionsToEnable)
            if (action != null && action.action != null)
                action.action.Enable();

        uiModeActive = false;
        uiModeInteractor = null;
        onClick?.Invoke();
    }

    void ToggleUIMode()
    {
        uiModeActive = !uiModeActive;

        // Only disable actions for the hand that's holding the camera
        InputActionReference[] actionsToToggle = activeHand == InteractorHandedness.Left
            ? leftActionsToDisable
            : rightActionsToDisable;

        // Disable/enable other actions that use the thumbstick
        foreach (var action in actionsToToggle)
        {
            if (action != null && action.action != null)
                if (uiModeActive) action.action.Disable();
                else action.action.Enable();
        }

        onClick?.Invoke();
    }
}
