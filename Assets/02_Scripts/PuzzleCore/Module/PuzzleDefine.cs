using System;

namespace Puzzle.Core
{
    // 1. 위치를 구성하는 구조체 (유니티 종속성 제거 및 정수형 격자 좌표용)
    public struct GridPos : IEquatable<GridPos>
    {
        public int X { get; }
        public int Y { get; }

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        // 2. 8방향 관련된 Vector2 (읽기 전용 static)
        // 참고: 배열의 형태(Y가 위로 가는지 아래로 가는지)에 따라 부호는 기획에 맞게 수정하세요.
        public static readonly GridPos Up = new GridPos(0, 1);
        public static readonly GridPos Down = new GridPos(0, -1);
        public static readonly GridPos Left = new GridPos(-1, 0);
        public static readonly GridPos Right = new GridPos(1, 0);

        public static readonly GridPos UpRight = new GridPos(1, 1);
        public static readonly GridPos UpLeft = new GridPos(-1, 1);
        public static readonly GridPos DownRight = new GridPos(1, -1);
        public static readonly GridPos DownLeft = new GridPos(-1, -1);

        // 편의를 위한 연산자 오버로딩 (위치 계산 시 유용)
        public static GridPos operator +(GridPos a, GridPos b) => new GridPos(a.X + b.X, a.Y + b.Y);
        public static GridPos operator -(GridPos a, GridPos b) => new GridPos(a.X - b.X, a.Y - b.Y);
        public static bool operator ==(GridPos a, GridPos b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(GridPos a, GridPos b) => !(a == b);

        public bool Equals(GridPos other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }

    // 값 타입이므로 데이터 전달 및 복사에 유리하며, 외부(View)에서 세팅해서 넘겨주기 좋습니다.
    public struct GameSpec
    {
        public ushort[,] boards;            //0.5칸씩 생성한다. 

        public int timeSec;                 //0이면 시간제는 아니다. 

        public int move;                    //0이면 턴제는 아니다.

        public string[] blockTypes;         //BlockType-BlockId

        public ushort inputType;            //InputType Enum의 Index

        public ushort puzzleMode;           //PuzzleType Enum의 Index

    }

 
    public enum PuzzleType
    {
        None = 0,
        ThreeMatch = 1,             // 3매치 퍼즐
    }

    // 블럭 타입 정의
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
        ColumnClear,                // 세로 한 줄을 제거하는 아이템
    }

    public enum CellType
    {
        Normal = 0,
        Close,                      // 닫힌 셀 (블럭이 들어갈 수 없는 장애물)
        Lock,                       // 잠긴 셀 (연출 및 조작으로 인해 잠시 연산 처리로 잠긴 셀)
    }

    public enum InputType
    {
        None = 0,
        Swap,                       // 두 블럭의 위치를 바꾸는 입력
        Link,                       // 두 블럭을 연결하는 입력 (예: 선으로 이어서 제거하는 퍼즐)
    }

    // (보너스) 로직 처리를 위해 방향을 명시할 때 쓰기 좋은 Enum
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

    public enum ClearCondition
    {
        None = 0,

        GetTargetBlocks,           // 특정 블럭 획득 (예: 목표 지점에 도달)   

    }

}