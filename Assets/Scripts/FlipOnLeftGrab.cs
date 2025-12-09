using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class FlipOnLeftGrab : MonoBehaviour
{
    private XRGrabInteractable grabbable;

    [SerializeField] private Transform CameraModel;
    [SerializeField] private Transform Screen;
    [SerializeField] private Transform screenPositionWhenGrabbedByLeftHand;
    private Vector3 defaultScreenPosition;


    private Vector3 rightyScale;
    private Vector3 leftyScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
        defaultScreenPosition = Screen.transform.localPosition;
        rightyScale = CameraModel.localScale;
        leftyScale = new Vector3(-rightyScale.x, rightyScale.y, rightyScale.z);
    }

    // Update is called once per frame
    void Update()
    {
        switch (grabbable.GetOldestInteractorSelecting()?.handedness)
        {
            case InteractorHandedness.Left:
                CameraModel.localScale = leftyScale;
                Screen.transform.localPosition = screenPositionWhenGrabbedByLeftHand.localPosition;
                break;
            case InteractorHandedness.Right:
                CameraModel.localScale = rightyScale;
                Screen.transform.localPosition = defaultScreenPosition;
                break;
        };
    }
}
