using Unity.Netcode;
using UnityEngine;
using Util;

namespace Test.TestScripts
{
    public class NgoLifeCycleTest : NetworkBehaviour
    {
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            UtilDebug.Log($"{gameObject.name} NGODeSpawn: {System.Environment.StackTrace}");
        }
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            UtilDebug.Log($"{gameObject.name}has a ParentTr when NGO Spawn {(gameObject.transform.parent is null ? "No parent object" : transform.parent.name)} stackTrace:{System.Environment.StackTrace}");

        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            UtilDebug.Log($"{gameObject.name} OnDestroy: {System.Environment.StackTrace}");
        }
        private void OnTransformParentChanged()
        {
            UtilDebug.Log($"{gameObject.name}OnParentTrChanged {(gameObject.transform.parent is null ? "No parent object" : transform.parent.name)}");
        }

    }
}
