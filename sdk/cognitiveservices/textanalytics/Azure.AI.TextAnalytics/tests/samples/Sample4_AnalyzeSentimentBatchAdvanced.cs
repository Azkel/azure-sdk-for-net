﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Testing;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Azure.AI.TextAnalytics.Samples
{
    [LiveOnly]
    public partial class TextAnalyticsSamples
    {
        [Test]
        public void AnalyzeSentimentBatchAdvanced()
        {
            string endpoint = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_ENDPOINT");
            string subscriptionKey = Environment.GetEnvironmentVariable("TEXT_ANALYTICS_SUBSCRIPTION_KEY");

            // Instantiate a client that will be used to call the service.
            var client = new TextAnalyticsClient(new Uri(endpoint), subscriptionKey);

            var inputs = new List<DocumentInput>
            {
                new DocumentInput("1")
                {
                     Language = "en",
                     Text = "That was the best day of my life!"
                },
                new DocumentInput("2")
                {
                     Language = "en",
                     Text = "This food is very bad. Everyone who ate with us got sick."
                },
                new DocumentInput("3")
                {
                     Language = "en",
                     Text = "I'm not sure how I feel about this product.",
                },
                new DocumentInput("4")
                {
                     Language = "en",
                     Text = "Pike Place Market is my favorite Seattle attraction.  We had so much fun there."
                }
            };

            SentimentResultCollection results = client.AnalyzeSentiment(inputs, new TextAnalyticsRequestOptions(showStatistics: true)).Value;

            int i = 0;
            Debug.WriteLine($"Results of Azure Text Analytics \"Sentiment Analysis\" Model, version: \"{results.ModelVersion}\"");
            Debug.WriteLine("");

            foreach (var result in results)
            {
                var document = inputs[i++];

                Debug.WriteLine($"On document (Id={document.Id}, Language=\"{document.Language}\", Text=\"{document.Text}\"):");

                Debug.WriteLine($"Document sentiment is {result.DocumentSentiment.SentimentClass.ToString()}, with scores: ");
                Debug.WriteLine($"    Positive score: {result.DocumentSentiment.PositiveScore:0.00}.");
                Debug.WriteLine($"    Neutral score: {result.DocumentSentiment.NeutralScore:0.00}.");
                Debug.WriteLine($"    Negative score: {result.DocumentSentiment.NegativeScore:0.00}.");

                Debug.WriteLine($"    Sentence sentiment results:");

                foreach (var sentenceSentiment in result.SentenceSentiments)
                {
                    Debug.WriteLine($"    On sentence \"{document.Text.Substring(sentenceSentiment.Offset, sentenceSentiment.Length)}\"");

                    Debug.WriteLine($"    Sentiment is {sentenceSentiment.SentimentClass.ToString()}, with scores: ");
                    Debug.WriteLine($"        Positive score: {sentenceSentiment.PositiveScore:0.00}.");
                    Debug.WriteLine($"        Neutral score: {sentenceSentiment.NeutralScore:0.00}.");
                    Debug.WriteLine($"        Negative score: {sentenceSentiment.NegativeScore:0.00}.");
                }

                Debug.WriteLine($"    Document statistics:");
                Debug.WriteLine($"        Character count: {result.Statistics.CharacterCount}");
                Debug.WriteLine($"        Transaction count: {result.Statistics.TransactionCount}");
                Debug.WriteLine("");
            }

            Debug.WriteLine($"Batch operation statistics:");
            Debug.WriteLine($"    Document count: {results.Statistics.DocumentCount}");
            Debug.WriteLine($"    Valid document count: {results.Statistics.ValidDocumentCount}");
            Debug.WriteLine($"    Invalid document count:{results.Statistics.InvalidDocumentCount}");
            Debug.WriteLine($"    Transaction count:{results.Statistics.TransactionCount}");
            Debug.WriteLine("");
        }
    }
}
