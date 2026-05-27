using System;

namespace GameManagers.DataManagement
{
    public interface IAllData
    {
        public void AddData(Type key, object value);
        public void OverWriteData(Type key, object value);
        public object GetData(Type key);
        public bool TryGetData(Type key, out object value);
        public void ClearData();
    }
}
