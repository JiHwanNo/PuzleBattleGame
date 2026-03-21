using System.Collections.Generic;

namespace Puzzle.Core
{
    /// <summary> 터치(클릭) 조작을 받을 수 있는 블럭 능력을 정의합니다. </summary>
    public interface ITouchableBlock
    {
        void OnTouched(IPuzzleBoard board, GridPos myPos);
    }

    /// <summary> 스왑(드래그 등) 조작을 받을 수 있는 블럭 능력을 정의합니다. </summary>
    public interface ISwappableBlock
    {
        bool OnSwapped(IPuzzleBoard board, GridPos myPos, GridPos targetPos);
    }

    /// <summary> 선 긋기(Link) 조작을 받을 수 있는 블럭 능력을 정의합니다. </summary>
    public interface ILinkableBlock
    {
        bool CanLink(IPuzzleBoard board, GridPos myPos, GridPos previousPos);
    }

    /// <summary>
    /// 셀 위에 놓여 유저가 조작하거나 매치되는 퍼즐 조각의 기본 추상 클래스입니다.
    /// 조작 방식(InputType)에 따라 이 클래스를 상속받은 구체적인 블럭 클래스들이 생성됩니다.
    /// </summary>
    public abstract class PuzzleBlock
    {
        /// <summary> 블럭의 속성 정보를 담고 있는 데이터 객체 </summary>
        protected BlockData _blockData;
        
        /// <summary>
        /// 블럭의 고유 아이디를 반환합니다.
        /// </summary>
        /// <returns>블럭 아이디 문자열</returns>
        public string GetBlockId() => _blockData.blockId;

        /// <summary>
        /// 지정된 데이터를 사용하여 새로운 블럭 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="data">블럭의 고유 설정 데이터</param>
        public PuzzleBlock(BlockData data)
        {
            _blockData = data;
        }

        /// <summary>
        /// 매 프레임마다 블럭의 상태를 업데이트합니다. (애니메이션, 타이머 등)
        /// </summary>
        internal virtual void Update()
        {
            // 하위 클래스에서 필요 시 오버라이드
        }
    }

    /// <summary>
    /// 3매치 등에서 스왑만 가능한 가장 기본적인 일반 블럭입니다.
    /// </summary>
    public class NormalBlock : PuzzleBlock, ISwappableBlock
    {
        public NormalBlock(BlockData data) : base(data) { }

        public bool OnSwapped(IPuzzleBoard board, GridPos myPos, GridPos targetPos)
        {
            // 기본적인 스왑은 항상 허용(true)하고, 보드(Board) 측에서 매치 여부를 판별하는 구조를 가정.
            // 블럭 자체의 특수 스왑 효과가 있다면 이곳에 작성합니다.
            return true;
        }
    }

    /// <summary>
    /// 터치하면 즉시 폭발하고, 스왑해도 폭발하는 형태의 폭탄 블럭 예시입니다.
    /// </summary>
    public class BombBlock : PuzzleBlock, ITouchableBlock, ISwappableBlock
    {
        public BombBlock(BlockData data) : base(data) { }

        public void OnTouched(IPuzzleBoard board, GridPos myPos)
        {
            UnityEngine.Debug.Log($"[BombBlock] 터치됨! 폭발 실행: {myPos}");
            // 보드에 폭발(파괴) 요청
            // board.ExplodeArea(myPos, radius: 1); (예시)
            board.GetCell(myPos).Block = null; 
            
            // View 액션 추가
            board.AddView(new BoardViewAction { type = ViewType.Destroy, frame = 0 });
        }

        public bool OnSwapped(IPuzzleBoard board, GridPos myPos, GridPos targetPos)
        {
            UnityEngine.Debug.Log($"[BombBlock] 스왑됨! 이동 후 폭발 실행: {myPos} -> {targetPos}");
            // 스왑 대상 위치를 중심으로 폭발 요청
            board.GetCell(targetPos).Block = null; 
            board.AddView(new BoardViewAction { type = ViewType.Destroy, frame = 0 });
            return true;
        }
    }

    /// <summary>
    /// BlockData의 InputType 및 설정 값에 따라 적절한 PuzzleBlock의 파생 객체를 생성하는 팩토리 클래스입니다.
    /// </summary>
    public static class PuzzleBlockFactory
    {
        public static PuzzleBlock Create(BlockData data)
        {
            // Flags enum이므로 특정 기능이 포함되어 있는지 확인 (HasFlag)
            bool isTouchable = data.inputType.HasFlag(InputType.Touch);
            bool isSwappable = data.inputType.HasFlag(InputType.Swap);

            // 예시: 폭탄 (터치 + 스왑 둘 다 지원하거나 특수한 BlockType을 가질 때)
            if (isTouchable && isSwappable)
            {
                return new BombBlock(data);
            }
            // 예시: 일반 스왑 블럭
            else if (isSwappable)
            {
                return new NormalBlock(data);
            }
            // 터치만 되는 블럭, 링크형 블럭 등 조건에 맞춰 분기 추가
            
            // 매칭되는 조건이 없으면 기본 블럭으로 생성 (조작 없음 등)
            return new NormalBlock(data);
        }
    }
}