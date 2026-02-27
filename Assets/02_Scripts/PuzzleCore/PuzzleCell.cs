namespace Puzzle.Core
{
    // 바닥(Panel)과 그 위의 물체(Block)를 모두 관리하는 컨테이너입니다.
    public class PuzzleCell
    {
        public GridPos Position { get; }
        public CellType CellType { get; set; }
        public PuzzleBlock Block { get; set; } // null일 경우 그 위엔 아무것도 없음을 의미
        public PuzzlePanel Panel { get; set; }

        public bool isLocked;

        public PuzzleCell(GridPos position)
        {
            Position = position;
            CellType = CellType.Normal; // 기본 바닥
            Block = null;
            Panel = null;
        }

        public void Update(GameInput input) 
        {
        }
    }
}
