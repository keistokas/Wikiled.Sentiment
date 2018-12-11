﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Logging;

namespace Wikiled.Sentiment.Text.Parser
{
    public class LexiconLoader : ILexiconLoader
    {
        private static readonly ILogger logger = ApplicationLogging.CreateLogger<ContextWordsDataLoader>();

        private Dictionary<string, ISentimentDataHolder> table;

        public IEnumerable<string> Supported => table.Select(item => item.Key);

        public void Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(path));
            }

            logger.LogInformation("Loading lexicons: {0}", path);
            table = new Dictionary<string, ISentimentDataHolder>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in Directory.GetFiles(path))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var holder = SentimentDataHolder.Load(file);
                table[name] = holder;
            }

            logger.LogInformation("Loaded {0} lexicons", table.Count);
        }

        public ISentimentDataHolder GetLexicon(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));
            }

            logger.LogDebug("Get lexicon: {0}", name);
            if (!table.TryGetValue(name, out var value))
            {
                throw new ArgumentOutOfRangeException(nameof(name), "Lexicon not found: " + name);
            }

            return value;
        }
    }
}
