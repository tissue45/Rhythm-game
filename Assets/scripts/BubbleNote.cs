using UnityEngine;

/// <summary>
/// 투명하고 반짝이는 2D 원형 노트 - 수축하는 링 디자인
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class BubbleNote : MonoBehaviour
{
    private float spawnTime;
    private float lifeTime;
    private float targetTime; 

    private LineRenderer outerRing;      // 수축하는 외부 링
    private LineRenderer targetRing;     // 고정된 타겟 링
    private LineRenderer flashRing;      // 반짝임 효과용 추가 링
    private bool isHit = false;

    private int segments = 50; // 부드러운 원
    public float targetRadius = 0.8f;
    private float startRadius = 2.8f;

    private Camera mainCamera;
    private Color noteColor = Color.white;
    private float randomScaleFactor = 1.5f;

    // 파티클
    private ParticleSystem glowParticles;

    public void Initialize(float lifeTime)
    {
        this.lifeTime = lifeTime * 1.2f; // [조정] 빠른 수축 (2.0 → 1.2 배율)
        this.spawnTime = Time.time;
        this.targetTime = spawnTime + this.lifeTime;
        
        mainCamera = Camera.main; 
        randomScaleFactor = Random.Range(1.3f, 1.7f);

        // 밝고 선명한 네온 색상
        Color[] neonPalette = new Color[] {
            new Color(0f, 1f, 1f),      // 시안
            new Color(1f, 0f, 1f),      // 마젠타
            new Color(1f, 1f, 0f),      // 옐로우
            new Color(0f, 1f, 0.5f),    // 민트
            new Color(1f, 0.5f, 0f),    // 오렌지
            new Color(0.5f, 1f, 0f)     // 라임
        };
        noteColor = neonPalette[Random.Range(0, neonPalette.Length)];
        noteColor.a = 1f;
    }

    void Start()
    {
        CreateSimpleRings();
        CreateGlowParticles();
        
        transform.localScale = Vector3.one * randomScaleFactor;
    }

    private void CreateSimpleRings()
    {
        // 1. 타겟 링 (고정, 얇고 투명하게)
        targetRing = GetComponent<LineRenderer>();
        SetupCircle(targetRing, targetRadius, 0.1f, noteColor, 0.5f); // [투명] 알파 0.8 → 0.5

        // 2. 외부 수축 링 (두껍고 투명하게)
        GameObject outerObj = new GameObject("OuterRing");
        outerObj.transform.SetParent(transform, false);
        outerRing = outerObj.AddComponent<LineRenderer>();
        SetupCircle(outerRing, startRadius, 0.18f, noteColor, 0.6f); // [투명] 알파 1.0 → 0.6

        // 3. [NEW] 반짝임 링 (Perfect 타이밍에 강렬하게)
        GameObject flashObj = new GameObject("FlashRing");
        flashObj.transform.SetParent(transform, false);
        flashRing = flashObj.AddComponent<LineRenderer>();
        SetupCircle(flashRing, targetRadius, 0.15f, Color.white, 0f); // 처음엔 투명
    }

    private void SetupCircle(LineRenderer lr, float radius, float width, Color color, float alpha)
    {
        lr.useWorldSpace = false;
        lr.positionCount = segments + 1;
        lr.loop = true;

        // 원 그리기
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments);
            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;
            lr.SetPosition(i, new Vector3(x, y, 0));
        }

        // 머티리얼 설정
        Shader shader = Shader.Find("Particles/Additive");
        if (shader == null) shader = Shader.Find("Mobile/Particles/Additive");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = new Color(2.5f, 2.5f, 2.5f, 1f); // 매우 밝게
        lr.material = mat;

        lr.startWidth = width;
        lr.endWidth = width;
        
        Color finalColor = color;
        finalColor.a = alpha;
        lr.startColor = finalColor;
        lr.endColor = finalColor;
    }

    // [REMOVED] CreateCenterDot() method - 중심 점 완전 제거

    private void CreateGlowParticles()
    {
        GameObject particleObj = new GameObject("GlowParticles");
        particleObj.transform.SetParent(transform, false);
        glowParticles = particleObj.AddComponent<ParticleSystem>();

        var main = glowParticles.main;
        main.startColor = noteColor;
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        main.startLifetime = 1.8f;
        main.maxParticles = 10;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        var emission = glowParticles.emission;
        emission.rateOverTime = 5;

        var shape = glowParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = targetRadius;

        var renderer = glowParticles.GetComponent<ParticleSystemRenderer>();
        // [FIX] Use Sprites/Default which is ALWAYS included in WebGL builds
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Mobile/Particles/Alpha Blended"); // Fallback
        
        Material mat = new Material(shader);
        mat.name = "GlowMat";
        renderer.material = mat;
    }

    private void Update()
    {
        if (isHit) return;

        if (Input.GetMouseButtonDown(0))
        {
            CheckInput();
        }

        float timeAlive = Time.time - spawnTime;
        float progress = timeAlive / lifeTime;
        float timeRemaining = targetTime - Time.time;

        // 외부 링 수축 (startRadius → targetRadius)
        float currentRadius = Mathf.Lerp(startRadius, targetRadius, progress);
        UpdateCircleRadius(outerRing, currentRadius);

        // Perfect 타이밍 계산
        float radiusDiff = Mathf.Abs(currentRadius - targetRadius);
        
        if (timeRemaining <= 1.5f)
        {
            if (radiusDiff < 0.25f) // Perfect 범위
            {
                // [NEW] 육각형처럼 강렬한 반짝임 효과
                float intensity = 1f - (radiusDiff / 0.25f);
                
                // 타겟 링 강렬한 펄스
                float pulse = Mathf.Sin(Time.time * 15) * 0.1f * intensity;
                targetRing.startWidth = 0.1f + pulse;
                targetRing.endWidth = 0.1f + pulse;
                
                // 외부 링 펄스
                outerRing.startWidth = 0.18f + pulse * 2f;
                outerRing.endWidth = 0.18f + pulse * 2f;
                
                // [NEW] 반짝임 링 - 흰색으로 강렬하게
                float flashIntensity = Mathf.PingPong(Time.time * 20, 1f) * intensity;
                Color flashColor = Color.white;
                flashColor.a = flashIntensity * 0.8f;
                flashRing.startColor = flashColor;
                flashRing.endColor = flashColor;
                flashRing.startWidth = 0.15f + flashIntensity * 0.2f;
                flashRing.endWidth = 0.15f + flashIntensity * 0.2f;
                
                // 색상 변화 (흰색으로)
                Color readyColor = Color.Lerp(noteColor, Color.white, intensity * 0.9f);
                readyColor.a = 0.8f; // [투명] 유지
                outerRing.startColor = readyColor;
                outerRing.endColor = readyColor;
                
                Color targetColor = readyColor;
                targetColor.a = 0.6f; // [투명] 유지
                targetRing.startColor = targetColor;
                targetRing.endColor = targetColor;
            }
            else
            {
                // 일반 준비 상태
                float pulse = Mathf.Sin(Time.time * 6) * 0.04f;
                targetRing.startWidth = 0.1f + pulse;
                targetRing.endWidth = 0.1f + pulse;
                outerRing.startWidth = 0.18f + pulse;
                outerRing.endWidth = 0.18f + pulse;
                
                Color outerColor = noteColor;
                outerColor.a = 0.6f; // [투명]
                outerRing.startColor = outerColor;
                outerRing.endColor = outerColor;
                
                Color targetColor = noteColor;
                targetColor.a = 0.5f; // [투명]
                targetRing.startColor = targetColor;
                targetRing.endColor = targetColor;
                
                // 반짝임 링 숨김
                flashRing.startColor = new Color(1, 1, 1, 0);
                flashRing.endColor = new Color(1, 1, 1, 0);
            }
        }
        else
        {
            // 일반 상태
            targetRing.startWidth = 0.1f;
            targetRing.endWidth = 0.1f;
            outerRing.startWidth = 0.18f;
            outerRing.endWidth = 0.18f;
            
            Color outerColor = noteColor;
            outerColor.a = 0.6f; // [투명]
            outerRing.startColor = outerColor;
            outerRing.endColor = outerColor;
            
            Color targetColor = noteColor;
            targetColor.a = 0.5f; // [투명]
            targetRing.startColor = targetColor;
            targetRing.endColor = targetColor;
            
            // 반짝임 링 숨김
            flashRing.startColor = new Color(1, 1, 1, 0);
            flashRing.endColor = new Color(1, 1, 1, 0);
        }

        // Miss 체크
        if (progress >= 1.0f)
        {
            OnMiss();
        }
    }

    private void UpdateCircleRadius(LineRenderer lr, float radius)
    {
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * (360f / segments);
            float rad = angle * Mathf.Deg2Rad;
            float x = Mathf.Cos(rad) * radius;
            float y = Mathf.Sin(rad) * radius;
            lr.SetPosition(i, new Vector3(x, y, 0));
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

    private void OnMouseDownManual() 
    {
        if (isHit) return;

        float timeRemaining = targetTime - Time.time;
        
        if (Mathf.Abs(timeRemaining) < 1.5f) 
        {
            OnHit();
        }
    }

    private void OnHit()
    {
        isHit = true;
        
        float timeAlive = Time.time - spawnTime;
        float progress = timeAlive / lifeTime;
        float currentRadius = Mathf.Lerp(startRadius, targetRadius, progress);
        float radiusDiff = Mathf.Abs(currentRadius - targetRadius);

        if (GameManager.Instance != null)
        {
            if (radiusDiff <= 0.25f) // Perfect
            {
                GameManager.Instance.AddPerfect(transform.position);
            }
            else if (radiusDiff <= 0.6f) // Great
            {
                GameManager.Instance.AddGreat(transform.position);
            }
            else // Good
            {
                GameManager.Instance.AddBad(transform.position);
            }
        }
        
        // [FIX] 코루틴 없이 즉시 삭제
        DestroyImmediately();
    }

    private void OnMiss()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddMiss(transform.position); 
        }
        
        // [FIX] 코루틴 없이 즉시 삭제
        DestroyImmediately();
    }

    private void DestroyImmediately()
    {
        // 모든 LineRenderer 즉시 비활성화 및 제거
        if (outerRing != null)
        {
            outerRing.enabled = false;
            outerRing.startColor = Color.clear;
            outerRing.endColor = Color.clear;
        }
        
        if (targetRing != null)
        {
            targetRing.enabled = false;
            targetRing.startColor = Color.clear;
            targetRing.endColor = Color.clear;
        }
        
        if (flashRing != null)
        {
            flashRing.enabled = false;
            flashRing.startColor = Color.clear;
            flashRing.endColor = Color.clear;
        }
        
        // 파티클 즉시 정리
        if (glowParticles != null)
        {
            glowParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        // 모든 Renderer 비활성화
        Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in allRenderers)
        {
            if (r != null) r.enabled = false;
        }
        
        // 모든 LineRenderer 비활성화
        LineRenderer[] allLines = GetComponentsInChildren<LineRenderer>();
        foreach (LineRenderer lr in allLines)
        {
            if (lr != null)
            {
                lr.enabled = false;
                lr.startColor = Color.clear;
                lr.endColor = Color.clear;
            }
        }
        
        // 즉시 삭제
        Destroy(gameObject);
    }
}
