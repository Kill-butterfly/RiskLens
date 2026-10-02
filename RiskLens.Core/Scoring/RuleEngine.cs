using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskLens.Core.Indicators;
using RiskLens.Core.Models;

namespace RiskLens.Core.Scoring
{
    public enum RuleDirection { HigherIsBetter, LowerIsBetter }

    /// <summary>一条阈值规则。阈值改这里，不用改代码逻辑。</summary>
    public record ThresholdRule(
        string Code,
        string Name,
        string Unit,
        RuleDirection Direction,
        decimal Green,      // 高于(或低于)此值 = 绿灯
        decimal Yellow,     // 介于绿黄之间；差于此值 = 红灯
        Func<FinancialSnapshot, decimal?> Compute,
        Func<decimal, string> Explain);

    public static class DefaultRules
    {
        public static IReadOnlyList<ThresholdRule> All =>
        [
            new("CURRENT_RATIO", "流动比率", "倍", RuleDirection.HigherIsBetter, 2m, 1.2m,
            FinancialIndicators.CurrentRatio,
            v => $"流动比率 {v:F2}：每 1 元短期负债有 {v:F2} 元流动资产兜底"),

        new("QUICK_RATIO", "速动比率", "倍", RuleDirection.HigherIsBetter, 1m, 0.6m,
            FinancialIndicators.QuickRatio,
            v => $"速动比率 {v:F2}：剔除存货后，每 1 元短期负债有 {v:F2} 元速动资产兜底"),

        new("DEBT_RATIO", "资产负债率", "比率", RuleDirection.LowerIsBetter, 0.5m, 0.75m,
            FinancialIndicators.DebtRatio,
            v => $"资产负债率 {v * 100:F0}%：总资产中有 {v * 100:F0}% 是借来的"),

        new("INTEREST_COVERAGE", "利息保障倍数", "倍", RuleDirection.HigherIsBetter, 3m, 1.5m,
            FinancialIndicators.InterestCoverage,
            v => $"利息保障倍数 {v:F2}：利润够付 {v:F2} 倍利息，低于 1 倍意味着借钱付息"),

        new("OCF_NP", "现金流/净利润", "倍", RuleDirection.HigherIsBetter, 1m, 0.3m,
            FinancialIndicators.OcfToNetProfit,
            v => $"经营现金流是净利润的 {v:F2} 倍：{v:N1} 以下说明利润可能没收到钱（纸面利润）"),

        new("GOODWILL_NA", "商誉/净资产", "比率", RuleDirection.LowerIsBetter, 0.2m, 0.5m,
            FinancialIndicators.GoodwillToNetAssets,
            v => $"商誉占净资产 {v * 100:F0}%：收购买贵了记下的账，暴雷时直接吞利润"),

        new("ALTMAN_Z", "Altman Z 分数", "分", RuleDirection.HigherIsBetter, 2.9m, 1.8m,
            FinancialIndicators.AltmanZ,
            v => $"Z 分数 {v:F2}：{v:F2} 分以上安全区，1.8 以下灰色区，越低越接近破产警戒线"),
    ];
    }

    public static class RuleEngine
    {
        public static RiskSignal Evaluate(ThresholdRule rule, FinancialSnapshot s)
        {
            var raw = rule.Compute(s);
            if (raw is null)
                return new(rule.Code, rule.Name, SignalLevel.Yellow, null, rule.Unit,
                    "数据不足", "缺少计算所需字段，请补录数据后再看");

            var v = raw.Value;
            var level = rule.Direction switch
            {
                RuleDirection.HigherIsBetter =>
                    v >= rule.Green ? SignalLevel.Green : v >= rule.Yellow ? SignalLevel.Yellow : SignalLevel.Red,
                _ =>
                    v <= rule.Green ? SignalLevel.Green : v <= rule.Yellow ? SignalLevel.Yellow : SignalLevel.Red
            };

            var threshold = rule.Direction == RuleDirection.HigherIsBetter
                ? $"绿灯 ≥ {rule.Green}，黄灯 ≥ {rule.Yellow}"
                : $"绿灯 ≤ {rule.Green}，黄灯 ≤ {rule.Yellow}";

            return new(rule.Code, rule.Name, level, v, rule.Unit, threshold, rule.Explain(v));
        }

        public static IReadOnlyList<RiskSignal> EvaluateAll(FinancialSnapshot s) =>
            DefaultRules.All.Select(r => Evaluate(r, s)).ToList();
    }
}
