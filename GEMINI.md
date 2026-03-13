# 🤖 GEMINI.md - PuzleBattleGame 개발 가이드라인

이 파일은 Gemini CLI가 이 프로젝트에서 코드를 생성, 수정, 분석할 때 반드시 준수해야 하는 핵심 원칙을 담고 있습니다.

---

## 📝 1. 언어 및 주석 규칙 (Language & Comments)
- **주석**: 모든 코드의 설명 및 주석은 **한글**로 작성합니다.
- **인코딩**: 파일 저장 시 반드시 **UTF-8 (BOM 포함)** 형식을 유지하여 한글 깨짐을 방지합니다.
- **상세 주석**: 모든 **메서드(Method)** 및 **매개변수(Parameter)**에는 해당 기능이 어떤 용도로 쓰이는지, 매개변수가 무엇을 의미하는지에 대한 상세 주석을 반드시 작성합니다. (XML 주석 형식 `///` 권장)
- **설명**: 복잡한 로직이나 데이터 구조를 수정할 때는 의도를 명확히 설명하는 주석을 추가합니다.

## 🏗️ 2. 아키텍처 원칙 (Architecture)
- **Model-View 분리**: 코어 로직(Model)과 시각적 연출(View)을 엄격히 분리합니다.
- **엔진 독립성**: Model 클래스(예: `PuzzleBoard`, `PuzzleCell` 등)는 `UnityEngine` 종속성을 최소화하여 순수 C# 논리로 작성합니다.
- **데이터 기반**: 스테이지 구성 및 게임 규칙은 직접 코딩하지 않고 JSON 데이터 주입(`Injection`) 방식을 통해 처리합니다.

## 📛 3. 명명 규칙 (Naming Convention)
- **클래스/메서드/공개 필드**: `PascalCase` (예: `PuzzleGameController`, `Initialize`)
- **비공개 필드(Private)**: `_camelCase` (예: `_board`, `_gameSpec`)
- **변수/매개변수**: `camelCase` (예: `stageData`, `rulePath`)

## 📦 4. 에셋 및 리소스 관리
- **Addressables 기반**: 모든 게임 리소스(프리팹, 사운드, JSON 등)는 Addressables 시스템을 통해 관리됩니다.
- **AssetManager 활용**: 에셋을 로드할 때는 반드시 `AssetManager.cs`에 구현된 `LoadAsset<T>`, `LoadAssetAsync<T>`, `LoadGameObject` 등의 메서드를 통해 에셋을 확보합니다. 직접적인 `Addressables.LoadAssetAsync` 호출보다는 기존 매니저의 래핑된 기능을 우선 사용합니다.

## 🛠️ 5. 기타 준수 사항
- **확장성**: 새로운 퍼즐 규칙(예: 육각형 매치)이 추가될 수 있음을 고려하여 인터페이스나 데이터 구조를 설계합니다.
- **안정성**: 씬 이동이나 데이터 로드 실패 시 에러 로그를 명확히 남겨 디버깅이 용이하도록 합니다.
