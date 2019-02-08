﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Wikiled.Sentiment.Analysis.Processing.Persistency
{
    public class DataLoader : IDataLoader
    {
        private readonly ILogger<DataLoader> logger;

        private readonly ILoggerFactory loggerFactory;

        public DataLoader(ILoggerFactory loggerFactory)
        {
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            logger = this.loggerFactory.CreateLogger<DataLoader>();
        }

        public IDataSource Load(IDataSourceConfig source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (source.All != null)
            {
                logger.LogInformation("Loading {0}", source.All);
                if (source.All.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    return new XmlDataLoader(loggerFactory.CreateLogger<XmlDataLoader>()).LoadOldXml(source.All);
                }

                logger.LogInformation("Loading {0} as JSON", source.All);
                var data = new JsonDataSource(loggerFactory.CreateLogger< JsonDataSource>(), source.All);
                return data;
            }

            var loaders = new List<IDataSource>();
            if (!string.IsNullOrEmpty(source.Negative))
            {
                loaders.Add(new SimpleDataSource(loggerFactory.CreateLogger<SimpleDataSource>(), source.Negative, SentimentClass.Negative));
            }

            if (!string.IsNullOrEmpty(source.Positive))
            {
                loaders.Add(new SimpleDataSource(loggerFactory.CreateLogger<SimpleDataSource>(), source.Negative, SentimentClass.Positive));
            }

            return new CombinedDataSource(loaders.ToArray());
        }
    }

}