using System;
using System.Collections.Generic;

namespace Puzzle.Core
{
    #region Interfaces
    /// <summary> 
    /// 터치(클릭) 조작을 받을 수 있는 블럭 능력을 정의합니다. 
    /// </summary>
    public interface ITouchableBlock
    {
        /// <summary> 블럭이 터치되었을 때 실행될 로직 </summary>
        void OnTouched(IPuzzleBoard board, GridPos myPos);
    }

    /// <summary> 
    /// 스왑(드래그 등) 조작을 받을 수 있는 블럭 능력을 정의합니다. 
    /// </summary>
    public interface ISwappableBlock
    {
        /// <summary> 블럭이 다른 블럭과 교체되었을 때 실행될 로직 </summary>
        bool OnSwapped(IPuzzleBoard board, GridPos myPos, GridPos targetPos);
    }

    /// <summary> 
    /// 선 긋기(Link) 조작을 받을 수 있는 블럭 능력을 정의합니다. 
    /// </summary>
    public interface ILinkableBlock
    {
        /// <summary> 이전 블럭과 현재 블럭이 연결 가능한지 확인 </summary>
        bool CanLink(IPuzzleBoard board, GridPos myPos, GridPos previousPos);
    }
    #endregion

    /// <summary>
    /// 셀 위에 놓여 유저가 조작하거나 매치되는 퍼즐 조각의 기본 추상 클래스입니다.
    /// </summary>
    public abstract class PuzzleBlock
    {
        /// <summary> 블럭의 속성 정보를 담고 있는 데이터 객체 </summary>
        protected BlockData _blockData;
        
        /// <summary>
        /// 블럭의 고유 아이디를 반환합니다.
        /// </summary>
        /// <returns>블럭 아이디</returns>
        public string GetBlockId()
        {
            return _blockData?.blockId;
        }

        /// <summary>
        /// 지정된 데이터를 사용하여 새로운 블럭 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="data">블럭 설정 데이터</param>
        public PuzzleBlock(BlockData data)
        {
            _blockData = data;
        }

        /// <summary>
        /// 매 프레임마다 블럭의 상태를 업데이트합니다.
        /// </summary>
        internal virtual void Update()
        {
        }
    }

    /// <summary>
    /// 3매치 등에서 스왑만 가능한 가장 기본적인 일반 블럭입니다.
    /// </summary>
    public class NormalBlock : PuzzleBlock, ISwappableBlock
    {
        public NormalBlock(BlockData data) : base(data) 
        { 
        }

        public bool OnSwapped(IPuzzleBoard board, GridPos myPos, GridPos targetPos)
        {
            // 원본 로직 유지: 기본적인 스왑 허용
            return true;
        }
    }

    /// <summary>
    /// 터치하면 즉시 폭발하거나 스왑 시 연쇄 효과를 내는 폭탄 블럭입니다.
    /// </summary>
    public class BombBlock : PuzzleBlock, ITouchableBlock, ISwappableBlock
    {
        public BombBlock(BlockData data) : base(data) 
        { 
        }

        public void OnTouched(IPuzzleBoard board, GridPos myPos)
        {
            // 원본 로직 보존 (중괄호 추가)
            var cell = board.GetCell(myPos);
            if (cell != null)
            {
                cell.Block = null;
                board.AddView(new BoardViewAction { type = ViewType.Destroy, frame = 0, position = myPos });
            }
        }

        public bool OnSwapped(IPuzzleBoard board, GridPos myPos, GridPos targetPos)
        {
            // 원본 로직 보존 (중괄호 추가)
            var targetCell = board.GetCell(targetPos);
            if (targetCell != null)
            {
                targetCell.Block = null;
                board.AddView(new BoardViewAction { type = ViewType.Destroy, frame = 0, position = targetPos });
            }
            return true;
        }
    }

    /// <summary>
    /// BlockData의 설정에 따라 적절한 PuzzleBlock의 파생 객체를 생성하는 팩토리 클래스입니다.
    /// </summary>
    public static class PuzzleBlockFactory
    {
        /// <summary>
        /// 데이터를 분석하여 터치 가능한지, 스왑 가능한지에 따라 알맞은 블럭 객체를 생성합니다.
        /// </summary>
        /// <param name="data">블럭 설정 데이터</param>
        /// <returns>생성된 구체적인 블럭 객체</returns>
        public static PuzzleBlock Create(BlockData data)
        {
            if (data == null)
            {
                return null;
            }

            bool isTouchable = data.inputType.HasFlag(InputType.Touch);
            bool isSwappable = data.inputType.HasFlag(InputType.Swap);

            if (isTouchable && isSwappable)
            {
                return new BombBlock(data);
            }
            else if (isSwappable)
            {
                return new NormalBlock(data);
            }
            
            return new NormalBlock(data);
        }
    }
}
