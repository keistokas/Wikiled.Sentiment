﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Wikiled.Sentiment.Text.NLP;
using Wikiled.Sentiment.Text.Parser;

namespace Wikiled.Sentiment.Text.Tests.Parser
{
    [TestFixture]
    public class QueueTextSplitterTests
    {
        private Mock<ISplitterFactory> factory;

        private Mock<ITextSplitter> splitter;

        private QueueTextSplitter instance;

        [SetUp]
        public void Setup()
        {
            factory = new Mock<ISplitterFactory>();
            splitter = new Mock<ITextSplitter>();
            instance = new QueueTextSplitter(3, factory.Object);
            factory.Setup(item => item.ConstructSingle()).Returns(splitter.Object);
        }

        [Test]
        public void Construct()
        {
            Assert.Throws<ArgumentNullException>(() => new QueueTextSplitter(5, null));
            Assert.Throws<ArgumentException>(() => new QueueTextSplitter(0, factory.Object));
        }

        [TestCase(1, 1)]
        [TestCase(2, 2)]
        [TestCase(4, 3)]
        [TestCase(10, 3)]
        public async Task Process(int times, int construction)
        {
            ParseRequest request = new ParseRequest("Test");
            splitter.Setup(item => item.Process(request))
                    .Returns(
                        async () =>
                        {
                            await Task.Delay(50);
                            return null;
                        });
                
            List<Task> tasks = new List<Task>();
            for(int i = 0; i < times; i++)
            {
                tasks.Add(instance.Process(request));
            }

            await Task.WhenAll(tasks);
            instance.Dispose();
            splitter.Verify(item => item.Process(request), Times.Exactly(times));
            factory.Verify(item => item.ConstructSingle(), Times.Exactly(construction));
            splitter.Verify(item => item.Dispose(), Times.Exactly(construction));
        }
    }
}
