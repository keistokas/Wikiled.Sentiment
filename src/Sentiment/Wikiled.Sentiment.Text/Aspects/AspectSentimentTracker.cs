﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Sentiment.Text.Aspects.Data;
using Wikiled.Sentiment.Text.Data;
using Wikiled.Sentiment.Text.Sentiment;
using Wikiled.Sentiment.Text.Words;

namespace Wikiled.Sentiment.Text.Aspects
{
    public class AspectSentimentTracker
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IContextSentimentFactory factory;

        private readonly Dictionary<string, List<SentimentValue>> table = new Dictionary<string, List<SentimentValue>>(StringComparer.OrdinalIgnoreCase);

        private readonly object syncRoot = new object();

        private int totalReviews;

        public AspectSentimentTracker(IContextSentimentFactory factory)
        {
            Guard.NotNull(() => factory, factory);
            this.factory = factory;
        }

        public void Process(IParsedReview review)
        {
            Guard.NotNull(() => review, review);
            log.Debug("Process");
            Interlocked.Increment(ref totalReviews);
            foreach (var aspect in review.Items.Where(item => item.IsFeature))
            {
                var context = factory.Construct(aspect.Relationship);
                lock(syncRoot)
                {
                    List<SentimentValue> sentiments;
                    if (!table.TryGetWordValue(aspect, out sentiments))
                    {
                        sentiments = new List<SentimentValue>();
                        table[aspect.Stemmed] = sentiments;
                    }

                    sentiments.AddRange(context.Sentiments);
                }
            }
        }

        public AspectSentimentData GetResults()
        {
            log.Debug("Save");
            AspectSentimentData data = new AspectSentimentData();
            data.TotalReviews = totalReviews;
            lock (syncRoot)
            {
                data.Records = table
                    .OrderByDescending(item => item.Key.Length)
                    .Where(item => item.Value.Count > 0)
                    .Select(
                        item => new AspectSentimentItem
                                {
                                    Text = item.Key,
                                    Times = item.Value.Count,
                                    Sentiment = RatingData.Accumulate(item.Value.Select(x => x.DataValue)).RawRating.Value
                                }).ToArray();
            }

            return data;
        }
    }
}
