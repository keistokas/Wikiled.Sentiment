﻿using System.Threading.Tasks;
using NLog;
using NUnit.Framework;
using Wikiled.Amazon.Logic;

namespace Wikiled.Sentiment.AcceptanceTests.Training
{
    [TestFixture]
    public class SentimentTests
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        [TestCase("B00002EQCW", "Total:<214> Positive:<96.500%> Negative:<71.429%> F1:<0.972> RMSE:0.94")]
        [TestCase("B0026127Y8", "Total:<853> Positive:<88.875%> Negative:<39.506%> F1:<0.911> RMSE:1.19")]
        public async Task TestElectronics(string product, string performance)
        {
            log.Info("TestElectronics: {0} {1}", product, performance);
            var testingClient = await Global.ElectronicBaseLine.Test(product, ProductCategory.Electronics).ConfigureAwait(false);
            Assert.AreEqual(performance, testingClient.GetPerformanceDescription());
        }


        [TestCase("B0002L5R78", "Total:<7241> Positive:<81.525%> Negative:<48.027%> F1:<0.870> RMSE:1.50")]
        public async Task TestVideo(string product, string performance)
        {
            log.Info("TestVideo: {0} {1}", product, performance);
            var testingClient = await Global.VideoBaseLine.Test(product, ProductCategory.Video).ConfigureAwait(false);
            Assert.AreEqual(performance, testingClient.GetPerformanceDescription());
        }
    }
}
