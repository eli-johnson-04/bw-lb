using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UI;


public class HandheldCamera : MonoBehaviour
{
    const int SCREEN_WIDTH = 1920;
    const int SCREEN_HEIGHT = 1080; // this should match the RenderTexture size assigned to the Camera

    // Button in editor to take photo.
    public bool takePhotoButton = false;

    [Header("Camera Settings")]
    [Range(1f, 100f)] public float zoom = 60f;               // Field of View
    [Range(0.5f, 100f)] public float focusDistance = 2.0f;   // 0.5m to infinity
    [Header("Exposure Settings")]
    [Range(1.0f, 32.0f)] public float aperture = 2.8f;        // f/1.0 to f/32.0
    
    // General 'exposure' setting, affects brightness
    [Range(-2f, 2f)] public float postExposure = 0f;
    //public Texture2D[] photoGallery = new Texture2D[10];

    // These are settings for how sensitive the increments are when the player adjusts settings
    [Header("Adjustment Settings")]
    [Range(1f, 20f)] public float zoomStep = 5f;
    [Range(0.1f, 5f)] public float apertureStep = 0.5f;
    [Range(0.1f, 5f)] public float focusDistanceStep = 0.1f;
    [Range(0.01f, 0.5f)] public float postExposureStep = 0.2f;
    [SerializeField] private Text ui;

    [SerializeField] private Camera cam;
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private GameObject photoPrefab;
    [SerializeField] private Shutter shutterSound;

    private enum AdjustmentSetting
    {
        Zoom,
        Aperture,
        FocusDistance,
        PostExposure
    }
    private AdjustmentSetting currentSetting = AdjustmentSetting.Zoom;

    void Start()
    {
        // ui.enabled = false;
        ui.gameObject.SetActive(false);
    }

    public void TakePhoto()
    {
        // Note, this does not implement any sort of cooldown.
        // TODO: add cooldown to prevent spamming photos.
        Texture2D photo = CaptureToTexture2D();
        //AddPhotoToGallery(photo);
        PrintPhoto(photo);
        PlayShutterSound();
    }

   public void IncrementSetting(bool isCountinuous = false) {
         ChangeSetting(isCountinuous, 1);
   }
   
   public void DecrementSetting(bool isContinuous = false) {
        ChangeSetting(isContinuous, -1);
   }

   public void NextSetting() {
        currentSetting = (AdjustmentSetting)(((int)currentSetting + 1) % Enum.GetNames(typeof(AdjustmentSetting)).Length);
        UpdateTextUI();
   }

   public void PreviousSetting()
    {
        currentSetting = (AdjustmentSetting)(((int)currentSetting - 1) % Enum.GetNames(typeof(AdjustmentSetting)).Length);
        UpdateTextUI();
    }

   private void ChangeSetting(bool isContinuous, float direction) {
        if (isContinuous) {
            direction *= Time.deltaTime;
        }
        
        switch (currentSetting)
        {
            case AdjustmentSetting.Zoom:
                zoom = Mathf.Clamp(zoom + direction * zoomStep, 1f, 100f);
                break;
            case AdjustmentSetting.Aperture:
                aperture = Mathf.Clamp(aperture + direction * apertureStep, 0.5f, 32f);
                break;
            case AdjustmentSetting.FocusDistance:
                focusDistance = Mathf.Clamp(focusDistance + direction * focusDistanceStep, 0.5f, 100f);
                break;
            case AdjustmentSetting.PostExposure:
                postExposure = Mathf.Clamp(postExposure + direction * postExposureStep, -2f, 2f);
                break;
        };
        UpdateCameraSettings();
        UpdateTextUI();
    }

    public void ToggleUIVisible()
    {
        ui.gameObject.SetActive(!ui.gameObject.activeInHierarchy);
    }

    private void UpdateTextUI() {
        switch (currentSetting)
        {
            case AdjustmentSetting.Zoom:
                ui.text = zoom.ToString("F1") + " mm";
                break;
            case AdjustmentSetting.Aperture:
                ui.text = "f/" + aperture.ToString("F1");
                break;
            case AdjustmentSetting.FocusDistance:
                ui.text = focusDistance.ToString("F2") + " m";
                break;
            case AdjustmentSetting.PostExposure:
                ui.text = postExposure.ToString("F1") + " ISO";
                break;
        };
    }

    
    void OnValidate()
    {
        UpdateCameraSettings();
        if (takePhotoButton)
        {
#if UNITY_EDITOR
            // Defer the capture so rendering / editor state has a chance to update.
            // EditorApplication.delayCall runs on the main thread on the next editor update.
            UnityEditor.EditorApplication.delayCall += () =>
            {
                // Guard in case the object was destroyed between scheduling and execution
                if (this != null)
                {
                    TakePhoto();
                }
            };
            // Clear the button immediately to avoid scheduling multiple calls.
            takePhotoButton = false;
#else
            TakePhoto();
            takePhotoButton = false;
#endif
        }
    }

    void UpdateCameraSettings()
    {
        // Update zoom
        if (cam != null)
        {
            cam.fieldOfView = zoom;
        }
        else
        {
            Debug.LogWarning("Camera component not found.");
        }

        // Exposure and Depth of Field require Post Processing
        if (postProcessVolume == null)
        {
            Debug.LogWarning("Post Process Volume component not found.");
            return; ;
        }
        if (postProcessVolume.profile == null) 
        {
            Debug.LogWarning("Post Process Volume profile is null.");
            return;
        }
        // Update Depth of Field settings
        DepthOfField dof;
        if (postProcessVolume.profile.TryGet<DepthOfField>(out dof))
        {   
            // Ensure we're using Bokeh mode
            dof.mode.value = DepthOfFieldMode.Bokeh;
            // Bokeh effect settings
            dof.focusDistance.value = focusDistance;
            dof.aperture.value = aperture;
            dof.focalLength.value = cam.focalLength;

            // Gaussian effect settings can be changed too but we'll stick with Bokeh for now.
            // (DOF is either in Bokeh mode or Gaussian mode, must pick settings to change accordingly)
        }
        else
        {
            Debug.LogWarning("Depth of Field not found in Post Process Volume profile.");
        }
        // Update Exposure settings
        ColorAdjustments colorAdjustments;
        if (postProcessVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            colorAdjustments.postExposure.value = postExposure;
        }
        else
        {
            Debug.LogWarning("Color Adjustments not found in Post Process Volume profile.");
        }
    }
    Texture2D CaptureToTexture2D()
    {
        if (cam == null)
        {
            Debug.LogWarning("CaptureToTexture2D: camera is null.");
            return null;
        }
        RenderTexture rt = cam.targetTexture;
        // Force the camera to render into the RT so we read the latest frame
        cam.Render();
        Texture2D photo = new(rt.width, rt.height);
        // Read the pixels from the active RT
        Graphics.ConvertTexture(rt, photo);
        return photo;
    }

    // void AddPhotoToGallery(Texture2D photo)
    // {
    //     // Store photo in the first available slot in photoGallery
    //     for (int i = 0; i < photoGallery.Length; i++)
    //     {
    //         if (photoGallery[i] == null)
    //         {
    //             photoGallery[i] = photo;
    //             break;
    //         }
    //     }
    // }

    void PlayShutterSound()
    {
        if (shutterSound != null)
        {
            shutterSound.PlayShutter();
        }
        else
        {
            Debug.LogWarning("Shutter sound component not found.");
        }
    }

    void PrintPhoto(Texture2D photo)
    {
        // Instantiate a photoPrefab in the scene to visualize the taken photo
        if (photoPrefab == null)
        {
            Debug.LogWarning("Photo Prefab not assigned.");
            return;
        }
        GameObject photoObject = Instantiate(
            photoPrefab,                                    // prefab to instantiate
            transform.position + transform.forward * 0.5f,  // spawn position
            Quaternion.LookRotation(transform.forward)      // spawn rotation
        );
        PhotoItem photoItem = photoObject.GetComponent<PhotoItem>(); 
        photoItem.Initialize(photo);
    }
}