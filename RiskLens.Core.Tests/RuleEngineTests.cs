using RiskLens.Core.Models;
using RiskLens.Core.Scoring;
using System;

namespace RiskLens.Core.Tests
{
    public class RuleEngineTests
    {
        private static FinancialSnapshot Healthy => new()
        {
            Year = 2024,
            Revenue = 1000,
            CurrentAssets = 400,
            CurrentLiabilities = 100,
            Inventory = 50,
            TotalAssets = 2000,
            TotalLiabilities = 600,
            NetProfit = 200,
            OperatingCashFlow = 250,
            Goodwill = 50,
            InterestExpense = 30,
            Ebit = 200,
            RetainedEarnings = 500
        };

        [Fact]
        public void HealthyCompany_HasNoRedSignals()
        {
            var signals = RuleEngine.EvaluateAll(Healthy);
            Assert.DoesNotContain(signals, s => s.Level == SignalLevel.Red);
        }

        [Fact]
        public void LowQuickRatio_TurnsRed()
        {
            var weak = Healthy with { CurrentAssets = 100, Inventory = 80 }; // 速动=0.2
            var signal = RuleEngine.EvaluateAll(weak)
                .First(s => s.Code == "QUICK_RATIO");
            Assert.Equal(SignalLevel.Red, signal.Level);
        }

        [Fact]
        public void MissingFields_YieldYellowNotCrash()
        {
            var empty = new FinancialSnapshot { Year = 2024 };
            var signals = RuleEngine.EvaluateAll(empty);
            Assert.All(signals, s => Assert.Equal(SignalLevel.Yellow, s.Level));
        }
    }
}