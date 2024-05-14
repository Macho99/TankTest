using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDetectable
{
    public void OnEnterDetect(out DetectData interactData);
    public void OnExitDetect();
}
