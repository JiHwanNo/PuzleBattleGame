using System.Collections.Generic;

namespace Puzzle.Core
{
    // 셀 위에 얹어지는 실제 퍼즐 조각입니다. 이동하거나 파괴될 수 있으므로 Class로 만듭니다.
    public class PuzzleBlock
    {
        BlockData blockData;
        List<IBlock> blockCommand = new List<IBlock>();
        
        public string GetBlockId() => blockData.blockId;

        // 필요하다면 블럭의 체력, 특수 아이디 등을 여기에 추가할 수 있습니다.
        public PuzzleBlock(BlockData data)
        {
            blockData = data;
        }

        public void AddCommand(BlockCommandType type)
        {
            IBlock command = BlockFactory.Create(type);
            if (command != null)
            {
                blockCommand.Add(command);
            }
        }

        internal void Update()
        {
            // 블럭의 상태를 업데이트하는 로직
            foreach (var command in blockCommand)
            {
                command.Update();
            }
        }
    }

    /// <summary>
    /// IBlock을 생성하는 팩토리 클래스입니다.
    /// </summary>
    public static class BlockFactory
    {
        public static IBlock Create(BlockCommandType type)
        {
            switch (type)
            {
                case BlockCommandType.Normal:
                    return new NormalBlockCommand();
                case BlockCommandType.Bomb:
                    return new BombBlockCommand();
                case BlockCommandType.Special:
                    return new SpecialBlockCommand();
                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// 생성할 수 있는 블럭(커맨드)의 종류를 정의합니다.
    /// </summary>
    public enum BlockCommandType
    {
        Normal,
        Bomb,
        Special
    }

    /// <summary>
    /// 다양한 블럭이 업데이트할 경우에 대해 적는다.
    /// </summary>
    public interface IBlock
    {
        void Update();
    }

    // --- IBlock 구현체 예시 ---

    public class NormalBlockCommand : IBlock
    {
        public void Update()
        {
            // 일반 블럭 업데이트 로직
        }
    }

    public class BombBlockCommand : IBlock
    {
        public void Update()
        {
            // 폭탄 블럭 업데이트 로직 (예: 타이머 감소 등)
        }
    }

    public class SpecialBlockCommand : IBlock
    {
        public void Update()
        {
            // 특수 블럭 업데이트 로직
        }
    }
}