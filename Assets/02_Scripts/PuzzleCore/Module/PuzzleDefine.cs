using System;
using System.Collections.Generic;

namespace Puzzle.Core
{
    /// <summary>
    /// 퍼즐 보드 내의 2차원 좌표를 나타내는 구조체입니다.
    /// </summary>
    public struct GridPos : IEquatable<GridPos>
    {
        /// <summary> 가로 좌표 </summary>
        public int X { get; }
        /// <summary> 세로 좌표 </summary>
        public int Y { get; }

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        // --- 방향 상수 정의 ---
        public static readonly GridPos Up = new GridPos(0, 1);
        public static readonly GridPos Down = new GridPos(0, -1);
        public static readonly GridPos Left = new GridPos(-1, 0);
        public static readonly GridPos Right = new GridPos(1, 0);

        public static readonly GridPos UpRight = new GridPos(1, 1);
        public static readonly GridPos UpLeft = new GridPos(-1, 1);
        public static readonly GridPos DownRight = new GridPos(1, -1);
        public static readonly GridPos DownLeft = new GridPos(-1, -1);

        // --- 연산자 오버로딩 ---
        public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.X + b.X, a.Y + b.Y);
        public static GridPos operator -(GridPos a, GridPos b) => new GridPos(a.X - b.X, a.Y - b.Y);
        public static bool operator ==(GridPos a, GridPos b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(GridPos a, GridPos b) => !(a == b);

        public bool Equals(GridPos other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }

    /// <summary>
    /// 규칙 JSON 파일을 파싱하기 위한 컨테이너 클래스입니다.
    /// </summary>
    [Serializable]
    public class GameRuleContainer
    {
        public RuleData rule;
        public List<BlockData> blocks;
    }

    /// <summary>
    /// 개별 게임 규칙(매치 방식 등)을 정의하는 데이터 구조체입니다.
    /// </summary>
    [Serializable]
    public struct RuleData
    {
        /// <summary> 규칙 고유 아이디 </summary>
        public string ruleId;
        /// <summary> 퍼즐 매칭 방식 </summary>
        public PuzzleType puzzleType;
        /// <summary> 보드 타일 모양 </summary>
        public BoardShape boardShape;
        /// <summary> 스테이지 클리어 목표 목록 </summary>
        public List<ObjectiveData> objectives;
    }

    /// <summary>
    /// 스테이지 클리어를 위해 달성해야 하는 개별 목표 데이터입니다.
    /// </summary>
    [Serializable]
    public struct ObjectiveData
    {
        /// <summary> 목표 종류 (점수, 블럭 수집 등) </summary>
        public ObjectiveType type;
        /// <summary> 목표 대상 (특정 블럭 ID 등, 필요 시 사용) </summary>
        public string targetId;
        /// <summary> 달성해야 하는 목표 수치 </summary>
        public int count;
    }

    /// <summary>
    /// 게임 클리어 목표의 종류를 정의합니다.
    /// </summary>
    public enum ObjectiveType
    {
        Score = 0,                  // 특정 점수 도달
        CollectBlock = 1,           // 특정 ID의 블럭 수집(파괴)
        ClearCell = 2,              // 특정 셀(바닥 등) 모두 제거
    }

    /// <summary>
    /// 블럭의 고유 속성 데이터를 담는 클래스입니다.
    /// </summary>
    [Serializable]
    public class BlockData
    {
        /// <summary> 블럭 고유 아이디 </summary>
        public string blockId;
        /// <summary> 조작 방식 </summary>
        public InputType inputType;
        /// <summary> 파괴 조건 </summary>
        public DestroyType destroyType;
        /// <summary> 내구도/생명력 </summary>
        public int life;
    }

    // ==========================================================
    // 열거형(Enum) 정의
    // ==========================================================

    /// <summary> 퍼즐의 핵심 매칭 로직 타입 </summary>
    public enum PuzzleType
    {
        None = 0,
        ThreeMatch = 1,             // 3매치 방식
        Link = 2,                   // 선 긋기 방식
    }

    /// <summary> 보드 타일의 기하학적 모양 </summary>
    public enum BoardShape
    {
        None = 0,
        Quadrangle = 1,             // 사각형
        Hexagon = 2,                // 육각형
    }

    /// <summary> 게임 오버 조건 </summary>
    public enum GameOverCondition
    {
        None = 0,
        TurnLimit = 1,              // 남은 턴수 제한
        TimeLimit = 2,              // 제한 시간 종료
    }

    /// <summary> 스테이지 클리어 조건 </summary>
    public enum ClearCondition
    {
        None = 0,
        GetTargetBlocks = 1,        // 특정 목표 블럭 수집
        ScoreTarget = 2,            // 목표 점수 도달
    }

    /// <summary> 블럭의 카테고리 분류 </summary>
    public enum BlockType
    {
        None = 0,                   // 빈 공간
        Normal = 100,               // 일반 블럭
        Item = 110,                 // 아이템 블럭
        Target = 200,               // 목표 지점/특수 대상
    }

    /// <summary> 셀의 속성 및 상태 </summary>
    public enum CellType
    {
        Close = 0,                  // 막힌 구역
        Normal = 1,                 // 일반 바닥
        Lock = 2,                   // 로직상 잠긴 상태
        Generator = 3,              // 블럭 생성기 (매 턴/필요 시 블럭 생성)
    }

    /// <summary> 
    /// 유저의 조작 방식 (다중 선택 가능)
    /// [Flags] 특성과 비트 시프트(<<)를 사용하여 하나의 블럭이 여러 조작법을 동시에 가질 수 있게 합니다.
    /// 예: Swap(0001) | Touch(0100) = 0101 (5). 이렇게 합쳐진 값은 HasFlag()로 각각 판별할 수 있습니다.
    /// </summary>
    [Flags]
    public enum InputType
    {
        None = 0,                   // 조작 불가
        Swap = 1 << 0,              // 위치 바꾸기 (1)
        Link = 1 << 1,              // 연결하기 (2)
        Touch = 1 << 2,             // 터치(클릭)하기 (4)
    }

    /// <summary> 블럭 파괴의 원인 및 방식 </summary>
    public enum DestroyType
    {
        None = 0,
        Two_Match = 1,              // 2개 매치 파괴
        Three_Match = 2,            // 3개 매치 파괴
        Splash = 50,                // 주변 폭발 여파
        Bomb = 51                   // 폭탄 직접 파괴
    }

    /// <summary> 방향 정의 </summary>
    public enum Direction
    {
        None = 0, Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight
    }

    /// <summary> 보드의 논리적 처리 상태 </summary>
    public enum BoardState
    {
        Waiting = 0,                // 입력 대기 (유저 조작 가능)
        Matching = 1,               // 매칭 판정 및 파괴 처리 중
        Falling = 2,                // 빈 공간으로 블럭 낙하 중
        Filling = 3,                // 생성기에서 새 블럭 보충 중
        Finish = 4,                 // 스테이지 종료/클리어 처리
    }

    /// <summary> 시각적 연출의 종류 </summary>
    public enum ViewType
    {
        None = 0,
        Destroy, // 파괴
        Create,  // 생성
        Move,    // 이동
        Land,    // 착지
    }
}
