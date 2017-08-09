﻿using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.Sentiment.TestLogic.Shared.Helpers;
using Wikiled.Sentiment.Text.Extensions;
using Wikiled.Sentiment.Text.NLP;
using Wikiled.Sentiment.Text.Parser;
using Wikiled.Sentiment.Text.Words;

namespace Wikiled.Sentiment.Text.Tests.Words
{
    [TestFixture]
    public class PhraseContructorTests
    {
        private SimpleTextSplitter splitter;

        private PhraseContructor phraseContructor;

        [SetUp]
        public void Setup()
        {
            splitter = new SimpleTextSplitter(ActualWordsHandler.Instance.WordsHandler);
            phraseContructor = new PhraseContructor(ActualWordsHandler.Instance.WordsHandler);
        }

        [TestCase("I like my school teacher.", 2, 1, "school teacher")]
        [TestCase("If enjoy professional basketball, with nike shoes, that will be a miracle", 2, 1, "professional basketball")]
        [TestCase("If enjoy professional basketball, with nike shoes, that will be a miracle", 4, 1, "nike shoes")]
        public async Task Process(string sentence, int word, int total, string lastPhrase)
        {
            var result = await splitter.Process(new ParseRequest(sentence)).ConfigureAwait(false);
            var review = result.GetReview(ActualWordsHandler.Instance.WordsHandler);
            var words = review.Items.ToArray();
            var phrases = phraseContructor.GetPhrases(words[word]).ToArray();
            Assert.AreEqual(total, phrases.Length);
            Assert.AreEqual(lastPhrase, phrases.Last().Text);
        }
    }
}
