using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HandheldCamera : MonoBehaviour
{
    //public bool powerOn = true; // When false, camera is off. Screen is black.
    public enum CameraMode { Physical, PostProcessing }
    public CameraMode cameraMode = CameraMode.Physical;
    
    [Header("Camera Settings")]
    [Range(24f, 200f)] public float zoom = 50.0f;           // 24mm to 200mm (35mm equivalent)
    [Range(0.5f, 100f)] public float focusDistance = 2.0f;   // 0.5m to infinity
    [Header("Exposure Settings")]
    [Range(1.4f, 16.0f)] public float aperture = 2.8f;        // f/1.4 to f/16
    [Range(100, 6400)] public int iso = 400;                // 100 to 6400
    [Range(1f, 1000f)] public float shutterSpeed = 125.0f;    // 1/1s to 1/1000s

    private Camera cam;
    private Volume postProcessVolume;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        postProcessVolume = GetComponentInChildren<Volume>();
    }
    void OnValidate()
    {
        UpdateCameraSettings();
    }

    void UpdateCameraSettings()
    {   
        switch (cameraMode)
        {
            case CameraMode.Physical:
                UpdatePhysical();
                break;
            case CameraMode.PostProcessing:
                UpdatePostProcess();
                break;
        }
    }


    void UpdatePhysical()
    {
        if (cam != null)
        {
            cam.iso = iso;
            cam.shutterSpeed = shutterSpeed;
            cam.aperture = aperture;
            cam.focalLength = zoom;
            cam.focusDistance = focusDistance;
        }
    }
    // Alternative method using Post Processing effects
    // Very unsure and untested.
    void UpdatePostProcess()
    {
        /*
        Breakdown of settings effects:
        Zoom - adjusts the cam.fieldOfView
        
        Aperture - affects depth of field post-processing effect
        ISO - affects film grain post-processing effect
        Shutter Speed - affects motion blur post-processing effect

        Focus Distance - affects depth of post-processing effect

        Additionally, the exposure can be simulated by adjusting the color grading exposure compensation
        */
        if (cam != null)
        {
            // Update Field of View based on zoom
            float focalLength = zoom;
            // TODO: what is sensor height?
            float sensorHeight = 24.0f; // Assuming a full-frame sensor height in mm
            cam.fieldOfView = 2.0f * Mathf.Atan((sensorHeight / 2.0f) / focalLength) * Mathf.Rad2Deg;
        }

        if (postProcessVolume != null)
        {
            // Update Depth of Field settings
            if (postProcessVolume.profile.TryGet<DepthOfField>(out var dof))
            {
                dof.aperture.value = aperture;
                dof.focusDistance.value = focusDistance;
            }
            else { Debug.LogWarning("DepthOfField not found in Post Processing Profile"); }

            // Update Film Grain settings to emulate ISO effect
            if (postProcessVolume.profile.TryGet<FilmGrain>(out var filmGrain))
            {
                // Map ISO to grain intensity
                // TODO: refine mapping
                filmGrain.intensity.value = Mathf.Clamp01((iso - 100) / (6400 - 100));
            }
            else { Debug.LogWarning("FilmGrain not found in Post Processing Profile"); }

            if (postProcessVolume.profile.TryGet<ColorAdjustments>(out var colorAdjustments))
            {
                // Simulate exposure adjustment based on shutter speed, aperture, and ISO
                float exposureCompensation = Mathf.Log10((aperture * aperture) / shutterSpeed * (iso / 100f));
                colorAdjustments.postExposure.value = exposureCompensation;
            }
            else
            {
                Debug.LogWarning("ColorAdjustments not found in Post Processing Profile");
            }
        }
    }
}

/*
Aperture: f/1.4 to f/16 

     f/1.4-2.8 = very shallow depth of field
     f/4-5.6 = moderate depth of field  
     f/8-16 = everything in focus
     

ISO: 100 to 6400 

     100-400 = clean, low noise
     800-1600 = moderate noise
     3200-6400 = high noise/grain
     

Focus Distance: 0.5m to infinity

     0.5-2m = close subjects
     2-10m = medium distance
     10m+ = distant subjects/landscapes
     

Zoom: 24mm to 200mm (35mm equivalent) 

     24-35mm = wide angle
     50-85mm = normal/portrait
     100-200mm = telephoto
*/
