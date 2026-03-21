# 📜 PuzleBattleGame 코드 컨벤션 (Code Conventions)

이 문서는 프로젝트의 가독성, 유지보수성, 그리고 협업 효율을 높이기 위한 코딩 표준을 정의합니다. 모든 개발자 및 AI(Gemini)는 이 규칙을 준수해야 합니다.

---

## 1. 파일 및 인코딩 (Files & Encoding)
- **인코딩**: 모든 소스 파일(.cs, .json 등)은 반드시 **UTF-8 (BOM 포함)** 형식을 사용합니다.
  - *이유: 유니티 에디터 및 Visual Studio에서 한글 주석이 깨지는 현상을 방지하기 위함입니다.*
- **언어**: 코드 내의 모든 **설명 및 주석은 한글**로 작성합니다.
- **줄 바꿈**: Windows 방식(CRLF)을 권장합니다.

## 2. 명명 규칙 (Naming Conventions)
### 클래스 및 인터페이스
- **클래스/구조체/열거형**: `PascalCase` (예: `PuzzleGameController`, `BoardState`)
- **인터페이스**: `I` 접두사와 함께 `PascalCase` (예: `IPuzzleBoard`, `ITouchable`)
- **파일명**: 클래스 이름과 파일 이름은 반드시 일치해야 합니다.

### 메서드 및 프로퍼티
- **공개 메서드 및 프로퍼티**: `PascalCase` (예: `Initialize()`, `CurrentState { get; }`)
- **비공개 메서드**: `PascalCase` (예: `ProcessMatching()`)

### 변수 및 필드
- **비공개 필드 (Private Field)**: `_camelCase` (언더바 접두사 사용, 예: `_board`, `_frameCount`)
- **지역 변수 및 매개변수**: `camelCase` (예: `targetPos`, `deltaFrame`)
- **상수 (Const)**: `PascalCase` 또는 `SCREAMING_SNAKE_CASE` (예: `MaxBlockCount` 또는 `MAX_BLOCK_COUNT`)

## 3. 주석 (Commenting)
- **XML 주석 (///)**: 모든 클래스, 공개 메서드, 공개 필드에는 반드시 XML 주석을 작성합니다.
  - 메서드의 경우 `<summary>`, `<param>`, `<returns>`를 상세히 기술합니다.
- **인라인 주석 (//)**: 복잡한 로직 내부에는 코드의 의도를 설명하는 한글 주석을 추가합니다.
- **TODO**: 미구현 기능이나 수정이 필요한 부분은 `// TODO:` 키워드를 사용하여 표시합니다.

## 4. 아키텍처 원칙 (Architecture)
### MVC 분리 (Strict MVC)
- **Model (Logic)**: 
  - `UnityEngine` 네임스페이스를 참조하지 않습니다. (순수 C# 논리)
  - `Debug.Log` 대신 인터페이스를 통한 로그 전달(`OnLog`)을 사용합니다.
- **View (Visual)**:
  - 모델의 상태 변화(`BoardViewAction`)를 구독하여 시각적 연출(애니메이션, 이펙트)만 담당합니다.
- **Controller (Input/Loop)**:
  - 유저 입력을 모델에 전달하고, 전체적인 게임 루프(Update/FixedUpdate)를 관리합니다.

### 리소스 관리
- **Addressables**: 모든 에셋(프리팹, 스프라이트 등)은 주소 기반으로 로드합니다.
- **AssetManager**: 에셋 로드 시 직접 호출 대신 `AssetManager`의 래핑된 메서드를 사용합니다.
- **PoolManager**: 빈번하게 생성/파괴되는 오브젝트(블럭, 이펙트)는 반드시 풀링을 거쳐야 합니다.

## 5. 유니티 특정 규칙 (Unity Specifics)
- **Z-축 관리**: 2D 게임의 레이어 순서를 위해 Z-축 또는 `Sorting Layer`를 명확히 지정합니다.
  - (예: 배경 0, 셀 0, 블럭 -0.1)
- **컴포넌트 참조**: `GetComponent` 호출은 가급적 `Awake`나 `Start`에서 수행하고 캐싱합니다.
- **Update vs FixedUpdate**:
  - 시각적 연출 및 단순 입력 감지는 `Update`에서 처리합니다.
  - 결정론적 로직(보드 업데이트 등)은 `FixedUpdate`에서 수행합니다.

## 6. 기타 (Miscellaneous)
- **Magic Number 금지**: 의미를 알 수 없는 숫자는 상수로 정의하여 사용합니다.
- **린트 (Linting)**: 사용되지 않는 변수나 네임스페이스(`using`)는 제거합니다.

## 7. 제어문 (Control Structures)
- **중괄호 (`{}`) 필수**: `if`, `else`, `for`, `while` 등의 제어문에는 단 한 줄의 실행문이 있더라도 반드시 중괄호를 사용합니다.
- **줄 바꿈**: 중괄호는 새로운 줄에서 시작하는 것을 권장합니다. (Allman 스타일)
  - **Good:**
    ```csharp
    if (condition)
    {
        DoSomething();
    }
    ```
  - **Bad:**
    ```csharp
    if (condition) DoSomething();
    ```

---
*마지막 업데이트: 2026-03-21*
