using UnityEngine;

public class PuzzleBlockCollider : MonoBehaviour
{
    [SerializeField]
    PuzzleBlockView _block;


    internal void OnClickBlock() => _block.OnClicked();

}
