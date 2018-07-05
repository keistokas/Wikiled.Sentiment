﻿using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NLog;
using NUnit.Framework;
using Wikiled.Amazon.Logic;
using Wikiled.Sentiment.AcceptanceTests.Helpers;
using Wikiled.Sentiment.AcceptanceTests.Helpers.Data;
using Wikiled.Sentiment.Analysis.Processing;
using Wikiled.Sentiment.Analysis.Processing.Pipeline;
using Wikiled.Sentiment.Text.NLP;

namespace Wikiled.Sentiment.AcceptanceTests.Sentiments
{
    [TestFixture]
    public class RegressionTests
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static readonly SentimentTestData[] testData =
        {
            new SentimentTestData("NotKnown", performance: "Total:<0> Positive:<0.000%> Negative:<0.000%> F1:<0.000> RMSE:NaN"),
            new SentimentTestData("B0002L5R78", 7581, 0, "Total:<7236> Positive:<77.855%> Negative:<72.222%> F1:<0.860> RMSE:1.51"),

            new SentimentTestData("B00002EQCW", 228, 0, "Total:<212> Positive:<85.787%> Negative:<66.667%> F1:<0.911> RMSE:1.34"),
            new SentimentTestData("B000BAX50G", 288, 0, "Total:<266> Positive:<89.412%> Negative:<63.636%> F1:<0.936> RMSE:1.05"),
            new SentimentTestData("B000ERAON2", 440, 0, "Total:<414> Positive:<84.444%> Negative:<75.926%> F1:<0.898> RMSE:1.30"),

            new SentimentTestData("B0026127Y8", 928, 0, "Total:<853> Positive:<69.819%> Negative:<76.543%> F1:<0.811> RMSE:1.48") { Category = ProductCategory.Video },
            new SentimentTestData("B009GN6F5Q", 472, 0, "Total:<390> Positive:<79.853%> Negative:<74.359%> F1:<0.837> RMSE:1.32") { Category = ProductCategory.Video },
            new SentimentTestData("B009CG8YJW", 418, 0, "Total:<345> Positive:<58.940%> Negative:<69.767%> F1:<0.722> RMSE:1.52") { Category = ProductCategory.Video },

            new SentimentTestData("B00004SGFS", 381, 0, "Total:<375> Positive:<94.444%> Negative:<72.727%> F1:<0.958> RMSE:0.94") { Category = ProductCategory.Kitchen },
            new SentimentTestData("B000PYF768", 507, 0, "Total:<479> Positive:<90.661%> Negative:<75.000%> F1:<0.940> RMSE:1.03") { Category = ProductCategory.Kitchen },
            new SentimentTestData("B0000Z6JIW", 297, 0, "Total:<288> Positive:<76.580%> Negative:<94.737%> F1:<0.866> RMSE:1.33") { Category = ProductCategory.Kitchen }
        };

        [TestCaseSource(nameof(testData))]
        public async Task RawSentimentDetection(SentimentTestData data)
        {
            log.Info("RawSentimentDetection: {0}", data);
            TestRunner runner = new TestRunner(TestHelper.Instance, data);
            await runner.Load().LastOrDefaultAsync();
            TestingClient testing = new TestingClient(new ProcessingPipeline(TaskPoolScheduler.Default, runner.Active, runner.Load(), new ParsedReviewManagerFactory()), string.Empty);
            testing.DisableAspects = true;
            testing.DisableSvm = true;
            testing.Init();
            await testing.Process().LastOrDefaultAsync();
            //var result = await testing.Process().ToArray();
            //result = result.OrderBy(item => item.Processed.Id).ToArray();
            //var text = result.Select(item => item.Adjustment.Rating.RawRating?.ToString()).Aggregate((one, two) => one + Environment.NewLine + two);
            Assert.AreEqual(data.Errors, testing.Errors);
            Assert.AreEqual(data.Performance, testing.GetPerformanceDescription());
        }
    }
}
