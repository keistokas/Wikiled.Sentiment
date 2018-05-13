﻿using System;
using Microsoft.Extensions.Caching.Memory;
using Wikiled.Common.Arguments;
using Wikiled.Sentiment.Text.Aspects;
using Wikiled.Sentiment.Text.NLP.Repair;
using Wikiled.Sentiment.Text.Sentiment;
using Wikiled.Sentiment.Text.Words;
using Wikiled.Text.Analysis.Dictionary;
using Wikiled.Text.Analysis.Dictionary.Streams;
using Wikiled.Text.Analysis.NLP;
using Wikiled.Text.Analysis.NLP.Frequency;
using Wikiled.Text.Analysis.NLP.NRC;
using Wikiled.Text.Analysis.POS;
using Wikiled.Text.Inquirer.Logic;

namespace Wikiled.Sentiment.Text.Parser
{
    public class BasicWordsHandler : IWordsHandler
    {
        private readonly Lazy<INRCDictionary> dictionary;

        private readonly Lazy<IInquirerManager> inquirerManager;

        private IAspectDectector aspectDectector;

        private WordsDictionary invertors;

        private WordsDictionary quantifiers;

        private WordsDictionary questions;

        private SentimentDataHolder sentiment;

        private WordsDictionary stop;

        public BasicWordsHandler(IPOSTagger posTagger)
        {
            Guard.NotNull(() => posTagger, posTagger);
            PosTagger = posTagger;
            WordFactory = new WordOccurenceFactory(this);
            AspectFactory = new MainAspectHandlerFactory(this);
            Reset();
            FrequencyListManager = new FrequencyListManager();
            inquirerManager = new Lazy<IInquirerManager>(
                () =>
                    {
                        var instance = new InquirerManager();
                        instance.Load();
                        return instance;
                    });

            dictionary = new Lazy<INRCDictionary>(
                () =>
                    {
                        var instance = new NRCDictionary();
                        instance.Load();
                        return instance;
                    });
        }

        public IAspectDectector AspectDectector { get => aspectDectector; set => aspectDectector = value ?? throw new ArgumentNullException(nameof(value)); }

        public IMainAspectHandlerFactory AspectFactory { get; }

        public bool DisableFeatureSentiment { get; set; }

        public bool DisableInvertors { get; set; }

        public IRawTextExtractor Extractor { get; } = new RawWordExtractor(new BasicEnglishDictionary(), new MemoryCache(new MemoryCacheOptions()));

        public IInquirerManager InquirerManager => inquirerManager.Value;

        public FrequencyListManager FrequencyListManager { get; }

        public bool IsDisableInvertorSentiment { get; set; }

        public INRCDictionary NRCDictionary => dictionary.Value;

        public IPOSTagger PosTagger { get; }

        public ISentenceRepairHandler Repair { get; } = NullSentenceRepairHandler.Instance;

        public ISentimentDataHolder SentimentDataHolder => sentiment;

        public IWordFactory WordFactory { get; }

        public WordRepairRule FindRepairRule(IWordItem word)
        {
            return null;
        }

        public bool IsFeature(IWordItem word)
        {
            return AspectDectector != null && AspectDectector.IsAspect(word);
        }

        public bool IsInvertAdverb(IWordItem word)
        {
            return !DisableInvertors && invertors.Contains(word.Text);
        }

        public bool IsKnown(IWordItem word)
        {
            return IsFeature(word) || IsStop(word) || IsQuestion(word) || IsSentiment(word) || IsInvertAdverb(word);
        }

        public bool IsQuestion(IWordItem word)
        {
            return questions.Contains(word.Text);
        }

        public bool IsSentiment(IWordItem word)
        {
            return sentiment.MeasureSentiment(word) != null;
        }

        public bool IsStop(IWordItem word)
        {
            return stop.Contains(word.Text);
        }

        public void Load()
        {
        }

        public double? MeasureQuantifier(IWordItem word)
        {
            if (quantifiers.RawData.TryGetValue(word.Text, out double value))
            {
                if (value == 0)
                {
                    return 1.5;
                }

                if (value > 0)
                {
                    return value + 0.5;
                }

                return 1 / (-value + 0.5);
            }

            return null;
        }

        public SentimentValue MeasureSentiment(IWordItem word)
        {
            if (word.IsStopWord ||
                !word.IsSimple)
            {
                return null;
            }

            return sentiment.MeasureSentiment(word);
        }

        public void Reset()
        {
            sentiment = new SentimentDataHolder();
            invertors = WordsDictionary.Construct(new DictionaryStream(@"Resources.Dictionary.InvertorWords.txt", new EmbeddedStreamSource<BasicWordsHandler>()));
            stop = WordsDictionary.Construct(new DictionaryStream(@"Resources.Dictionary.StopWords.txt", new EmbeddedStreamSource<BasicWordsHandler>()));
            questions = WordsDictionary.Construct(new DictionaryStream(@"Resources.Dictionary.QuestionWords.txt", new EmbeddedStreamSource<BasicWordsHandler>()));
            var sentiments = WordsDictionary.Construct(new DictionaryStream(@"Resources.Dictionary.Sentiments.txt", new EmbeddedStreamSource<BasicWordsHandler>()));
            quantifiers = WordsDictionary.Construct(new DictionaryStream(@"Resources.Dictionary.Quantifiers.txt", new EmbeddedStreamSource<BasicWordsHandler>()));
            AspectDectector = NullAspectDectector.Instance;
            sentiment.PopulateEmotionsData(sentiments.RawData);
        }
    }
}
