using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;
using System.Linq;

using DecentM.Prefabs.VideoPlayer.Plugins;
using DecentM.EditorTools;

namespace DecentM.Prefabs.Subtitles.Tests
{
    public class Vtt
    {
        private static string NormalFile = File.ReadAllText(
            $"{AssetPaths.SelfLocation}/Prefabs/VideoPlayer/Scripts/Editor/Tests/Fixtures/normal.vtt"
        );

        private static string AcidFile = File.ReadAllText(
            $"{AssetPaths.SelfLocation}/Prefabs/VideoPlayer/Scripts/Editor/Tests/Fixtures/acidtest.vtt"
        );

        [Test]
        public void EmptyInputProducesEmptyResult()
        {
            var result = SubtitleCompiler.Compile("", SubtitleFormat.Vtt, SubtitleFormat.Vsi);

            Assert.IsEmpty(result.output);
            Assert.Zero(result.errors.Count);
        }

        [Test]
        public void CompilesNormalFileWithNoErrors()
        {
            var result = SubtitleCompiler.Compile(
                NormalFile,
                SubtitleFormat.Vtt,
                SubtitleFormat.Vsi
            );

            Assert.IsNotEmpty(result.output);
            Assert.Zero(result.errors.Count);
        }

        [Test]
        public void ProducesCorrectNodeKinds()
        {
            Ast ast = SubtitleCompiler.CompileIntermediate(NormalFile, SubtitleFormat.Vtt);

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

                    case NodeKind.Comment:
                        Assert.True(node.value is string);
                        break;

                    case NodeKind.TextParameters:
                        Assert.True(node.value is Dictionary<string, string>);
                        break;

                    default:
                        Assert.Fail(
                            $"The .vtt parser shouldn't be able to produce this node kind: {node.kind}"
                        );
                        break;
                }
            }
        }

        [Test]
        public void DoesNotProduceUnnecessaryNewlines()
        {
            Ast ast = SubtitleCompiler.CompileIntermediate(NormalFile, SubtitleFormat.Vtt);

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
            Ast result = SubtitleCompiler.CompileIntermediate(NormalFile, SubtitleFormat.Vtt);
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
            Ast result = SubtitleCompiler.CompileIntermediate(NormalFile, SubtitleFormat.Vtt);
            string output = SubtitleCompiler.Write(result, SubtitleFormat.Vtt);

            Ast parsedResult = SubtitleCompiler.CompileIntermediate(output, SubtitleFormat.Vtt);
            string parsedOutput = SubtitleCompiler.Write(parsedResult, SubtitleFormat.Vtt);

            Assert.AreEqual(output, parsedOutput);
        }
    }
}
