﻿using Microsoft.Extensions.Logging;
using System;
using Wikiled.Sentiment.Text.Parser;
using Wikiled.Text.Analysis.NLP;
using Wikiled.Text.Analysis.POS;
using Wikiled.Text.Analysis.POS.Tags;
using Wikiled.Text.Inquirer.Logic;

namespace Wikiled.Sentiment.Text.Words
{
    public class WordOccurenceFactory : IWordFactory
    {
        private readonly IContextWordsHandler wordsHandlers;

        private readonly IRawTextExtractor extractor;

        private readonly IInquirerManager inquirerManager;

        private readonly ILogger<WordOccurenceFactory> log;

        public WordOccurenceFactory(ILogger<WordOccurenceFactory> log, IContextWordsHandler wordsHandlers, IRawTextExtractor extractor, IInquirerManager inquirerManager)
        {
            this.wordsHandlers = wordsHandlers ?? throw new ArgumentNullException(nameof(wordsHandlers));
            this.extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            this.inquirerManager = inquirerManager ?? throw new ArgumentNullException(nameof(inquirerManager));
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public IPhrase CreatePhrase(string posPhrase)
        {
            BasePOSType postType = POSTags.Instance.UnknownPhrase;
            if (!POSTags.Instance.Contains(posPhrase))
            {
                log.LogWarning("POS not found <{0}>", posPhrase);
            }
            else
            {
                postType = POSTags.Instance.FindType(posPhrase);
            }

            return Phrase.Create(wordsHandlers, postType);
        }

        public IWordItem CreateWord(string word, string wordType)
        {
            return CreateWord(word, POSTags.Instance.FindType(wordType));
        }

        public IWordItem CreateWord(string word, string lemma, string wordType)
        {
            return WordOccurrence.Create(wordsHandlers, extractor, inquirerManager, word, lemma, POSTags.Instance.FindType(wordType));
        }

        public IWordItem CreateWord(string word, BasePOSType wordPosType)
        {
            return WordOccurrence.Create(wordsHandlers, extractor, inquirerManager, word, null, wordPosType);
        }
    }
}
