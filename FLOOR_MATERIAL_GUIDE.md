# 바닥 반사 개선 가이드

## 문제
바닥의 Specular 반사가 너무 과하고 노이즈가 많아서 비현실적으로 보입니다.

## 해결 방법 (Unity 에디터에서 수동 조정)

### 1. 바닥 Material 찾기
1. Hierarchy에서 바닥 오브젝트 선택 (예: "Floor", "Ground", "Classroom" 등)
2. Inspector에서 Mesh Renderer → Materials 확인
3. Material을 더블클릭하여 열기

### 2. Material 속성 조정

#### Smoothness (광택도)
- **현재 값**: 0.9 (너무 반짝임)
- **권장 값**: **0.3 ~ 0.4** (은은한 광택)
- **위치**: Material Inspector → Smoothness 슬라이더

#### Metallic (금속성)
- **현재 값**: 확인 필요
- **권장 값**: **0** (나무는 금속이 아님)
- **위치**: Material Inspector → Metallic 슬라이더

#### Normal Map (노멀 맵 강도)
- **현재 값**: 확인 필요
- **권장 값**: **0.5 ~ 0.7** (너무 강하면 노이즈 발생)
- **위치**: Material Inspector → Normal Map → Scale

### 3. 추가 개선 (선택사항)

#### Reflection Probes
- Hierarchy → Create → Light → Reflection Probe
- 바닥 위에 배치
- Resolution: 128 (성능 고려)
- Type: Baked

#### Lightmap Settings
- Window → Rendering → Lighting
- Lightmap Resolution: 20 ~ 40
- Ambient Occlusion: 체크

## 예상 결과

**Before:**
- 거울처럼 반짝이는 바닥
- 노이즈가 많음
- 비현실적

**After:**
- 은은한 광택의 나무 바닥
- 깨끗한 반사
- 자연스러운 느낌

## 스크립트로 자동 조정 (실험적)

아래 스크립트를 Manager에 추가하면 자동으로 바닥 Material을 찾아서 조정합니다:

```csharp
// FloorMaterialFixer.cs 참고
```

---

**참고:** Material 변경 후 반드시 **Ctrl+S**로 저장하세요!
