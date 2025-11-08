using UnityEngine;
// This script represents a printed photo item.
public class PhotoItem : MonoBehaviour
{
    // Renderer of the child quad to display the photo texture.
    [SerializeField] private Renderer quadRenderer;
    [SerializeField] private Material photoMaterial; // Copied for each instance

    public void Initialize(Texture2D capturedImage)
    {
        // Create a new material instance to avoid shared material issues
        Material instanceMaterial = new Material(photoMaterial);
        instanceMaterial.mainTexture = capturedImage;
        quadRenderer.material = instanceMaterial;
    }
}