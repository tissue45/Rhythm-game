using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class MouseTrail : MonoBehaviour
{
    public float zDistance = 20.0f; // Distance from camera to bubbles

    void Start()
    {
        SetupTrail();
        // Remove particles
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null) Destroy(ps);
    }

    void SetupTrail()
    {
        TrailRenderer trail = GetComponent<TrailRenderer>();
        trail.startWidth = 1.5f; // 더 두꺼운 트레일
        trail.endWidth = 0.0f;
        trail.time = 0.4f; // Slightly longer
        trail.minVertexDistance = 0.1f;
        trail.numCornerVertices = 10; // Smoother curves
        trail.numCapVertices = 5; // Rounder ends

        // Rainbow Gradient
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.red, 0.0f), 
                new GradientColorKey(new Color(1f, 0.5f, 0f), 0.15f), // Orange
                new GradientColorKey(Color.yellow, 0.3f), 
                new GradientColorKey(Color.green, 0.45f), 
                new GradientColorKey(Color.cyan, 0.6f), 
                new GradientColorKey(Color.blue, 0.75f), 
                new GradientColorKey(new Color(0.5f, 0f, 1f), 1.0f) // Violet
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        trail.colorGradient = gradient;
        
        // Force Additive Shader for Glowing effect
        Shader additiveShader = Shader.Find("Mobile/Particles/Additive");
        if (additiveShader == null) additiveShader = Shader.Find("Legacy Shaders/Particles/Additive");
        if (additiveShader == null) additiveShader = Shader.Find("Particles/Standard Unlit"); // Fallback

        if (additiveShader != null)
        {
            if (trail.material == null || !trail.material.shader.name.Contains("Additive"))
            {
                 Debug.Log("Forcing Additive Shader on Trail");
                 trail.material = new Material(additiveShader);
            }
        }
        else
        {
             Debug.LogError("Could not find an Additive shader! Trail might look grey.");
        }
    }



    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        
        // Assume Camera.main exists. If 2D/UI camera is different, this might need adjustment.
        // ScreenToWorldPoint requires z to be distance from camera.
        if (Camera.main == null)
        {
            Debug.LogError("Main Camera not found! Tag your camera as 'MainCamera'.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Create a geometric plane at Z = 20.
        // The normal faces the camera.
        // If camera is at Z < 20 (looking +Z), normal is Vector3.back (0,0,-1).
        // If camera is at Z > 20 (looking -Z), normal is Vector3.forward (0,0,1).
        // Let's guess/check or just use the math:
        
        float targetZ = 20f;
        
        // Simple math to find intersection with Z plane:
        // ray.origin.z + ray.direction.z * t = targetZ
        // t = (targetZ - ray.origin.z) / ray.direction.z
        
        if (Mathf.Abs(ray.direction.z) > 0.001f) // Prevent div by zero
        {
            float t = (targetZ - ray.origin.z) / ray.direction.z;
            if (t > 0) // Should be in front of camera
            {
                Vector3 point = ray.GetPoint(t);
                transform.position = point;
            }
        }
    }
}
