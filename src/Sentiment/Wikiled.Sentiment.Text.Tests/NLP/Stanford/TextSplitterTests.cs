﻿using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.Sentiment.TestLogic.Shared.Helpers;
using Wikiled.Sentiment.Text.NLP;
using Wikiled.Sentiment.Text.NLP.Stanford;
using Wikiled.Sentiment.Text.Parser;
using Wikiled.Sentiment.Text.Words;
using Wikiled.Text.Analysis.Cache;

namespace Wikiled.Sentiment.Text.Tests.NLP.Stanford
{
    [TestFixture]
    [Timeout(1000 * 60 * 10)]
    public class TextSplitterTests
    {
        private StanfordTextSplitter splitter;

        [OneTimeSetUp]
        public void GlobalSetup()
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, ConfigurationManager.AppSettings["resources"], @"Stanford");
            splitter = new StanfordTextSplitter(path, ActualWordsHandler.Instance.WordsHandler, NullCachedDocumentsSource.Instance);
        }

        [OneTimeTearDown]
        public void Release()
        {
            splitter.Dispose();
        }

        [Test]
        public async Task ProcessX()
        {
            string sentence = "Actually I used it today for ~1.5 hours, and now, 3 hours after I am back, my left hand is still shaking.";
            var result = await splitter.Process(new ParseRequest(sentence) { Date = DateTime.Now }).ConfigureAwait(false);
            var data = new ParsedReviewManager(ActualWordsHandler.Instance.WordsHandler, result).Create();
            Assert.AreEqual(1, data.Sentences.Count);
            Assert.AreEqual(23, data.Sentences[0].Occurrences.Count());
            Assert.AreEqual(11, data.Sentences[0].Occurrences.GetImportant().Count());
            Assert.AreEqual(4, data.Sentences[0].Parts.Count());
        }

        [Test]
        public async Task ProcessPerformance()
        {
            string sentence = "By default, the application is set to search for new virus definitions daily, but you always can use the scheduling tool to change this.";
            string sentence2 = "Should a virus create serious system problems, AVG creates a rescue disk to scan your computer in MS-DOS mode.";
            for (int i = 0; i < 100; i++)
            {
                var data = await splitter.Process(new ParseRequest(sentence + " " + sentence2) { Date = DateTime.Now }).ConfigureAwait(false);
                var review = new ParsedReviewManager(ActualWordsHandler.Instance.WordsHandler, data).Create();
                Assert.IsNotNull(review);
            }
        }

        [Test]
        public async Task Process()
        {
            string sentence = "By default, the application is set to search for new virus definitions daily, but you always can use the scheduling tool to change this.";
            string sentence2 = "Should a virus create serious system problems, AVG creates a rescue disk to scan your computer in MS-DOS mode.";
            var result = await splitter.Process(new ParseRequest(sentence + " " + sentence2) { Date = DateTime.Now }).ConfigureAwait(false);
            var data = new ParsedReviewManager(ActualWordsHandler.Instance.WordsHandler, result).Create();

            Assert.AreEqual(2, data.Sentences.Count);
            Assert.AreEqual(24, data.Sentences[0].Occurrences.Count());
            Assert.AreEqual(13, data.Sentences[0].Occurrences.GetImportant().Count());
            Assert.AreEqual(3, data.Sentences[0].Parts.Count());

            Assert.AreEqual(19, data.Sentences[1].Occurrences.Count());
            Assert.AreEqual(13, data.Sentences[1].Occurrences.GetImportant().Count());
            Assert.AreEqual(2, data.Sentences[1].Parts.Count());
        }
    }
}