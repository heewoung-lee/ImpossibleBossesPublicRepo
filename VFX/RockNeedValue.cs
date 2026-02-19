using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;
#if UNITY_EDITOR
public class RockNeedValue : MonoBehaviour, IPopulatingDefaultValue
{
    public void PopulatingDefaultValue(List<Object> searchedObject)
    {
        List<Mesh> meshList = searchedObject.Where(mesh=> mesh is Mesh).Cast<Mesh>().ToList();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            MeshFilter childmesh = child.GetComponent<MeshFilter>();
            MeshCollider childCollider = child.GetComponent<MeshCollider>();
            childCollider.sharedMesh = meshList.Find(findMesh => findMesh.name == child.name);
            childmesh.mesh = meshList.Find(findMesh => findMesh.name == child.name);
        }
    }
}
#endif
