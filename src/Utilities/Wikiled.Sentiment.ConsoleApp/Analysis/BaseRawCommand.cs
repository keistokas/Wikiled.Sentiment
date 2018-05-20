﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using NLog;
using Wikiled.Common.Serialization;
using Wikiled.Console.Arguments;
using Wikiled.Redis.Config;
using Wikiled.Redis.Logic;
using Wikiled.Sentiment.Analysis.Processing;
using Wikiled.Sentiment.Analysis.Processing.Splitters;
using Wikiled.Sentiment.Text.Cache;
using Wikiled.Sentiment.Text.Data.Review;
using Wikiled.Sentiment.Text.Resources;
using Wikiled.Text.Analysis.Cache;
using Wikiled.Text.Analysis.POS;

namespace Wikiled.Sentiment.ConsoleApp.Analysis
{
    public abstract class BaseRawCommand : Command
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private ISplitterHelper splitter;

        protected SemaphoreSlim Semaphore { get; set; }

        public string Weights { get; set; }

        public bool FullWeightReset { get; set; }

        public bool Redis { get; set; }

        public int? Port { get; set; }

        public string Articles { get; set; }

        public string Positive { get; set; }

        public string Negative { get; set; }

        public string Input { get; set; }

        public bool UseBagOfWords { get; set; }

        public bool InvertOff { get; set; }

        public POSTaggerType Tagger { get; set; } = POSTaggerType.SharpNLP;

        public override void Execute()
        {
            log.Info("Initialize...");
            ICacheFactory cacheFactory = new NullCacheFactory();
            if (Redis)
            {
                log.Info("Using REDIS...");
                int port = Port ?? 6370;
                var redis = new RedisLink("Twitter", new RedisMultiplexer(new RedisConfiguration("localhost", port)));
                redis.Open();
                cacheFactory = new RedisDocumentCacheFactory(redis);
            }

            splitter = new MainSplitterFactory(cacheFactory, new ConfigurationHandler()).Create(Tagger);
            splitter.DataLoader.DisableFeatureSentiment = InvertOff;
            log.Info("Processing...");

            if (!string.IsNullOrEmpty(Weights))
            {
                log.Info("Adjusting Embeddings sentiments using [{0}] ...", Weights);
                if (FullWeightReset)
                {
                    log.Info("Full weight reset");
                    splitter.DataLoader.SentimentDataHolder.Clear();
                }

                var adjuster = new WeightSentimentAdjuster(splitter.DataLoader.SentimentDataHolder);
                adjuster.Adjust(Weights);
            }

            IEnumerable<IParsedDocumentHolder> review;

            if (!string.IsNullOrEmpty(Input))
            {
                review = GetOtherReviews();
            }
            else if (string.IsNullOrEmpty(Positive))
            {
                review = GetAllReviews();
            }
            else
            {
                review = GetNegativeReviews().Concat(GetPositiveReviews());
            }
                             
            Process(SynchronizedReviews(review), splitter);
        }

        protected abstract void Process(IEnumerable<IParsedDocumentHolder> reviews, ISplitterHelper splitter);

        private IEnumerable<IParsedDocumentHolder> SynchronizedReviews(IEnumerable<IParsedDocumentHolder> reviews)
        {
            foreach (var review in reviews)
            {
                Semaphore?.Wait();
                yield return review;
            }
        }

        private IEnumerable<IParsedDocumentHolder> GetAllReviews()
        {
            log.Info("Loading {0}", Articles);
            var data = XDocument.Load(Articles).XmlDeserialize<ProcessingData>();
            return splitter.Splitter.GetParsedReviewHolders(data);
        }

        private IEnumerable<IParsedDocumentHolder> GetPositiveReviews()
        {
            log.Info("Positive {0}", Positive);
            return splitter.Splitter.GetParsedReviewHolders(Positive, true);
        }

        private IEnumerable<IParsedDocumentHolder> GetNegativeReviews()
        {
            log.Info("Negative {0}", Negative);
            return splitter.Splitter.GetParsedReviewHolders(Negative, false);
        }

        private IEnumerable<IParsedDocumentHolder> GetOtherReviews()
        {
            log.Info("Other {0}", Input);
            return splitter.Splitter.GetParsedReviewHolders(Input, null);
        }
    }
}
