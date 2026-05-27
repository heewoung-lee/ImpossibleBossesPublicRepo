using System;
using System.Collections.Generic;

namespace GameManagers.DataManagement
{
    public class AllData : IAllData
    {
        private Dictionary<Type,object> _allData = new Dictionary<Type,Object>();

        public void AddData(Type key, object value)
        {
            _allData.Add(key,value);
        }
        public void OverWriteData(Type key, object value)
        {
            _allData[key] = value;
        }
        public object GetData(Type key)
        {
            return _allData[key];
        }
        public bool TryGetData(Type key, out object value)
        {
            value = null;
            if (_allData.TryGetValue(key, out value) == true)
            {
                return true;
            }
            return false;
        }
        public void ClearData()
        {
            _allData.Clear();
        }
    }
}
