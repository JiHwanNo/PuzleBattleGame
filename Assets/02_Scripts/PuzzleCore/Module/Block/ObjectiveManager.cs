using System.Collections.Generic;

namespace Puzzle.Core
{
    /// <summary>
    /// 스테이지의 클리어 목표(Block Collection 등)와 현재 달성 수치를 실시간으로 관리하는 클래스입니다.
    /// 보드 로직에서 블럭이 파괴될 때 이 매니저를 통해 수치를 갱신하며,
    /// 모든 목표가 달성되었는지를 판별하여 게임 종료(Finish) 시점을 결정하는 역할을 합니다.
    /// </summary>
    public class ObjectiveManager
    {
        /// <summary> 각 목표 블럭 ID별 현재 수집된 개수 </summary>
        private Dictionary<string, int> _currentCounts = new Dictionary<string, int>();
        
        /// <summary> 스테이지 시작 시 설정된 클리어 목표 데이터 목록 </summary>
        private List<ObjectiveData> _objectives;

        /// <summary>
        /// 지정된 목표 데이터를 바탕으로 매니저를 초기화합니다.
        /// </summary>
        /// <param name="objectives">GameSpec으로부터 전달받은 목표 리스트</param>
        public ObjectiveManager(List<ObjectiveData> objectives)
        {
            _objectives = objectives ?? new List<ObjectiveData>();
            _currentCounts = new Dictionary<string, int>();
        }

        /// <summary>
        /// 특정 블럭이 파괴되었을 때 호출되어 수집 카운트를 갱신합니다.
        /// </summary>
        /// <param name="blockId">파괴된 블럭의 고유 아이디</param>
        public void OnBlockDestroyed(string blockId)
        {
            if (string.IsNullOrEmpty(blockId))
            {
                return;
            }

            if (_currentCounts.ContainsKey(blockId))
            {
                _currentCounts[blockId]++;
            }
            else
            {
                _currentCounts[blockId] = 1;
            }
        }

        /// <summary>
        /// 현재 설정된 모든 클리어 조건이 만족되었는지 검사합니다.
        /// </summary>
        /// <returns>모든 목표를 달성했다면 true, 하나라도 미달성이라면 false</returns>
        public bool IsAllObjectivesCleared()
        {
            if (_objectives == null || _objectives.Count == 0)
            {
                return false;
            }

            foreach (var obj in _objectives)
            {
                // 특정 블럭을 목표 개수만큼 모았는지 체크
                _currentCounts.TryGetValue(obj.blockId, out int count);
                if (count < obj.targetCount)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 특정 블럭의 현재 수집 개수를 가져옵니다.
        /// </summary>
        /// <param name="blockId">블럭 ID</param>
        /// <returns>수집된 개수</returns>
        public int GetCurrentCount(string blockId)
        {
            _currentCounts.TryGetValue(blockId, out int count);
            return count;
        }
    }
}
