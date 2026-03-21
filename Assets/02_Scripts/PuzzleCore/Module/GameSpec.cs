using System;
using System.Collections.Generic;
using System.Linq;

namespace Puzzle.Core
{
    /// <summary>
    /// 게임 전체의 규칙과 스테이지 구성 정보를 통합 관리하는 클래스입니다.
    /// JSON 데이터를 역직렬화하여 저장하며, 모델 초기화 시 필요한 데이터를 제공합니다.
    /// </summary>
    public class GameSpec
    {
        /// <summary> 게임 전반에 적용되는 규칙 설정 </summary>
        public GameRule rule;
        /// <summary> 현재 스테이지의 타일 배치 및 초기 블럭 정보 </summary>
        public StageData stageData;

        /// <summary>
        /// 특정 ID를 가진 블럭의 상세 데이터를 반환합니다.
        /// </summary>
        /// <param name="id">찾으려는 블럭 ID</param>
        /// <returns>블럭의 설정 정보</returns>
        public BlockData GetBlock(string id)
        {
            return rule?.blocks?.FirstOrDefault(b => b.id == id);
        }
    }

    /// <summary>
    /// 퍼즐 게임의 전체적인 규칙 설정을 담는 데이터 구조체입니다.
    /// </summary>
    [Serializable]
    public class GameRule
    {
        /// <summary> 퍼즐의 종류 (3매치, 링크 등) </summary>
        public PuzzleType puzzleType;
        /// <summary> 게임 내에 등장 가능한 블럭들의 리스트 </summary>
        public List<BlockData> blocks;
        /// <summary> 스테이지 클리어를 위한 목표 리스트 </summary>
        public List<ObjectiveData> objectives;
    }

    /// <summary>
    /// 개별 블럭의 속성 정보를 담는 데이터 구조체입니다.
    /// </summary>
    [Serializable]
    public class BlockData
    {
        /// <summary> 블럭의 고유 식별자 </summary>
        public string id;
        /// <summary> 블럭의 표시 이름 </summary>
        public string name;
        /// <summary> 블럭의 조작 능력 (비트 플래그) </summary>
        public InputType inputType;
    }

    /// <summary>
    /// 스테이지 클리어 조건을 정의하는 데이터 구조체입니다.
    /// </summary>
    [Serializable]
    public class ObjectiveData
    {
        /// <summary> 목표 대상 블럭의 ID </summary>
        public string blockId;
        /// <summary> 달성해야 하는 목표 개수 </summary>
        public int targetCount;
    }

    /// <summary>
    /// 스테이지의 전체적인 배치 및 크기 정보를 담는 데이터 구조체입니다.
    /// </summary>
    [Serializable]
    public class StageData
    {
        /// <summary> 스테이지의 가로 칸 수 </summary>
        public int stage_width;
        /// <summary> 스테이지의 세로 칸 수 </summary>
        public int stage_height;
        /// <summary> 좌표별 셀 설정 정보 </summary>
        public List<CellData> cells;
    }

    /// <summary>
    /// 보드 상의 개별 칸에 대한 초기 설정을 담는 데이터 구조체입니다.
    /// </summary>
    [Serializable]
    public class CellData
    {
        /// <summary> X 좌표 </summary>
        public int x;
        /// <summary> Y 좌표 </summary>
        public int y;
        /// <summary> 타일의 종류 (0:Normal, 1:Generator 등) </summary>
        public int cell_type;
        /// <summary> 초기 배치될 블럭의 ID (비어있을 수 있음) </summary>
        public string block_id;
        /// <summary> 생성기(Generator)일 경우 등장 가능한 블럭 ID 리스트 </summary>
        public List<string> generator_block_ids;
    }
}
