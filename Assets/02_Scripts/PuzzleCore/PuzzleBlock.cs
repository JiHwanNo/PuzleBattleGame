namespace Puzzle.Core
{
    // 셀 위에 얹어지는 실제 퍼즐 조각입니다. 이동하거나 파괴될 수 있으므로 Class로 만듭니다.
    public class PuzzleBlock
    {
        public BlockType Type { get; set; }

        // 필요하다면 블럭의 체력, 특수 아이디 등을 여기에 추가할 수 있습니다.
        public PuzzleBlock(BlockType type)
        {
            Type = type;
        }
    }
}