using System;
using System.Collections.Generic;

namespace Puzzle.Core
{
    [Serializable]
    public struct GameSpec
    {

        public RuleData gameMode;                   // 어떤 게임 기능 데이터
        public List<BlockData> blocks;              // 어떤 블럭 기능 데이터
    }

}
