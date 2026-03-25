<gemini_mandate>
# 🤖 GEMINI.md - PuzleBattleGame 개발 가이드라인

이 파일은 Gemini CLI가 이 프로젝트에서 코드를 생성, 수정, 분석할 때 반드시 준수해야 하는 핵심 원칙을 담고 있습니다. 컨텍스트 절약을 위해 상세 정보는 다른 문서를 참조하십시오.

---

<core_architecture>
## 🏗️ 1. 핵심 아키텍처 원칙 (Core Architecture)
- **다양한 퍼즐 지원**: 3매치, 링크, 육각형 등 여러 형태의 퍼즐 게임을 유연하게 수용할 수 있도록 `IPuzzleBoard` 인터페이스 및 능력(Capability) 기반의 블럭 아키텍처를 사용합니다.
- **데이터 기반 생성 (GameSpec)**: 스테이지 구성(Stage)과 게임 규칙(Rule)은 직접 코딩하지 않고 JSON 데이터를 로드하여 `GameSpec` 객체로 병합한 뒤, 이를 Model에 주입(`Injection`)하는 방식으로 게임을 생성합니다.
- **엄격한 MVC 분리**: 
  - `Model` (PuzzleBoard, PuzzleBlock 등): 유니티 엔진(`UnityEngine`)에 대한 종속성 없이 순수 C# 논리로만 작성됩니다.
  - `View` (PuzzleBoardView 등): Model의 상태 변화(`BoardViewAction`)를 전달받아 시각적 연출과 애니메이션만을 담당합니다.
  - `Controller` (PuzzleGameController): 유저의 입력을 감지하고 큐에 담아 Model에 전달하며, 전체적인 게임 루프를 제어합니다.
- **결정론적 리플레이(Replay) 시스템**:
  - 완벽한 리플레이 기능을 지원하기 위해 모든 유저의 **조작(Input)은 발생한 프레임(Frame)과 함께 큐(Queue)에 저장**되어 순차적으로 처리됩니다.
  - **공유 난수(Random) 클래스**: 난수 생성은 유니티 기본 랜덤이 아닌 단일 난수 클래스를 공유하여 사용합니다. 시드(Seed)와 프레임을 일정하게 유지함으로써 동일한 입력에 대해 항상 동일한 결과(결정론적 작동)를 보장합니다.
  - **프레임 관리 및 로직 분리**:
    - **논리 프레임(`_frameCount`)**: 환경에 구애받지 않는 일정한 시간축을 확보하기 위해 반드시 `FixedUpdate`에서만 전진시킵니다.
    - **게임 로직(`_board.Update`)**: 부드러운 연출과 입력을 위해 `Update`에서 실행합니다. 모든 시각적 액션은 발생 시점의 `_frameCount`를 참조하여 기록됩니다.
</core_architecture>

<resource_management>
## 📦 2. 에셋 및 리소스 관리
- **Addressables 기반**: 모든 게임 리소스(프리팹, 사운드, JSON 등)는 Addressables 시스템을 통해 관리됩니다.
- **AssetManager 활용**: 에셋을 로드할 때는 반드시 `AssetManager.cs`에 구현된 `LoadAsset<T>`, `LoadAssetAsync<T>`, `LoadGameObject` 등의 메서드를 통해 에셋을 확보합니다. 직접적인 `Addressables.LoadAssetAsync` 호출보다는 기존 매니저의 래핑된 기능을 우선 사용합니다.
</resource_management>

<strict_rules>
## 🛠️ 3. 기타 준수 사항
- **자동 커밋 금지**: Gemini CLI는 어떠한 경우에도 사용자의 명시적인 요청 없이 스스로 커밋(`git commit`)을 수행하지 않습니다. 모든 변경사항은 사용자가 검토한 후 직접 커밋하거나, 커밋 요청이 있을 때만 수행합니다.
- **확장성**: 새로운 퍼즐 규칙(예: 육각형 매치)이 추가될 수 있음을 고려하여 인터페이스나 데이터 구조를 설계합니다.
- **안정성**: 씬 이동이나 데이터 로드 실패 시 에러 로그를 명확히 남겨 디버깅이 용이하도록 합니다.
</strict_rules>

<external_references>
## 🔗 4. 참조 문서 (External References)
- **코딩 규칙 및 명명 규칙**: `CONVENTIONS.md` 문서를 **엄격히 준수**하십시오.
- **아키텍처 맵핑 및 탐색**: 코드를 찾거나 버그를 분석할 때는 **반드시 `MAP.md`를 먼저 확인**하여 해당 로직의 정확한 위치를 파악하십시오.
- **프로젝트 개요 및 계획**: `README.md` 문서를 참고하십시오.
- **지금까지의 개발 히스토리 및 변경점**: 컨텍스트 파악, 버그 추적, 또는 기존 작업 내용을 이어가야 할 경우 **반드시 `CHANGELOG.md`**를 열어 확인하십시오.
</external_references>

</gemini_mandate>
