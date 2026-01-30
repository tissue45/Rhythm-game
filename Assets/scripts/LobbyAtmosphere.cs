using UnityEngine;

public class LobbyAtmosphere : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float moveAmount = 0.3f; // Strength of camera movement
    public float smoothTime = 0.2f; // Smoothness

    [Header("Particle Settings")]
    public int maxParticles = 100;
    public Color particleColor = new Color(1f, 1f, 1f, 0.9f); // Glowing White (High Alpha)
    public float particleSize = 0.2f;

    private Vector3 _startPos;
    private Camera _cam;
    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        _cam = Camera.main;
        if (_cam != null)
        {
            _startPos = _cam.transform.position;
        }

        // CreateIceParticles(); // [REMOVED] User requested to remove particles
    }

    private ParticleSystem _ps;

    private void Update()
    {
        if (_cam == null) return;

        // Mouse Parallax Logic
        float x = (Input.mousePosition.x / Screen.width) * 2 - 1;
        float y = (Input.mousePosition.y / Screen.height) * 2 - 1;
        Vector3 targetPos = _startPos + new Vector3(x * moveAmount, y * moveAmount, 0);
        _cam.transform.position = Vector3.SmoothDamp(_cam.transform.position, targetPos, ref _velocity, smoothTime);

        /* [REMOVED] Particle Update
        if (_ps != null)
        {
            var main = _ps.main;
            main.startColor = particleColor;
            
            var col = _ps.colorOverLifetime;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] { new GradientColorKey(particleColor, 0.0f), new GradientColorKey(particleColor, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(0f, 0.0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0f, 1.0f) }
            );
            col.color = grad;
        }
        */
    }

    // Programmatically create a Particle System (No assets needed)
    private void CreateIceParticles()
    {
        GameObject pObj = new GameObject("AtmosphereParticles");
        pObj.transform.SetParent(this.transform);
        pObj.transform.localPosition = new Vector3(0, 5, 5); 
        pObj.transform.localRotation = Quaternion.Euler(90, 0, 0); 

        _ps = pObj.AddComponent<ParticleSystem>(); // Cache it
        ParticleSystemRenderer psr = pObj.GetComponent<ParticleSystemRenderer>();
        // ... (rest same, just caching _ps)
        
        // Material
        psr.material = new Material(Shader.Find("Particles/Standard Unlit")); 
        psr.material.color = Color.white; 
        psr.material.mainTexture = CreateCircleTexture(); // [FIX] Use procedural circle texture

        // Main Module
        var main = _ps.main;
        // ...

        main.startLifetime = 10f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, particleSize);
        main.startColor = particleColor;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // Emission
        var emission = _ps.emission;
        emission.rateOverTime = 10f;

        // Shape (Box at top)
        var shape = _ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(20, 1, 10); // Wide area
        
        // Color over Lifetime (Fade out)
        var col = _ps.colorOverLifetime;
        col.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(particleColor, 0.0f), new GradientColorKey(particleColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0.0f), new GradientAlphaKey(0.8f, 0.2f), new GradientAlphaKey(0f, 1.0f) }
        );
        col.color = grad;

        // Size over Lifetime
        var sz = _ps.sizeOverLifetime;
        sz.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0.0f, 0.0f);
        curve.AddKey(0.5f, 1.0f);
        curve.AddKey(1.0f, 0.0f);
        sz.size = new ParticleSystem.MinMaxCurve(1.0f, curve);

        // Noise (Floaty effect)
        var noise = _ps.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 0.5f;
    }

    private Texture2D CreateCircleTexture()
    {
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] colors = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(1f - (dist / radius));
                // Make it sharper circle with soft edge
                alpha = Mathf.Pow(alpha, 3f); 
                colors[y * size + x] = new Color(1, 1, 1, alpha);
            }
        }
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }
}
