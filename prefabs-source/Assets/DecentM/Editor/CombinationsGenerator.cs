using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DecentM.EditorTools
{
    public class CombinationsGenerator<T>
    {
        private int progress = 0;
        private int total = 0;

        private void GetCombinationsRec(IList<IEnumerable<T>> sources, T[] chain, int index, ICollection<T[]> combinations)
        {
            foreach (var element in sources[index])
            {
                chain[index] = element;
                if (index == sources.Count - 1)
                {
                    this.progress++;
                    EditorUtility.DisplayProgressBar($"Generating combinations...", $"{progress}/{this.total}", (float)progress / this.total);

                    var finalChain = new T[chain.Length];
                    chain.CopyTo(finalChain, 0);
                    combinations.Add(finalChain);
                }
                else
                {
                    this.GetCombinationsRec(sources: sources, chain: chain, index: index + 1, combinations: combinations);
                }
            }
        }

        public List<T[]> GetCombinations(IEnumerable<T>[] enumerables, int total)
        {
            this.progress = 0;
            this.total = Mathf.Max(total, 1);

            var combinations = new List<T[]>(enumerables.Length);
            if (enumerables.Length > 0)
            {
                var chain = new T[enumerables.Length];
                this.GetCombinationsRec(sources: enumerables, chain: chain, index: 0, combinations: combinations);
            }

            EditorUtility.ClearProgressBar();
            return combinations;
        }
    }
}
