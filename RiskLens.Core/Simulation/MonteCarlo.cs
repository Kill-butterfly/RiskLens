using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskLens.Core.Models;

namespace RiskLens.Core.Simulation
{
    public record StressAssumption(
    decimal RevenueDeltaPct,      // 收入变动，-0.3 = 打七折
    decimal EbitMarginDeltaPct,   // EBIT 率额外变动
    decimal InterestDeltaPct);    // 利率上浮

    public static class StressTest
    {
        public static decimal? StressedInterestCoverage(FinancialSnapshot s, StressAssumption a)
        {
            var stressedEbit = s.Ebit * (1 + a.RevenueDeltaPct) + s.Revenue * a.EbitMarginDeltaPct;
            var stressedInterest = s.InterestExpense * (1 + a.InterestDeltaPct);
            return stressedInterest == 0 ? null : stressedEbit / stressedInterest;
        }

        public static string Verdict(decimal? coverage) => coverage switch
        {
            null => "数据不足",
            < 1m => "⚠️ 连利息都付不出（覆盖 < 1 倍）",
            < 1.5m => "⚠️ 付息非常吃力（覆盖 < 1.5 倍）",
            _ => "该情景下仍能覆盖利息"
        };
    }

    public record MonteCarloResult(
        double ProbabilityCoverageBelow1,
        double P5, double P50, double P95);

    public static class MonteCarlo
    {
        /// <summary>对收入增速做正态假设，模拟利息保障倍数的分布。</summary>
        public static MonteCarloResult Run(
            FinancialSnapshot s,
            double growthMean, double growthStd,
            double? ebitMarginOverride = null,
            int iterations = 10_000)
        {
            var margin = ebitMarginOverride ?? ((double)s.Revenue == 0 ? 0.1 : (double)(s.Ebit / s.Revenue));
            var rng = new Random(42);
            var samples = new double[iterations];
            var below = 0;

            for (var i = 0; i < iterations; i++)
            {
                var u1 = 1.0 - rng.NextDouble();
                var u2 = rng.NextDouble();
                var z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                var growth = growthMean + growthStd * z;
                var revenue = (double)s.Revenue * (1 + growth);
                var ebit = revenue * margin;
                var ic = (double)s.InterestExpense == 0
                    ? double.PositiveInfinity
                    : ebit / (double)s.InterestExpense;
                samples[i] = ic;
                if (ic < 1) below++;
            }

            Array.Sort(samples);
            return new(
                (double)below / iterations,
                samples[(int)(iterations * 0.05)],
                samples[(int)(iterations * 0.50)],
                samples[(int)(iterations * 0.95)]);
        }
    }
}
