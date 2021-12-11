
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System;

namespace DecentM.Tools
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ArrayTools : UdonSharpBehaviour
    {
        public object[][] PushObjectArrayToJagged(object[][] array, object[] item)
        {
            object[][] result = new object[array.Length + 1][];

            Array.Copy(array, result, array.Length);

            result[result.Length - 1] = item;

            return result;
        }
    }
}
