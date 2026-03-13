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
    /// 전체 게임의 설정 데이터(규칙, 스테이지, 블럭 등)를 담는 통합 사양 클래스입니다.
    /// </summary>
    [Serializable]
    public class GameSpec
    {
        /// <summary> 스테이지 레이아웃 및 초기 배치 데이터 </summary>
        public StageData stageData;
        /// <summary> 게임 규칙 리스트 </summary>
        public List<RuleData> rules;
        /// <summary> 등장 가능한 블럭 정보 리스트 </summary>
        public List<BlockData> blocks;

        /// <summary> 특정 아이디에 해당하는 규칙을 찾습니다. </summary>
        public RuleData GetRule(string ruleId) => rules?.Find(r => r.ruleId == ruleId) ?? default;
        /// <summary> 특정 아이디에 해당하는 블럭 정보를 찾습니다. </summary>
        public BlockData GetBlock(string blockId) => blocks?.Find(b => b.blockId == blockId);
    }

    /// <summary>
    /// 스테이지 전체 구성을 담는 데이터 클래스입니다.
    /// </summary>
    [Serializable]
    public class StageData
    {
        /// <summary> 스테이지 고유 고유 아이디 </summary>
        public int stage_id;
        /// <summary> 보드 가로 크기 </summary>
        public int stage_width;
        /// <summary> 보드 세로 크기 </summary>
        public int stage_height;
        /// <summary> 셀별 세부 데이터 리스트 </summary>
        public List<CellData> cells;
    }

    /// <summary>
    /// 개별 셀의 초기 설정값을 담는 데이터 클래스입니다.
    /// </summary>
    [Serializable]
    public class CellData
    {
        public int x;
        public int y;
        public string block_id;
        public int panel_id;
        public int cell_type;
    }

    /// <summary>
    /// 규칙 JSON 파일을 파싱하기 위한 컨테이너 클래스입니다.
    /// </summary>
    [Serializable]
    public class GameRuleContainer
    {
        public List<RuleData> rules;
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
        Normal = 0,                 // 일반 바닥
        Close = 1,                  // 막힌 구역
        Lock = 2,                   // 로직상 잠긴 상태
    }

    /// <summary> 유저의 조작 방식 </summary>
    public enum InputType
    {
        None = 0,                   // 조작 불가
        Swap = 1,                   // 위치 바꾸기
        Link = 2,                   // 연결하기
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
