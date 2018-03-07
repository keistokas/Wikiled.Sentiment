﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wikiled.Sentiment.Text.NLP;
using Wikiled.Sentiment.Text.Parser;
using Wikiled.Sentiment.Text.Sentiment;
using Wikiled.Text.Analysis.Structure;

namespace Wikiled.Sentiment.TestLogic.Shared.Helpers
{
    public class DocumentLoader
    {
        private readonly ITextSplitter extraction;

        private readonly string path;

        private readonly IWordsHandler handler;

        public DocumentLoader(ITextSplitter splitter, IWordsHandler handler)
        {
            this.handler = handler;
            extraction = splitter;
            path = Path.Combine(TestContext.CurrentContext.TestDirectory, @"MachineLearning\Data\");
        }

        public async Task<Document> InitDocument(string name = "cv000_29416.txt")
        {
            var result = await extraction.Process(new ParseRequest(File.ReadAllText(Path.Combine(path, name)))).ConfigureAwait(false);
            var review = new ParsedReviewManager(handler, result).Create();
            return review.GenerateDocument(new NullRatingAdjustment());
        }

        public async Task<Document> InitDocumentWithWords()
        {
            var document = await InitDocument().ConfigureAwait(false);
            var words = document.Words.ToArray();
            for (int i = 0; i < words.Length; i++)
            {
                words[i].CalculatedValue = i % 3;
            }

            return document;
        }

        public async Task<List<Document>> InitDocumentsWithWords()
        {
            var documents = new List<Document>();
            foreach (var file in Directory.GetFiles(path, "*.txt"))
            {
                var document = await InitDocument(file).ConfigureAwait(false);
                var words = document.Words.ToArray();
                for (int i = 0; i < words.Length; i++)
                {
                    words[i].Value = i % 3;
                }

                documents.Add(document);
            }

            return documents;
        }
    }
}
