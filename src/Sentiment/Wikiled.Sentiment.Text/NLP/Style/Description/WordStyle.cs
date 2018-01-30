﻿using Newtonsoft.Json;
using Wikiled.Core.Utility.Arguments;
using Wikiled.Sentiment.Text.NLP.Style.Description.Data;
using Wikiled.Text.Analysis.NLP.NRC;
using Wikiled.Text.Analysis.Structure;

namespace Wikiled.Sentiment.Text.NLP.Style.Description
{
    public class WordStyle
    {
        public WordStyle(WordEx word)
        {
            Guard.NotNull(() => word, word);
            Word = word.Text;
        }

        public InquirerData Inquirer { get; set; }

        public NRCRecord NRC { get; set; }

        public string Word { get; }

        [JsonIgnore]
        public bool HasValue => (NRC != null && NRC.HasAnyValue) || Inquirer != null;
    }
}
