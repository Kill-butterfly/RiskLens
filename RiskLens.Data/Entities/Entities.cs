using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace RiskLens.Data.Entities
{
    public class CompanyEntity
    {
        public int Id { get; set; }
        [MaxLength(200)] public string Name { get; set; } = "";
        public List<FinancialSnapshotEntity> Snapshots { get; set; } = new();
    }

    public class FinancialSnapshotEntity
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public CompanyEntity Company { get; set; } = null!;
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal NetProfit { get; set; }
        public decimal OperatingCashFlow { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal CurrentAssets { get; set; }
        public decimal CurrentLiabilities { get; set; }
        public decimal Inventory { get; set; }
        public decimal Receivables { get; set; }
        public decimal Goodwill { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal InterestExpense { get; set; }
        public decimal Ebit { get; set; }
        public decimal RetainedEarnings { get; set; }
        public string Source { get; set; } = "";
    }

    /// <summary>AI 抽取的原始存档：模型版本 + prompt 输出 JSON，事后可复现。</summary>
    public class ExtractionRecordEntity
    {
        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public string SourceName { get; set; } = "";
        public string ModelName { get; set; } = "";
        public string RawJson { get; set; } = "";
        public bool EvidenceValidated { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
