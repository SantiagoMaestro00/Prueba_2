using UnityEngine;
using System;

public interface IMiniGame
{
    void Init(Arrastrable item, CabinetSlotUI slot, Action<bool> onComplete);
    void Abort();
}