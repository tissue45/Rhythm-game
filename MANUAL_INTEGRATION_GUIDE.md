# SchoolLobbyManager.cs 수동 수정 가이드

`SchoolLobbyManager.cs` 파일의 인코딩 문제로 인해 자동 수정이 불가능합니다.
아래 두 가지 변경사항을 Unity 에디터에서 직접 수정해주세요.

## 변경사항 1: 로고 위치 조정 (약 175-177번째 줄)

### 기존 코드:
```csharp
logoRect.anchoredPosition = new Vector2(0, 0);
logoRect.localScale = Vector3.one * 1.5f; // Slightly larger for center focus
```

### 변경 후:
```csharp
// [ENHANCED] Move logo to upper-center for better layout
logoRect.anchoredPosition = new Vector2(0, 150); // Upper-center position
logoRect.localScale = Vector3.one * 1.2f; // Slightly smaller for breathing room
```

**설명:** 로고를 화면 중앙에서 상단 중앙으로 이동하고 크기를 약간 줄여서 캐릭터와의 겹침을 방지합니다.

---

## 변경사항 2: ButtonStyleEnhancer 추가 (약 198-201번째 줄)

### 기존 코드:
```csharp
            // [NEW] Create Version Display        
            CreateVersionDisplay();
        }
    }
```

### 변경 후:
```csharp
            // [NEW] Create Version Display        
            CreateVersionDisplay();

            // [ENHANCED] Apply button style enhancements
            ButtonStyleEnhancer styleEnhancer = gameObject.GetComponent<ButtonStyleEnhancer>();
            if (styleEnhancer == null)
            {
                styleEnhancer = gameObject.AddComponent<ButtonStyleEnhancer>();
                Debug.Log("[SchoolLobbyManager] Added ButtonStyleEnhancer to enhance all lobby buttons");
            }
        }
    }
```

**설명:** ButtonStyleEnhancer 컴포넌트를 자동으로 추가하여 모든 로비 버튼에 향상된 스타일을 적용합니다.

---

## 또는 Unity 에디터에서 직접 추가하는 방법:

수동 코드 수정이 번거로우시면, Unity 에디터에서 다음과 같이 할 수도 있습니다:

1. Lobby 씬을 엽니다
2. Hierarchy에서 SchoolLobbyManager가 붙어있는 GameObject를 찾습니다
3. Inspector에서 `Add Component` 버튼 클릭
4. "ButtonStyleEnhancer" 검색 후 추가

이렇게 하면 버튼 스타일 향상이 자동으로 적용됩니다!

---

## 완료 후 확인사항:

- Unity 에디터에서 Lobby 씬을 열고 Play 모드로 진입
- 버튼에 마우스를 올렸을 때 부드러운 확대 애니메이션 확인
- 버튼 텍스트에 검은색 아웃라인이 있는지 확인
- 로고가 상단 중앙에 위치하는지 확인
