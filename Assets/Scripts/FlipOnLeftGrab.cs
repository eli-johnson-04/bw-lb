using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class FlipOnLeftGrab : MonoBehaviour
{
    private XRGrabInteractable grabbable;

    [SerializeField] private Transform CameraModel;
    [SerializeField] private GameObject leftyScreen;
    [SerializeField] private GameObject rightyScreen;


    private Vector3 rightyScale;
    private Vector3 leftyScale;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grabbable = GetComponent<XRGrabInteractable>();
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
                leftyScreen.SetActive(true);
                rightyScreen.SetActive(false);
                break;
            case InteractorHandedness.Right:
                CameraModel.localScale = rightyScale;
                leftyScreen.SetActive(false);
                rightyScreen.SetActive(true);
                break;
        };
    }
}
