using System;
using GameManagers.Data;
using Stats; 
namespace Data.DataType.StatType
{
    [Serializable]
    public struct FighterStat : IKey<int>, IUnitStat
    {
        public int level { get; set; } // Key용
        public int hp { get; set; }
        public int attack { get; set; }
        public int xpRequired { get; set; }
        public int defence { get; set; }
        public float speed { get; set; }
        public float viewAngle { get; set; }
        public float viewDistance { get; set; }

        public int Key => level;
    }
}