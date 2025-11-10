using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FixedHandGrabbable : XRGrabInteractable
{
    public Transform leftHandAttachTransform;
    public Transform rightHandAttachTransform;

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
}
