﻿using Moq;
using Wikiled.Sentiment.Text.Aspects;
using Wikiled.Sentiment.Text.Parser;
using Wikiled.Text.Analysis.NLP;

namespace Wikiled.Sentiment.TestLogic.Shared.Helpers
{
    public class WordsHandlerHelper
    {
        public WordsHandlerHelper()
        {
            Handler = new Mock<IWordsHandler>();
            AspectDectector = new Mock<IAspectDectector>();
            RawTextExractor = new Mock<IRawTextExtractor>();
            Handler.Setup(item => item.AspectDectector).Returns(AspectDectector.Object);
            Handler.Setup(item => item.Extractor).Returns(RawTextExractor.Object);
            Loader = new Mock<DocumentLoader>();
            RawTextExractor.Setup(item => item.GetWord(It.IsAny<string>())).Returns((string myval) => myval);
        }

        public Mock<IWordsHandler> Handler { get; }

        public Mock<IAspectDectector> AspectDectector { get; }

        public Mock<IRawTextExtractor> RawTextExractor { get; }

        public Mock<DocumentLoader> Loader { get; }
    }
}
