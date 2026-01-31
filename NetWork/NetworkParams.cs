using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace NetWork
{
    //1.27 그냥 다 열음
    //기존에는 create를 이용해서 데이터를 캡슐화 했는데 그럴것 까진 없다고 생각함
    public struct NetworkParams : INetworkSerializable
    {
        public float ArgFloat => _argFloat;
        public Vector3 ArgPosVector3 => _argPosVector3;
        public string ArgString => _argString.Value;
        public int ArgInt => _argInteger;
        public ulong ArgUlong => _argUlong;
        public bool ArgBoolean => _argBoolean;
        
        private float _argFloat;
        private Vector3 _argPosVector3;
        private FixedString512Bytes _argString;
        private int _argInteger;
        private ulong _argUlong;
        private bool _argBoolean;

        public NetworkParams(float argFloat = 0, Vector3 argPosVector3 = default, FixedString512Bytes argString = default, int argInteger = 0, bool argBoolean = false, ulong argUlong = ulong.MaxValue)
        {
            _argFloat = argFloat;
            _argPosVector3 = argPosVector3;
            _argString = argString;
            _argInteger = argInteger;
            _argUlong = argUlong;
            _argBoolean = argBoolean;
        }


        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _argFloat);
            serializer.SerializeValue(ref _argPosVector3);
            serializer.SerializeValue(ref _argString);
            serializer.SerializeValue(ref _argInteger);
            serializer.SerializeValue(ref _argUlong);
            serializer.SerializeValue(ref _argBoolean);
        }
    }
}