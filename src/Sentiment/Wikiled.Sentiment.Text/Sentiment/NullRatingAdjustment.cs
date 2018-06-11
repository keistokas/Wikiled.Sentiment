﻿using Wikiled.Common.Arguments;
using Wikiled.Sentiment.Text.Data;
using Wikiled.Sentiment.Text.Words;

namespace Wikiled.Sentiment.Text.Sentiment
{
    public class NullRatingAdjustment : IRatingAdjustment
    {
        public NullRatingAdjustment(IParsedReview review)
        {
            Guard.NotNull(() => review, review);
            Review = review;
            Rating = review.CalculateRawRating();
        }

        public RatingData Rating { get; }

        public IParsedReview Review { get; }

        public SentimentValue GetSentiment(IWordItem word)
        {
            return word?.Relationship?.Sentiment;
        }
    }
}
