using Unity.Netcode;

namespace NetWork
{
    public struct NetworkAnimationInfo : INetworkSerializable
    {

        public float AnimLength;
        public float DecelerationRatio;
        public float AnimStopThreshold;
        public float AddIndicatorDuration;
        public double ServerTime;
        public float StartAnimationSpeed;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref AnimLength);
            serializer.SerializeValue(ref DecelerationRatio);
            serializer.SerializeValue(ref AnimStopThreshold);
            serializer.SerializeValue(ref AddIndicatorDuration);
            serializer.SerializeValue(ref ServerTime);
            serializer.SerializeValue(ref StartAnimationSpeed);
        }

        public NetworkAnimationInfo(float animLength, float decelerationRatio, float animStopThreshold,float addIndicatorDuration,double serverTime,float startAnimSpeed = 1f)
        {
            AnimLength = animLength;
            DecelerationRatio = decelerationRatio;
            AnimStopThreshold = animStopThreshold;
            AddIndicatorDuration = addIndicatorDuration;
            ServerTime = serverTime;
            StartAnimationSpeed = startAnimSpeed;
        }
    }
}
