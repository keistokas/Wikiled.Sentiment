using Wikiled.Text.Analysis.POS;
using Wikiled.Sentiment.Text.Parser;

namespace Wikiled.Sentiment.Analysis.Processing
{
    public interface ISplitterHelper
    {
        IWordsHandler DataLoader { get; }

        ITextSplitter Splitter { get; }

        int Parallel { get; }

        void Load(POSTaggerType tagger = POSTaggerType.Stanford);
    }
}