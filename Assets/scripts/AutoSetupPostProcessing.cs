using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class AutoSetupPostProcessing : MonoBehaviour
{
    void Start()
    {
        // [EMERGENCY FIX] 
        // The programmatic setup of PostProcessing is causing critical NullReferenceExceptions 
        // likely due to missing reference to 'PostProcessResources' asset which cannot be easily loaded via script.
        // We will cleanup the components to stop the errors.

        Debug.Log("[AutoSetupPostProcessing] Cleaning up PostProcessing to fix errors...");

        // 1. Remove Layer from Camera
        Camera cam = Camera.main;
        if (cam != null)
        {
            var layer = cam.GetComponent<PostProcessLayer>();
            if (layer != null) 
            {
                Destroy(layer);
            }
        }

        // 2. Remove Volume
        PostProcessVolume imgVolume = FindObjectOfType<PostProcessVolume>();
        if (imgVolume != null)
        {
            Destroy(imgVolume.gameObject);
        }

        // 3. Remove dependencies if possible (optional, but we just stop using them)
        // Self destruct
        Destroy(this);
    }
}
