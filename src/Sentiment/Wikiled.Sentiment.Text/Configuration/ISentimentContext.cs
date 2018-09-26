﻿using Wikiled.Sentiment.Text.Aspects;
using Wikiled.Sentiment.Text.Parser;

namespace Wikiled.Sentiment.Text.Configuration
{
    public interface ISentimentContext
    {
        IAspectDectector Aspect { get; }

        bool DisableFeatureSentiment { get; }

        bool DisableInvertors { get; }

        ISentimentDataHolder Lexicon { get; }

        void ChangeAspect(IAspectDectector aspect);
    }
}
