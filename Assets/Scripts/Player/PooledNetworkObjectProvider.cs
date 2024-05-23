using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PooledNetworkObjectProvider : NetworkObjectProviderDefault
{
    public override NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject instance)
    {

        return base.AcquirePrefabInstance(runner, context, out instance);
    }
    public override void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
    {
        base.ReleaseInstance(runner, context);
    }

}
