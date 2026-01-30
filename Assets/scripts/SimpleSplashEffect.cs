using UnityEngine;

public class SimpleSplashEffect : MonoBehaviour
{
    private void Start()
    {
        // 파티클 시스템 컴포넌트 추가
        ParticleSystem ps = gameObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer psr = gameObject.GetComponent<ParticleSystemRenderer>();

        // [메인 설정]
        var main = ps.main;
        main.startLifetime = 0.5f; // 수명 짧게
        main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f); // 속도 다채롭게
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f); // 크기 랜덤
        main.startColor = new Color(0.2f, 0.5f, 1f, 0.8f); // 시원한 파란색
        main.gravityModifier = 1f; // 중력 적용 (물처럼 떨어지게)
        main.loop = false; // 한 번만 재생
        main.playOnAwake = true;

        // [이모션 - 발사]
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0; // 지속 발사 X
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) }); // 한 방에 20개 팡!

        // [모양 - 구형으로 퍼짐]
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        // [렌더러 - 기본 재질]
        psr.material = new Material(Shader.Find("Sprites/Default")); // 기본 스프라이트 쉐이더 사용

        // 1초 뒤에 객체 삭제 (이펙트 끝나면 청소)
        Destroy(gameObject, 1.0f);
    }
}
