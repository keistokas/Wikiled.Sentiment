﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Wikiled.Arff.Persistence;
using Wikiled.Sentiment.Text.MachineLearning;

namespace Wikiled.Sentiment.Analysis.Processing
{
    public class AnalyseReviews 
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private IArffDataSet currentSet;

        public AnalyseReviews()
        {
            Model = new NullMachineSentiment();
        }

        public IMachineSentiment Model { get; private set; }

        public string SvmPath { get; set; }


        public void InitEnvironment()
        {
            if (Directory.Exists(SvmPath))
            {
                Directory.Delete(SvmPath, true);
            }

            Directory.CreateDirectory(SvmPath);
        }

        public void SetArff(IArffDataSet dataSet)
        {
            currentSet = dataSet;
        }

        public async Task TrainSvm()
        {
            try
            {
                if (currentSet == null)
                {
                    throw new ArgumentNullException(nameof(currentSet));
                }

                var dataSet = currentSet;
                var machine = await MachineSentiment.Train(dataSet, CancellationToken.None).ConfigureAwait(false);
                machine.Save(SvmPath);
                LoadSvm();
                log.Info("SVM Training Completed...");
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        private void LoadSvm()
        {
            log.Info("Loading SVM vectors...");
            Model = MachineSentiment.Load(SvmPath);
        }
    }
}
