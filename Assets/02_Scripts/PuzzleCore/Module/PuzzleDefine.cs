using System;
using System.Collections.Generic;

namespace Puzzle.Core
{
    // 1. 위치를 구성하는 구조체
    public struct GridPos : IEquatable<GridPos>
    {
        public int X { get; }
        public int Y { get; }

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static readonly GridPos Up = new GridPos(0, 1);
        public static readonly GridPos Down = new GridPos(0, -1);
        public static readonly GridPos Left = new GridPos(-1, 0);
        public static readonly GridPos Right = new GridPos(1, 0);

        public static readonly GridPos UpRight = new GridPos(1, 1);
        public static readonly GridPos UpLeft = new GridPos(-1, 1);
        public static readonly GridPos DownRight = new GridPos(1, -1);
        public static readonly GridPos DownLeft = new GridPos(-1, -1);

        public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.X + b.X, a.Y + b.Y);
        public static GridPos operator -(GridPos a, GridPos b) => new GridPos(a.X - b.X, a.Y - b.Y);
        public static bool operator ==(GridPos a, GridPos b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(GridPos a, GridPos b) => !(a == b);

        public bool Equals(GridPos other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }

    // ==========================================================
    // [수정됨] JSON 데이터를 파싱하기 위한 데이터 구조체(struct)들
    // ==========================================================

    [Serializable]
    public struct GameSpec
    {
        public RuleData gameMode;                   // 어떤 게임 기능 데이터
        public List<BlockData> blocks;              // 어떤 블럭 기능 데이터
    }

    [Serializable]
    public struct RuleData
    {
        public PuzzleType puzzleType;
        public BoardShape boardShape;
    }

    [Serializable]
    public struct BlockData
    {
        public string blockId;
        public InputType inputType;
        public DestroyType destroyType;
        public int life;
    }

    // ==========================================================
    // 열거형(Enum) 정의
    // ==========================================================

    public enum PuzzleType
    {
        None = 0,
        ThreeMatch = 1,             // 3매치 퍼즐
        Link = 2,                   // 선 긋기 퍼즐
    }

    public enum BoardShape
    {
        None = 0,
        Quadrangle = 1,             // 사각형 보드
        Hexagon = 2,                // 육각형 보드
    }

    public enum GameOverCondition
    {
        None = 0,
        TurnLimit = 1,              // 턴(이동 횟수) 제한
        TimeLimit = 2,              // 시간 제한
    }

    public enum ClearCondition
    {
        None = 0,
        GetTargetBlocks = 1,        // 특정 블럭 획득 (예: 목표 지점에 도달)   
        ScoreTarget = 2,            // 목표 점수 달성
    }

    public enum BlockType
    {
        None = 0,                   // 빈 공간 (블럭 없음)
        Normal = 100,               // 일반 퍼즐 블럭
        Item = 110,                 // 특수 능력이 있는 아이템 블럭
        Target = 200,               // 슬라이딩 퍼즐 등에서 도달해야 하는 목표 지점
    }

    public enum ItemType
    {
        None = 0,
        Bomb = 10,                  // 주변 블럭을 제거하는 폭탄 아이템
        RowClear = 50,              // 가로 한 줄을 제거하는 아이템
        ColumnClear = 51,           // 세로 한 줄을 제거하는 아이템
    }

    public enum CellType
    {
        Normal = 0,
        Close = 1,                  // 닫힌 셀 (블럭이 들어갈 수 없는 장애물)
        Lock = 2,                   // 잠긴 셀 (연출 및 조작으로 인해 잠시 연산 처리로 잠긴 셀)
    }

    public enum InputType
    {
        None = 0,                   // 조작 불가 (장애물 등)
        Swap = 1,                   // 두 블럭의 위치를 바꾸는 입력
        Link = 2,                   // 두 블럭을 연결하는 입력
    }

    public enum DestroyType
    {
        None = 0,

        // 매치에 의한 파괴
        Two_Match = 1,      // 2개 이상의 같은 블럭을 매치하여 파괴 (블라스트 류)
        Three_Match = 2,    // 3개 이상의 같은 블럭을 매치하여 파괴 (3매치 류)

        // 외부 영향에 의한 파괴
        Splash = 50,        // 다른 블럭이 터질 때 근접해 있어서 연쇄/여파로 파괴
        Bomb = 51           // 폭탄 등의 아이템이 터질 때 폭발 영역 안에 있어서 파괴
    }

    public enum Direction
    {
        None = 0,
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }
}