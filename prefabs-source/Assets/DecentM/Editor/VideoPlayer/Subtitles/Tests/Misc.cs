using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using System.Linq;

using DecentM.EditorTools;

namespace DecentM.Prefabs.Subtitles.Tests
{
    public class Misc
    {
        private static string SrtFile = File.ReadAllText(
            $"{EditorAssets.SelfLocation}/Prefabs/VideoPlayer/Scripts/Editor/Tests/Fixtures/normal.srt"
        );

        [Test]
        public void TheGoogleTranslateTest()
        {
            var result1 = SubtitleCompiler.Compile(SrtFile, SubtitleFormat.Srt, SubtitleFormat.Vtt);
            var result2 = SubtitleCompiler.Compile(
                result1.output,
                SubtitleFormat.Vtt,
                SubtitleFormat.Vsi
            );
            var result3 = SubtitleCompiler.Compile(
                result2.output,
                SubtitleFormat.Vsi,
                SubtitleFormat.Srt
            );

            string processed = new TextProcessor(SrtFile)
                .CRLFToLF()
                .ResolveHTMLEntities()
                .GetResult();

            Assert.AreEqual(processed, result3.output);
        }
    }
}
