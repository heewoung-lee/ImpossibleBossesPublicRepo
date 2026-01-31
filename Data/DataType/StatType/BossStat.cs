using System;
using GameManagers.Data;

namespace Data.DataType.StatType
{
    [Serializable]
    public struct BossStat : IKey<int>, IGoogleSheetData
    {
        public int bossID;
        public int hp;
        public int attack;
        public int defence;
        public float speed;
        public float viewAngle;
        public float viewDistance;

        public int Key => bossID;
    }
}

