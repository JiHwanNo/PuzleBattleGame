# 📜 PuzleBattleGame 코드 컨벤션 (Code Conventions)

이 문서는 프로젝트의 가독성, 유지보수성, 그리고 협업 효율을 높이기 위한 코딩 표준을 정의합니다. 모든 개발자 및 AI 도구는 이 규칙을 준수해야 합니다.

---

## 1. 파일 및 인코딩 (Files & Encoding)
- **인코딩**: 모든 소스 파일(.cs, .json 등)은 반드시 **UTF-8 (BOM 포함)** 형식을 사용합니다.
- **언어**: 코드 내의 모든 **설명 및 주석은 한글**로 작성합니다.
- **상세 주석**: 모든 **메서드(Method)** 및 **매개변수(Parameter)**에는 해당 기능이 어떤 용도로 쓰이는지, 매개변수가 무엇을 의미하는지에 대한 상세 주석을 반드시 작성합니다. (XML 주석 형식 `///` 권장)
- **설명**: 복잡한 로직이나 데이터 구조를 수정할 때는 의도를 명확히 설명하는 주석을 추가합니다.

## 2. 명명 규칙 (Naming Conventions)
### 클래스 및 인터페이스
- **클래스/메서드/공개 필드/구조체/열거형**: `PascalCase` (예: `PuzzleGameController`, `Initialize`)
- **인터페이스**: `I` 접두사와 함께 `PascalCase` (예: `IPuzzleBoard`)

### 변수 및 필드
- **비공개 필드 (Private Field)**: `_camelCase` (언더바 접두사 사용, 예: `_board`, `_frameCount`)
- **지역 변수 및 매개변수**: `camelCase` (예: `targetPos`, `stageData`)

## 3. 제어문 (Control Structures) - 중괄호 필수
- **Allman 스타일**: 모든 제어문(`if`, `for`, `while`, `switch` 등)은 단 한 줄의 실행문이 있더라도 반드시 **중괄호`{}`**를 사용하며, 중괄호는 새로운 줄에서 시작합니다.
  - **Good:**
    ```csharp
    if (condition)
    {
        DoSomething();
    }
    ```

## 4. 스타일 교정 원칙 (Refactoring Mandate)
- **로직 보존**: 스타일 교정 시 기존의 실행 로직, 메서드 호출(예: `SendMessage`), 조건문 결과 등을 절대 변경하거나 삭제하지 않습니다.
- **주석**: 모든 메서드와 클래스 상단에는 한글 XML 주석(`///`)을 추가하며, 기존 주석은 보존하거나 한글로 업데이트합니다.

## 5. 디버깅 및 시각화 (Debugging & Visualization)
- **뷰 상태 우선**: 씬 뷰나 GUI에 표시되는 디버그 정보는 모델 데이터뿐만 아니라, 실제 화면에 생성된 **뷰 객체(View Object)의 상태**를 우선적으로 반영하여 동기화 오류를 쉽게 파악할 수 있도록 합니다.

---
*마지막 업데이트: 2026-03-25*
