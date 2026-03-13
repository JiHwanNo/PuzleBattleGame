# 🎮 PuzleBattleGame (퍼즐 배틀 게임)

유니티(Unity) 엔진을 활용한 확장 가능한 퍼즐 게임 프레임워크입니다. 3매치(3-Match) 및 링크(Link) 방식 등 다양한 퍼즐 규칙을 JSON 데이터를 통해 유연하게 적용할 수 있도록 설계되었습니다.

---

## 🚀 프로젝트 개요

- **엔진 버전**: Unity 6000.0.38f1 (URP)
- **장르**: 퍼즐 배틀 (Puzzle Battle)
- **핵심 목표**: 데이터 기반의 유연한 스테이지 및 규칙 시스템 구축

## ✨ 주요 특징

### 1. 데이터 기반 설계 (Data-Driven)
- **JSON 기반 규칙**: `GameRule.json`을 통해 매칭 방식(3매치, 링크), 보드 형태(사각형, 육각형) 등을 동적으로 설정합니다.
- **스테이지 시스템**: `Stage.json` 데이터를 파싱하여 복잡한 스테이지 레이아웃과 초기 블럭 배치를 생성합니다.

### 2. 고도화된 아키텍처 (Model-View Separation)
- **Core Logic (Model)**: `PuzzleBoard`, `PuzzleCell`, `PuzzleBlock` 등 순수 C# 로직으로 퍼즐 연산을 처리하여 유닛 테스트와 재사용성을 높였습니다.
- **Visuals (View)**: `PuzzleBoardView`, `PuzzleBlockView` 등 전용 뷰 클래스를 통해 애니메이션과 연출을 관리합니다.

### 3. 리소스 및 시스템 관리
- **Addressables**: 에셋 번들 시스템을 통해 리소스 로드 속도를 최적화하고 메모리를 효율적으로 관리합니다.
- **Manager System**: `Main`, `AssetManager`, `PopupManager` 등 싱글톤 기반의 중앙 집중식 관리 시스템을 제공합니다.

## 📂 폴더 구조 및 씬(Scenes)

- **TitleScene**: 게임 시작 및 초기화
- **LoadingScene**: 데이터 로드 및 씬 전환 처리
- **LobbyScene**: 스테이지 선택 및 게임 준비 (StageInjection 연동)
- **GameScene**: 실제 퍼즐 플레이 및 전투 로직 실행
- **PopupScene**: 결과창 및 시스템 설정 등 UI 전용 씬

## 🛠 기술 스택

- **Render Pipeline**: Universal Render Pipeline (URP)
- **UI System**: TextMesh Pro (TMP)
- **Asset Management**: Addressables Grouping
- **Data Handling**: JsonUtility & Serializable Data Classes

---

## 📝 향후 개발 계획

- [ ] 블럭 매칭 및 파괴 로직 고도화
- [ ] 블럭 낙하 및 재생성 시스템 구현
- [ ] 다양한 특수 아이템(폭탄, 줄 제거 등) 추가
- [ ] 적(Enemy) AI 및 배틀 연동 시스템 구축
