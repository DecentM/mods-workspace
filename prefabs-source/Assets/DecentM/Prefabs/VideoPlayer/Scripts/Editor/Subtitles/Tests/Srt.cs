using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace DecentM.Subtitles.Tests
{
    public class Srt
    {
        [Test]
        public void EmptyInputProducesEmptyResult()
        {
            var result = SubtitleCompiler.Compile("", SubtitleFormat.Srt, SubtitleFormat.Vsi);

            Assert.IsEmpty(result.output);
            Assert.Zero(result.errors.Count);
        }
    }
}
