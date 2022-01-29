
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

        public object PushObjectToObjectArray(object[] array, object item)
        {
            object[] result = new object[array.Length + 1];

            Array.Copy(array, result, array.Length);

            result[result.Length - 1] = item;

            return result;
        }

        public object[] RemoveObjectFromObjectArray(object[] array, int index)
        {
            object[] result = new object[array.Length - 1];

            int i = 0;

            foreach (object item in array)
            {
                // Ignore the requested index
                if (i == index)
                {
                    i++;
                    continue;
                }

                result[i] = item;

                i++;
            }

            return result;
        }
    }
}
