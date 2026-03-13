using UnityEngine;
using Puzzle.Core;

/// <summary>
/// 개별 퍼즐 블럭의 시각적 표현과 애니메이션을 담당하는 클래스입니다.
/// </summary>
public class PuzzleBlockView : MonoBehaviour
{
    /// <summary> 이 뷰와 연결된 블럭 모델 데이터 </summary>
    private PuzzleBlock _blockData;

    /// <summary>
    /// 블럭 뷰를 특정 모델 데이터로 초기화합니다.
    /// </summary>
    /// <param name="blockData">연결할 블럭 모델 객체</param>
    public void Initialize(PuzzleBlock blockData)
    {
        _blockData = blockData;
        UpdateVisual();
    }

    /// <summary>
    /// 모델 데이터의 상태에 맞춰 블럭의 스프라이트나 색상 등을 업데이트합니다.
    /// </summary>
    public void UpdateVisual()
    {
        if (_blockData == null) return;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 블럭 아이디에 따른 시각적 차별화 (추후 에셋 로드 방식으로 보강 가능)
            string blockId = _blockData.GetBlockId();
            
            // TODO: blockId를 기반으로 AssetManager를 통해 스프라이트를 로드하여 적용
        }
    }
}
