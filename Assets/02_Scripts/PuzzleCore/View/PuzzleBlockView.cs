using UnityEngine;
using Puzzle.Core; // Core 네임스페이스 참조

public class PuzzleBlockView : MonoBehaviour
{
    private PuzzleBlock _blockData;

    // 블록의 데이터를 연결하고 시각적 요소를 초기화합니다.
    public void Initialize(PuzzleBlock blockData)
    {
        _blockData = blockData;
        UpdateVisual();
    }

    // 블록의 타입(BlockType)에 따라 색상이나 스프라이트를 변경합니다.
    public void UpdateVisual()
    {
        if (_blockData == null) return;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // 테스트를 위해 BlockType에 따라 색상을 다르게 지정합니다.
            switch (_blockData.Type) //
            {
                case BlockType.Normal: //
                    spriteRenderer.color = Color.white;
                    break;
                case BlockType.Item: //
                    spriteRenderer.color = Color.red;
                    break;
                case BlockType.Target: //
                    spriteRenderer.color = Color.blue;
                    break;
                default:
                    spriteRenderer.color = Color.gray;
                    break;
            }
        }
    }
}