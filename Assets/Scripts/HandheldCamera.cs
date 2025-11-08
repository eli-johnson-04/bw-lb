using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


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
    [Range(-20f, 20f)] public float postExposure = 0f;
    public Texture2D[] photoGallery = new Texture2D[10];

    // These are settings for how sensitive the increments are when the player adjusts settings
    [Header("Adjustment Settings")]
    [Range(0.1f, 5f)] public float apertureStep = 0.5f;
    [Range(0.1f, 5f)] public float focusDistanceStep = 0.1f;
    [Range(0.1f, 5f)] public float postExposureStep = 0.1f;

    [SerializeField] private Camera cam;
    [SerializeField] private Volume postProcessVolume;
    [SerializeField] private GameObject photoPrefab;
    [SerializeField] private Shutter shutterSound;


    public void TakePhoto()
    {
        // Note, this does not implement any sort of cooldown.
        Texture2D photo = CaptureToTexture2D();
        AddPhotoToGallery(photo);
        PrintPhoto(photo);
        PlayShutterSound();
    }
    void OnValidate()
    {
        UpdateCameraSettings();
        if (takePhotoButton)
        {
            TakePhoto();
            takePhotoButton = false;
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
        // Render current camera view to a Texture2D and store in photoGallery
        Texture2D photo = new Texture2D(SCREEN_WIDTH, SCREEN_HEIGHT, TextureFormat.RGB24, false);
        RenderTexture rt = cam.targetTexture;
        RenderTexture.active = rt;
        photo.ReadPixels(new Rect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT), 0, 0);
        photo.Apply();
        RenderTexture.active = null; // Unsure if this reset is correct
        return photo;
    }

    void AddPhotoToGallery(Texture2D photo)
    {
        // Store photo in the first available slot in photoGallery
        for (int i = 0; i < photoGallery.Length; i++)
        {
            if (photoGallery[i] == null)
            {
                photoGallery[i] = photo;
                break;
            }
        }
    }

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