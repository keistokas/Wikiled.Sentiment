﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wikiled.Common.Logging;
using Wikiled.Common.Utilities.Resources;

namespace Wikiled.Sentiment.Text.Config
{
    public class LexiconConfigLoader
    {
        private readonly ILogger<LexiconConfigLoader> log;

        public LexiconConfigLoader(ILogger<LexiconConfigLoader> log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public ILexiconConfig Load(string root = null)
        {
            root ??= string.Empty;
            var fileLocation = Path.Combine(root, "lexicon.json");
            log.LogInformation("Load configuration {0}...", fileLocation);
            var config = JsonSerializer.Deserialize<LexiconConfig>(File.ReadAllBytes(fileLocation));
            if (!string.IsNullOrEmpty(root))
            {
                config.Resources = Path.Combine(root, config.Resources);
            }

            return config;
        }

        public async Task<ILexiconConfig> Download(string location = null)
        {
            var config = Load(location);
            var dataDownloader = new DataDownloader(ApplicationLogging.LoggerFactory);
            if (Directory.Exists(config.Resources))
            {
                log.LogInformation("Resources folder {0} found.", config.Resources);
            }
            else
            {
                if (config.Model == null)
                {
                    throw new ArgumentOutOfRangeException(nameof(config.Model));
                }

                await dataDownloader.DownloadFile(new Uri(config.Model.Remote), config.Resources).ConfigureAwait(false);
            }

            if (config.Lexicons != null)
            {
                await dataDownloader.DownloadFile(new Uri(config.Lexicons.Remote), config.Resources, true).ConfigureAwait(false);
            }

            return config;
        }
    }
}
