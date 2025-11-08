using System;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRDirectInteractor))]
public class AllowTakePicture : MonoBehaviour
{
    private XRDirectInteractor interactor;
    private IXRSelectInteractable selected;
    public string twoHandedGrabbableTag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactor = GetComponent<XRDirectInteractor>();
        interactor.selectEntered.AddListener(CheckDisableActivate);
        interactor.selectExited.AddListener(CheckAllowActivate);
    }

    void CheckDisableActivate(SelectEnterEventArgs args)
    {
        selected = args.interactableObject;
        if (selected.transform.CompareTag(twoHandedGrabbableTag) && !selected.firstInteractorSelecting.Equals(interactor))
            SetAllowActivate(false);
    }

    public void SetAllowActivate(bool b)
    {
        interactor.allowActivate = b;
        Debug.Log(interactor.handedness.ToString() + "interactor now " + (interactor.allowActivate ? "" : "no longer") + " allowed to activate :3 ");
    }

    void CheckAllowActivate(SelectExitEventArgs args)
    {
        if (args.interactableObject.interactorsSelecting.Count == 1)
        {
            if (interactor.allowActivate == false)
                SetAllowActivate(true);
        }
    }

    void Update()
    {
        if (selected != null && !interactor.hasSelection)
        {
            selected.interactorsSelecting[0].transform.TryGetComponent(out AllowTakePicture atp);
            if (atp != null) atp.SetAllowActivate(true);
            selected = null;
        }
    }
}
