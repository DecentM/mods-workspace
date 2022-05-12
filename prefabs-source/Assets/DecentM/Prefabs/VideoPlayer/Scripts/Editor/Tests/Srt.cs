using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using System.Linq;

using DecentM.VideoPlayer.Plugins;
using DecentM.EditorTools;

namespace DecentM.Subtitles.Tests
{
    public class Srt
    {
        private static string NormalFile = File.ReadAllText(
            $"{EditorAssets.SelfLocation}/Prefabs/VideoPlayer/Scripts/Editor/Tests/Fixtures/normal.srt"
        );

        [Test]
        public void EmptyInputProducesEmptyResult()
        {
            var result = SubtitleCompiler.Compile("", SubtitleFormat.Srt, SubtitleFormat.Vsi);

            Assert.IsEmpty(result.output);
            Assert.Zero(result.errors.Count);
        }

        [Test]
        public void CompilesNormalFileWithNoErrors()
        {
            var result = SubtitleCompiler.Compile(
                NormalFile,
                SubtitleFormat.Srt,
                SubtitleFormat.Vsi
            );

            Assert.IsNotEmpty(result.output);
            Assert.Zero(result.errors.Count);
        }

        [Test]
        public void ProducesCorrectNodeKinds()
        {
            Ast ast = SubtitleCompiler.CompileIntermediate(NormalFile, SubtitleFormat.Srt);

            Assert.NotZero(ast.nodes.Count);

            foreach (Node node in ast.nodes)
            {
                switch (node.kind)
                {
                    case NodeKind.TextContents:
                        Assert.True(node.value is string);
                        break;

                    case NodeKind.TimestampStart:
                        Assert.True(node.value is int);
                        break;

                    case NodeKind.TimestampEnd:
                        Assert.True(node.value is int);
                        break;

                    default:
                        Assert.Fail(
                            $"The .srt parser shouldn't be able to produce this node kind: {node.kind}"
                        );
                        break;
                }
            }
        }

        [Test]
        public void DoesNotProduceUnnecessaryNewlines()
        {
            Ast ast = SubtitleCompiler.CompileIntermediate(NormalFile, SubtitleFormat.Srt);

            foreach (Node node in ast.nodes)
            {
                if (node.kind != NodeKind.TextContents)
                    continue;

                Assert.False(((string)node.value).StartsWith("\n"));
                Assert.False(((string)node.value).EndsWith("\n"));
            }
        }

        [Test]
        public void ProducesParseableVsi()
        {
            Ast result = SubtitleCompiler.CompileIntermediate(NormalFile, SubtitleFormat.Srt);
            string output = SubtitleCompiler.Write(result, SubtitleFormat.Vsi);

            Assert.DoesNotThrow(() => SubtitlesPlugin.ParseInstructions(output));

            object[][] parsedInstructions = SubtitlesPlugin.ParseInstructions(output);
            Assert.AreEqual(output.Split('\n').Length, parsedInstructions.Length);

            int textCount = result.nodes.Where(n => n.kind == NodeKind.TextContents).Count();
            // there should be more instructions than texts, but less than double, because clear instructions are not always issued
            Assert.Less(textCount, parsedInstructions.Length);
            Assert.Less(parsedInstructions.Length, textCount * 2);
        }

        [Test]
        public void ProducesParseableSrt()
        {
            Ast result = SubtitleCompiler.CompileIntermediate(NormalFile, SubtitleFormat.Srt);
            string output = SubtitleCompiler.Write(result, SubtitleFormat.Srt);

            Ast parsedResult = SubtitleCompiler.CompileIntermediate(output, SubtitleFormat.Srt);
            string parsedOutput = SubtitleCompiler.Write(parsedResult, SubtitleFormat.Srt);

            Assert.AreEqual(output, parsedOutput);
        }
    }
}
