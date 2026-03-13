using System;
using System.Collections.Generic;

namespace Puzzle.Core
{
    [Serializable]
    public class GameSpec
    {
        public StageData stageData;
        public List<RuleData> rules;
        public List<BlockData> blocks;

        public RuleData GetRule(string ruleId) => rules?.Find(r => r.ruleId == ruleId) ?? default;
        public BlockData GetBlock(string blockId) => blocks?.Find(b => b.blockId == blockId);
    }

    [Serializable]
    public class StageData
    {
        public int stage_id;
        public int stage_width;
        public int stage_height;
        public List<CellData> cells;
    }

    [Serializable]
    public class CellData
    {
        public int x;
        public int y;
        public string block_id;
        public int panel_id;
        public int cell_type;
    }
}