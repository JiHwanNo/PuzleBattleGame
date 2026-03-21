using System;
using System.Collections.Generic;

namespace Puzzle.Core
{
    #region Enums
    /// <summary>
    /// 퍼즐 게임의 종류를 정의합니다.
    /// </summary>
    public enum PuzzleType
    {
        /// <summary> 일반적인 3매치 게임 </summary>
        ThreeMatch,
        /// <summary> 터치하여 연결하는 링크 게임 </summary>
        Link,
        /// <summary> 육각형 타일 기반 매치 게임 </summary>
        Hexa
    }

    /// <summary>
    /// 퍼즐 보드의 현재 상태를 정의합니다.
    /// </summary>
    public enum BoardState
    {
        /// <summary> 유저 입력을 기다리는 상태 </summary>
        Waiting,
        /// <summary> 매칭된 블럭을 체크하고 제거하는 상태 </summary>
        Matching,
        /// <summary> 블럭들이 아래로 떨어지는 상태 </summary>
        Falling,
        /// <summary> 비어있는 공간에 새로운 블럭을 생성하는 상태 </summary>
        Filling,
        /// <summary> 게임이 종료된 상태 (목표 달성) </summary>
        Finish
    }

    /// <summary>
    /// 개별 셀(타일)의 속성을 정의합니다.
    /// </summary>
    public enum CellType
    {
        /// <summary> 일반적인 타일 </summary>
        Normal,
        /// <summary> 블럭이 생성되는 타일 </summary>
        Generator,
        /// <summary> 블럭이 배치될 수 없는 장애물 타일 </summary>
        Obstacle,
        /// <summary> 타일 자체가 없는 빈 공간 </summary>
        Empty
    }

    /// <summary>
    /// 화면(View)에 요청할 연출 종류를 정의합니다.
    /// </summary>
    public enum ViewType
    {
        /// <summary> 블럭 생성 </summary>
        Create,
        /// <summary> 블럭 파괴 </summary>
        Destroy,
        /// <summary> 블럭 이동 </summary>
        Move,
        /// <summary> 블럭 속성 변경 </summary>
        Update
    }

    /// <summary>
    /// 블럭의 조작 능력을 비트 플래그로 정의합니다.
    /// </summary>
    [Flags]
    public enum InputType
    {
        /// <summary> 조작 불가능 </summary>
        None = 0,
        /// <summary> 터치/클릭 가능 </summary>
        Touch = 1 << 0,
        /// <summary> 스왑(인접 교체) 가능 </summary>
        Swap = 1 << 1,
        /// <summary> 드래그 연결 가능 </summary>
        Link = 1 << 2
    }
    #endregion

    #region Structures
    /// <summary>
    /// 보드 상의 2차원 그리드 좌표를 나타내는 구조체입니다.
    /// </summary>
    public struct GridPos : IEquatable<GridPos>
    {
        public int X;
        public int Y;

        public GridPos(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPos other && Equals(other);
        }

        public bool Equals(GridPos other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(GridPos left, GridPos right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GridPos left, GridPos right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }

    /// <summary>
    /// 특정 프레임에 발생한 유저 입력을 기록하는 구조체입니다.
    /// </summary>
    public struct InputRecord
    {
        public ulong Frame;
        public GridPos Position;

        public InputRecord(ulong frame, GridPos pos)
        {
            Frame = frame;
            Position = pos;
        }
    }
    #endregion
}
