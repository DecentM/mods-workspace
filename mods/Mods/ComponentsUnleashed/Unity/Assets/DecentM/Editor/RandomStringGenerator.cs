using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DecentM.EditorTools
{
    public class RandomStringGenerator
    {
        private static string alphabet =
            "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string GenerateRandomString(int length)
        {
            string result = "";

            for (int i = 0; i < length; i++)
            {
                result += alphabet[Random.Range(0, alphabet.Length - 1)];
            }

            return result;
        }

        public static List<string> GenerateRandomStrings(int count, int length)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < count; i++)
            {
                string instanceId = "";

                // Just in case we get a duplicate, keep generating new ids until we get a unique one
                while (instanceId == "" || result.Contains(instanceId))
                {
                    instanceId = GenerateRandomString(length);
                }

                result.Add(instanceId);
            }

            return result;
        }
    }
}
