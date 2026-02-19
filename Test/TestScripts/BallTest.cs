using UnityEngine;
using Util;

namespace Test.TestScripts
{
    public class BallTest : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            UtilDebug.Log(collision.gameObject.layer);
        }
    }
}
