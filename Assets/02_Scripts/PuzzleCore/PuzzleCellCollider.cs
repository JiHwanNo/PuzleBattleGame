using UnityEngine;

public class PuzzleCellCollider : MonoBehaviour
{
    [SerializeField]
    PuzzleCellView _cell;
    internal void OnClickCell() => _cell.OnClicked();

}