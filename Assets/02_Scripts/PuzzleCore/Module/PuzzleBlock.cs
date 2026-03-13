using System.Collections.Generic;

namespace Puzzle.Core
{
    /// <summary>
    /// 셀 위에 놓여 유저가 조작하거나 매치되는 실제 퍼즐 조각 클래스입니다.
    /// 이동, 파괴, 특수 능력 등의 행동을 수행할 수 있습니다.
    /// </summary>
    public class PuzzleBlock
    {
        /// <summary> 블럭의 속성 정보를 담고 있는 데이터 객체 </summary>
        private BlockData _blockData;
        
        /// <summary> 이 블럭이 수행할 구체적인 행동(커맨드) 리스트 </summary>
        private List<IBlock> _blockCommands = new List<IBlock>();
        
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
        /// 블럭에 새로운 행동 명령(커맨드)을 추가합니다.
        /// </summary>
        /// <param name="type">추가할 커맨드 종류</param>
        public void AddCommand(BlockCommandType type)
        {
            IBlock command = BlockFactory.Create(type);
            if (command != null)
            {
                _blockCommands.Add(command);
            }
        }

        /// <summary>
        /// 매 프레임마다 블럭의 상태를 업데이트하고 할당된 커맨드들을 실행합니다.
        /// </summary>
        internal void Update()
        {
            foreach (var command in _blockCommands)
            {
                command.Update();
            }
        }
    }

    /// <summary>
    /// 다양한 종류의 블럭 커맨드 객체를 생성하는 팩토리 클래스입니다.
    /// </summary>
    public static class BlockFactory
    {
        /// <summary>
        /// 요청된 타입에 맞는 블럭 커맨드 객체를 생성하여 반환합니다.
        /// </summary>
        /// <param name="type">생성할 커맨드 타입</param>
        /// <returns>생성된 커맨드 객체. 정의되지 않은 타입인 경우 null 반환</returns>
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
    /// 생성 가능한 블럭 커맨드(행동 방식)의 종류를 정의합니다.
    /// </summary>
    public enum BlockCommandType
    {
        /// <summary> 일반 매치용 블럭 </summary>
        Normal,
        /// <summary> 폭탄 아이템 블럭 </summary>
        Bomb,
        /// <summary> 기타 특수 블럭 </summary>
        Special
    }

    /// <summary>
    /// 블럭의 행동 방식을 정의하는 공통 인터페이스입니다.
    /// </summary>
    public interface IBlock
    {
        /// <summary> 커맨드 로직을 업데이트합니다. </summary>
        void Update();
    }

    /// <summary> 일반 블럭의 동작을 정의하는 커맨드 클래스입니다. </summary>
    public class NormalBlockCommand : IBlock
    {
        /// <summary> 일반 블럭 전용 업데이트 로직을 수행합니다. </summary>
        public void Update() { }
    }

    /// <summary> 폭탄 블럭의 동작을 정의하는 커맨드 클래스입니다. </summary>
    public class BombBlockCommand : IBlock
    {
        /// <summary> 폭탄 타이머 감소 및 폭발 판정 등의 로직을 수행합니다. </summary>
        public void Update() { }
    }

    /// <summary> 특수 블럭의 동작을 정의하는 커맨드 클래스입니다. </summary>
    public class SpecialBlockCommand : IBlock
    {
        /// <summary> 특수 블럭 전용 동작 로직을 수행합니다. </summary>
        public void Update() { }
    }
}
