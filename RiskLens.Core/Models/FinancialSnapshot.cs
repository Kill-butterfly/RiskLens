using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskLens.Core.Models
{

    /// <summary>某一年的财务快照。所有风险计算的唯一输入。</summary>
    public record FinancialSnapshot
    {
        public int Id { get; init; }            // 数据库主键，0 = 新增
        public int Year { get; init; }
        public decimal Revenue { get; init; }           // 营业收入
        public decimal GrossProfit { get; init; }       // 毛利
        public decimal NetProfit { get; init; }         // 净利润
        public decimal OperatingCashFlow { get; init; } // 经营现金流
        public decimal TotalAssets { get; init; }
        public decimal TotalLiabilities { get; init; }
        public decimal CurrentAssets { get; init; }
        public decimal CurrentLiabilities { get; init; }
        public decimal Inventory { get; init; }
        public decimal Receivables { get; init; }
        public decimal Goodwill { get; init; }
        public decimal TotalDebt { get; init; }
        public decimal InterestExpense { get; init; }
        public decimal Ebit { get; init; }
        public decimal RetainedEarnings { get; init; }
        public string Source { get; init; } = "";      // 数据来源（年报页码/URL）
    }
}
