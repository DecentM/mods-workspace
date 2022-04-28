using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DecentM.Metrics
{
    public class InstanceIdGenerator
    {
        private static string alphabet = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string GenerateInstanceId(int length)
        {
            string result = "";

            for (int i = 0; i < length; i++)
            {
                result += alphabet[Random.Range(0, alphabet.Length - 1)];
            }

            return result;
        }

        public static List<string> GenerateInstanceIds(int count, int length)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string instanceId = "";

                // Just in case we get a duplicate, keep generating new ids until we get a unique one
                while (instanceId == "" || result.Contains(instanceId))
                {
                    instanceId = GenerateInstanceId(length);
                }

                result.Add(instanceId);
            }

            return result;
        }
    }
}
