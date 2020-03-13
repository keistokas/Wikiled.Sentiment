﻿using System;
using System.Linq;
using System.Security.Cryptography;
using Wikiled.Sentiment.Text.Data;
using Wikiled.Sentiment.Text.Parser;
using Wikiled.Text.Analysis.NLP;
using Wikiled.Text.Analysis.POS;
using Wikiled.Text.Analysis.POS.Tags;
using Wikiled.Text.Analysis.Structure;
using Wikiled.Text.Inquirer.Data;
using Wikiled.Text.Inquirer.Logic;

namespace Wikiled.Sentiment.Text.Words
{
    public class WordOccurrence : IWordItem
    {
        private NamedEntities entity;
        private string customEntity;

        private WordOccurrence(string text, string raw, BasePOSType pos)
        {
            Text = text;
            POS = pos ?? throw new ArgumentNullException(nameof(pos));
            if (pos.IsGroup)
            {
                throw new ArgumentException(nameof(pos));
            }

            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(text));
            }

            Stemmed = raw;
        }

        public NamedEntities Entity
        {
            get
            {
                if (!string.IsNullOrEmpty(Text) &&
                    Text[0] == '#')
                {
                    return NamedEntities.Hashtag;
                }

                return entity;
            }

            set => entity = value;
        }

        public string CustomEntity
        {
            get => customEntity;
            set
            {
                Entity = string.IsNullOrEmpty(value) ? NamedEntities.None : NamedEntities.Misc;
                customEntity = value;
            }
        }

        public bool IsFeature { get; private set; }

        public bool IsFixed => !string.IsNullOrEmpty(Text) &&
                               Text.IndexOf("xxxbad", StringComparison.CurrentCultureIgnoreCase) == 0 ||
                               Text.IndexOf("xxxgood", StringComparison.CurrentCultureIgnoreCase) == 0;

        public bool IsInvertor { get; private set; }

        public bool IsQuestion { get; private set; }

        public bool IsSentiment { get; private set; }

        public bool IsSimple => true;

        public bool IsStopWord { get; private set; }

        public bool IsTopAttribute { get; private set; }

        public string NormalizedEntity { get; set; }

        public IWordItem Parent { get; set; }

        public BasePOSType POS { get; }

        public double? QuantValue { get; private set; }

        public IWordItemRelationships Relationship { get; private set; }

        public string Stemmed { get; }

        public string Text { get; }

        public int WordIndex { get; set; }

        public InquirerDefinition Inquirer { get; private set; }

        public static WordOccurrence Create(IContextWordsHandler wordsHandlers, IRawTextExtractor extractor, IInquirerManager inquirerManager, string text, string raw, BasePOSType pos)
        {
            if (wordsHandlers == null)
            {
                throw new ArgumentNullException(nameof(wordsHandlers));
            }

            if (extractor == null)
            {
                throw new ArgumentNullException(nameof(extractor));
            }

            if (inquirerManager == null)
            {
                throw new ArgumentNullException(nameof(inquirerManager));
            }

            text = wordsHandlers.Context.UseOriginalCase ? text : text?.ToLower();
            var rawWord = string.IsNullOrEmpty(raw) ? extractor.GetWord(text) : raw;
            rawWord = wordsHandlers.Context.UseOriginalCase ? rawWord : rawWord?.ToLower();

            if ((pos?.WordType == WordType.Symbol || pos?.WordType == WordType.SeparationSymbol) &&
                text?.Length > 1 &&
                text.Any(char.IsLetter))
            {
                pos = POSTags.Instance.UnknownWord;
            }

            var item = new WordOccurrence(text, rawWord, pos);
            item.Relationship = new WordItemRelationships(wordsHandlers, item);
            item.IsSentiment = wordsHandlers.IsSentiment(item);
            item.IsFeature = wordsHandlers.IsFeature(item);
            item.IsTopAttribute = wordsHandlers.IsAttribute(item);
            item.QuantValue = wordsHandlers.MeasureQuantifier(item);
            item.IsInvertor = wordsHandlers.IsInvertAdverb(item);
            item.IsQuestion = wordsHandlers.IsQuestion(item);
            item.IsStopWord = wordsHandlers.IsStop(item);
            item.Inquirer = inquirerManager.GetDefinitions(text);
            return item;
        }

        public static WordOccurrence CreateBasic(string text, BasePOSType pos)
        {
            var item = new WordOccurrence(text, text, pos);
            item.Relationship = new WordItemRelationships(null, item);
            return item;
        }

        public override string ToString()
        {
            if (Relationship?.Sentiment != null)
            {
                return $"Word: [{Text}] [{POS.Tag}] [Sentiment:{Relationship.Sentiment.DataValue.Value}]";
            }

            return $"Word: [{Text}] [{POS.Tag}]";
        }

        public void Reset()
        {
            Parent?.Reset();
            Relationship?.Reset();
        }
    }
}
