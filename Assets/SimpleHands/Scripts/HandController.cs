using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

// Gets input for animating the hand

public class HandController : MonoBehaviour
{
    [SerializeField] private InputActionReference gripAction;
    [SerializeField] private InputActionReference triggerAction;
    public Hand hand;

    // Update is called once per frame
    void LateUpdate()
    {
        Debug.Log("GripValue" + gripAction.action.ReadValue<float>());
        Debug.Log("TriggerValue" + triggerAction.action.ReadValue<float>());
        hand.SetGrip(gripAction.action.ReadValue<float>());
        hand.SetTrigger(triggerAction.action.ReadValue<float>());
    }
}
