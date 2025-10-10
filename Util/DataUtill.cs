using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using GameManagers;
using GameManagers.Interface.ResourcesManager;
using Newtonsoft.Json;
using UnityEngine;
using Zenject;

namespace Util
{
    public static class DataUtil
    {
        public static DataTable GetDataTable(string fileName, string tableName, IResourcesServices resourcesServices)
        {
            UnityEngine.Object obj = resourcesServices.Load<UnityEngine.Object>(fileName);
            string value = ((TextAsset)obj).ToString();
            DataTable data = JsonConvert.DeserializeObject<DataTable>(value);
            data.TableName = tableName;

            return data;
        }

        public static DataTable GetDataTable(FileInfo info, IResourcesServices resourcesServices)
        {
            string fileName = Path.GetFileNameWithoutExtension(info.Name);
            string path = string.Concat("Data/", fileName);
            string value = string.Empty;
            try
            {
                value = resourcesServices.Load<TextAsset>(path).ToString();
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            DataTable data = JsonConvert.DeserializeObject<DataTable>(value);
            data.TableName = fileName;

            return data;
        }
        public static void SetObjectFile<T>(string key, T data)
        {
            string value = JsonConvert.SerializeObject(data);
            File.WriteAllText(Application.dataPath + "/Resources/Data/" + key + ".json", value);
        }
        
        public static void AddSerializableAttributeType(Type monoScriptType,List<Type> typeList)
        {
            if (Attribute.IsDefined(monoScriptType, typeof(SerializableAttribute)))
            {
                typeList.Add(monoScriptType);
                //Debug.Log($"Find RequestType: {type.FullName}");
            }
        }
    }
}