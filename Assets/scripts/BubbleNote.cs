using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BubbleNote : MonoBehaviour
{
    private float spawnTime;
    private float lifeTime;
    private float targetTime; 

    private LineRenderer lineRenderer;
    private bool isHit = false;

    private int segments = 50;
    public float xradius = 1.0f; 
    public float yradius = 1.0f;
    private float startRadiusScale = 3.0f; 

    private Camera mainCamera;
    private static Texture2D softVideoLineTex; // Cached Texture

    private float randomScaleFactor = 2.0f;
    private Color noteColor = Color.white;

    public void Initialize(float lifeTime)
    {
        this.lifeTime = lifeTime;
        this.spawnTime = Time.time;
        this.targetTime = spawnTime + lifeTime;
        
        mainCamera = Camera.main; 

        // [FIX] Random Size (1.5x to 2.8x)
        randomScaleFactor = Random.Range(1.8f, 3.0f);

        // [FIX] Random Neon Color
        // High Saturation (0.7-1.0), High Value (1.0) for Neon look
        noteColor = Color.HSVToRGB(Random.value, Random.Range(0.7f, 1f), 1f);
        noteColor.a = 0.8f; // Alpha
    }

    private Renderer targetRenderer; // Visual to effect

    // Revert to standard Start, remove Awake destruction
    // Revert to standard Start, remove Awake destruction
    void Start()
    {
        // 1. Setup Line Renderer (Ring) - NEON GLOW STYLE
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        
        // Defaults
        if (segments < 10) segments = 50; 
        if (xradius < 0.1f) xradius = 1.0f;
        if (yradius < 0.1f) yradius = 1.0f;

        // Draw Circle Geometry
        lineRenderer.positionCount = segments + 1;
        lineRenderer.loop = true; 

        float lx, ly;
        float angle = 0f;
        for (int i = 0; i < (segments + 1); i++)
        {
            lx = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            ly = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            lineRenderer.SetPosition(i, new Vector3(lx, ly, 0));
            angle += (360f / segments);
        }

        // [CRITICAL FIX] Restore "Glow" Texture
        // We create a gradient that is SOLID in the absolute center but FADES OUT to create a "Halo"
        if (softVideoLineTex == null) {
            softVideoLineTex = new Texture2D(1, 128); // Higher resolution gradient
            for (int k = 0; k < 128; k++) { 
                // Normalized pos 0..1
                float t = k / 127f; 
                // Sin wave from 0 to PI (0 -> 1 -> 0)
                float v = Mathf.Sin(t * Mathf.PI); 
                // Power it to make it sharper (less "fat")
                v = Mathf.Pow(v, 3.0f); 
                
                softVideoLineTex.SetPixel(0, k, new Color(1, 1, 1, v));
            }
            softVideoLineTex.Apply();
            softVideoLineTex.wrapMode = TextureWrapMode.Clamp;
        }

        // [FIX] Material Setup
        var lineShader = Shader.Find("Particles/Additive");
        if (lineShader == null) lineShader = Shader.Find("Mobile/Particles/Additive");
        if (lineShader == null) lineShader = Shader.Find("Sprites/Default");
        
        Material ringMat = new Material(lineShader);
        ringMat.mainTexture = softVideoLineTex;
        
        // Color: bright white with transparency to let the additive blend work its magic
        ringMat.color = new Color(1f, 1f, 1f, 1f); 
        lineRenderer.material = ringMat;
        
        // [CRITICAL FIX] Width: 0.5f
        // Since the texture fades out at edges, this 0.5f will look like a 0.1f core with 0.2f glow on sides
        lineRenderer.startWidth = 0.5f; 
        lineRenderer.endWidth = 0.5f;
        
        // Default color (Blue-ish Cyan Glow)
        Color neonColor = new Color(0.2f, 0.8f, 1f, 0.9f);
        lineRenderer.startColor = neonColor;
        lineRenderer.endColor = neonColor;
        
        lineRenderer.enabled = true;

        // 2. Setup Note Visuals (Sphere)
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null) targetRenderer = gameObject.AddComponent<MeshRenderer>();
        
        // Soft Bubble Texture for Mesh
        Texture2D sphereTex = new Texture2D(32, 32); // Use local var to ensure separation
        for (int ty = 0; ty < 32; ty++) {
            for (int tx = 0; tx < 32; tx++) {
                float dx = tx - 15.5f;
                float dy = ty - 15.5f;
                float dist = Mathf.Sqrt(dx*dx + dy*dy) / 16f;
                float alpha = Mathf.Clamp01(1f - dist);
                alpha = Mathf.Pow(alpha, 0.5f); 
                sphereTex.SetPixel(tx, ty, new Color(1, 1, 1, alpha));
            }
        }
        sphereTex.Apply();

        // [CRITICAL FIX] Alpha Blended Sphere (Transparent)
        Shader safeShader = Shader.Find("Mobile/Particles/Alpha Blended");
        if (safeShader == null) safeShader = Shader.Find("Particles/Alpha Blended");
        if (safeShader == null) safeShader = Shader.Find("Sprites/Default");

        Material sphereMat = new Material(safeShader);
        sphereMat.mainTexture = sphereTex;
        
        // Apply Note Color with Transparency
        Color finalColor = noteColor;
        finalColor.a = 0.5f; // Slightly more transparent
        sphereMat.color = finalColor;
        
        if (sphereMat.HasProperty("_TintColor"))
            sphereMat.SetColor("_TintColor", finalColor);

        targetRenderer.material = sphereMat;
        targetRenderer.enabled = true;
        
        transform.localScale = Vector3.zero;
    }

    // Deprecated
    public void SetMaterial(Material mat) { }

    private void Update()
    {
        if (isHit) return;

        if (Input.GetMouseButtonDown(0))
        {
            CheckInput();
        }

        float timeAlive = Time.time - spawnTime;
        float progress = timeAlive / lifeTime;

        // [FIX] BUBBLE POP-IN ANIMATION WITH RANDOM SCALE
        float popDuration = 0.4f; 
        if (timeAlive < popDuration)
        {
            float popT = timeAlive / popDuration;
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * randomScaleFactor, Mathf.SmoothStep(0, 1, popT));
        }
        else
        {
             transform.localScale = Vector3.one * randomScaleFactor;
        }

        float timeRemaining = targetTime - Time.time;

        if (timeRemaining <= 0.5f)
        {
            // Judgment Ready: Cyan Pulse
            // Keep it THICKER for GLOW (0.5f base)
            Color readyColor = new Color(0.2f, 0.9f, 1f, 0.9f);
            lineRenderer.startColor = readyColor; 
            lineRenderer.endColor = readyColor;
            
            float pulse = Mathf.PingPong(Time.time * 8, 0.05f); 
            lineRenderer.startWidth = 0.5f + pulse; 
            lineRenderer.endWidth = 0.5f + pulse;
        }
        else
        {
            // Normal: White and GLOWING (0.5f)
            lineRenderer.startColor = new Color(1f, 1f, 1f, 0.8f);
            lineRenderer.endColor = new Color(1f, 1f, 1f, 0.8f);
            lineRenderer.startWidth = 0.5f;
            lineRenderer.endWidth = 0.5f;
            lineRenderer.enabled = true;
        }

        if (progress >= 1.0f)
        {
            OnMiss();
        }
        else
        {
            float currentScale = Mathf.Lerp(startRadiusScale, 1.0f, progress);
            DrawCircle(currentScale);
        }
    }

    private void CheckInput()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform == transform) 
            {
                OnMouseDownManual(); 
            }
        }
    }

    private void DrawCircle(float scaleRadius)
    {
        float xradius = 0.5f * scaleRadius;
        float yradius = 0.5f * scaleRadius;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0));

            angle += (360f / segments);
        }
    }

    private void OnMouseDownManual() 
    {
        if (isHit) return;

        float timeRemaining = targetTime - Time.time;
        
        if (Mathf.Abs(timeRemaining) < 0.5f) 
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        isHit = true;
        
        // [FIX] Shrink-Explode Effect
        StartCoroutine(ShrinkExplodeEffect());
        
        Color effectColor = Color.white;

        if (GameManager.Instance != null)
        {
            float timeDiff = Mathf.Abs(targetTime - Time.time);
            
            if (timeDiff <= 0.15f)
            {
                GameManager.Instance.AddPerfect(transform.position);
                effectColor = new Color(0, 1, 1); // Cyan
            }
            else if (timeDiff <= 0.3f)
            {
                GameManager.Instance.AddGreat(transform.position);
                effectColor = Color.green;
            }
            else
            {
                GameManager.Instance.AddBad(transform.position);
                effectColor = new Color(1, 0.5f, 0); // Orange
            }
        }
        
        GameObject splashObj = new GameObject("SplashEffect");
        splashObj.transform.position = transform.position;
        // Basic Particle System
        ParticleSystem ps = splashObj.AddComponent<ParticleSystem>();
        var renderer = splashObj.GetComponent<ParticleSystemRenderer>();
        
        // [FIX] Assign Material to prevent Pink "Missing Material" Glitch
        renderer.material = new Material(Shader.Find("Sprites/Default")); 
        
        // [FIX] Particle Refinement: "Small Glowing Dots"
        var main = ps.main;
        main.startColor = effectColor;
        // Small glowing dots
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f); 
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f); // Tiny dots
        main.startLifetime = 0.4f; // Short life (0.4s)
        main.loop = false;
        
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0; // Burst only
        emission.SetBursts(new ParticleSystem.Burst[]{ new ParticleSystem.Burst(0f, 15) }); // 15 particles

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f; // Small source radius
        
        // Disable Velocity over Lifetime defaults if any, to ensure "Burst" feel
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local; // Fix mismatch error
        vel.radial = 1f; // Explode outward
        
        // Cleanup
        Destroy(splashObj, 1.5f);
        Destroy(gameObject);
    }

    private void OnMiss()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMiss(transform.position); 
        }
        Destroy(gameObject);
    }

    // [FIX] Shrink-Explode Visual Effect
    private System.Collections.IEnumerator ShrinkExplodeEffect()
    {
        Vector3 originalScale = transform.localScale;
        float shrinkDuration = 0.1f;
        float explodeDuration = 0.15f;
        
        // Phase 1: Shrink to 70%
        float elapsed = 0;
        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;
            transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.7f, t);
            yield return null;
        }
        
        // Phase 2: Explode to 150% then fade
        elapsed = 0;
        while (elapsed < explodeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / explodeDuration;
            transform.localScale = Vector3.Lerp(originalScale * 0.7f, originalScale * 1.5f, t);
            
            // Fade out renderer
            var rend = GetComponent<Renderer>();
            if (rend != null && rend.material != null)
            {
                Color c = rend.material.color;
                c.a = Mathf.Lerp(0.7f, 0f, t); // Start from 70% opacity
                rend.material.color = c;
            }
            yield return null;
        }
    }
}
