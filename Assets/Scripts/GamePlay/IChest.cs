using UnityEngine;

public interface IChest : IInteractable
{
    bool IsOpened { get; }
    void OpenChest(GameObject interactor);
}
