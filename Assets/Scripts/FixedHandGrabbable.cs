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

    [SerializeField] private InputActionReference thumbstickAction;

    [Header("Thumbstick Events")]
    public UnityEvent onScrollUp;
    public UnityEvent onScrollDown;
    public UnityEvent onIncrement;
    public UnityEvent onDecrement;
    
    private bool wasPressed = false;

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        if (interactorsSelecting.Count == 0)
            attachTransform = args.interactorObject.handedness switch
            {
                InteractorHandedness.Left => leftHandAttachTransform,
                InteractorHandedness.Right => rightHandAttachTransform,
                _ => attachTransform
            };
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
        if (interactorsSelecting.Count != 0)
            attachTransform = interactorsSelecting[0].handedness switch
            {
                InteractorHandedness.Left => leftHandAttachTransform,
                InteractorHandedness.Right => rightHandAttachTransform,
                _ => attachTransform
            };
        base.OnSelectExited(args);
    }

    void Update()
    {
        if (!isSelected) return;
        
        Vector2 thumbstick = thumbstickAction.action.ReadValue<Vector2>();
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
}
