using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDetectable
{
    public void Detect(out InteractData interactData);
}
