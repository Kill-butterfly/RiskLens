using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskLens.Core.Models;

namespace RiskLens.Core.Indicators
{
    public static class FinancialIndicators
    {
        public static decimal? Ratio(decimal numerator, decimal denominator)
            => denominator == 0 ? null : numerator / denominator;

        public static decimal? CurrentRatio(FinancialSnapshot s) =>
            Ratio(s.CurrentAssets, s.CurrentLiabilities);

        public static decimal? QuickRatio(FinancialSnapshot s) =>
            Ratio(s.CurrentAssets - s.Inventory, s.CurrentLiabilities);

        public static decimal? DebtRatio(FinancialSnapshot s) =>
            Ratio(s.TotalLiabilities, s.TotalAssets);

        public static decimal? InterestCoverage(FinancialSnapshot s) =>
            Ratio(s.Ebit, s.InterestExpense);

        public static decimal? OcfToNetProfit(FinancialSnapshot s) =>
            Ratio(s.OperatingCashFlow, s.NetProfit);

        public static decimal? GoodwillToNetAssets(FinancialSnapshot s) =>
            Ratio(s.Goodwill, s.TotalAssets - s.TotalLiabilities);

        /// <summary>Altman Z 分数。非上市公司用账面权益替代市值权益，结果偏保守。</summary>
        public static decimal? AltmanZ(FinancialSnapshot s)
        {
            if (s.TotalAssets == 0 || s.TotalLiabilities == 0) return null;
            var workingCapital = s.CurrentAssets - s.CurrentLiabilities;
            var bookEquity = s.TotalAssets - s.TotalLiabilities;
            return 1.2m * workingCapital / s.TotalAssets
                 + 1.4m * s.RetainedEarnings / s.TotalAssets
                 + 3.3m * s.Ebit / s.TotalAssets
                 + 0.6m * bookEquity / s.TotalLiabilities
                 + 1.0m * s.Revenue / s.TotalAssets;
        }
    }
}
