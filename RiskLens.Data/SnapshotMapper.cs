using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskLens.Core.Models;
using RiskLens.Data.Entities;

namespace RiskLens.Data
{
    public static class SnapshotMapper
    {
        public static FinancialSnapshot ToCore(this FinancialSnapshotEntity e) => new()
        {
            Id = e.Id,
            Year = e.Year,
            Revenue = e.Revenue,
            GrossProfit = e.GrossProfit,
            NetProfit = e.NetProfit,
            OperatingCashFlow = e.OperatingCashFlow,
            TotalAssets = e.TotalAssets,
            TotalLiabilities = e.TotalLiabilities,
            CurrentAssets = e.CurrentAssets,
            CurrentLiabilities = e.CurrentLiabilities,
            Inventory = e.Inventory,
            Receivables = e.Receivables,
            Goodwill = e.Goodwill,
            TotalDebt = e.TotalDebt,
            InterestExpense = e.InterestExpense,
            Ebit = e.Ebit,
            RetainedEarnings = e.RetainedEarnings,
            Source = e.Source
        };

        public static void CopyTo(this FinancialSnapshot c, FinancialSnapshotEntity e)
        {
            e.Year = c.Year; e.Revenue = c.Revenue; e.GrossProfit = c.GrossProfit;
            e.NetProfit = c.NetProfit; e.OperatingCashFlow = c.OperatingCashFlow;
            e.TotalAssets = c.TotalAssets; e.TotalLiabilities = c.TotalLiabilities;
            e.CurrentAssets = c.CurrentAssets; e.CurrentLiabilities = c.CurrentLiabilities;
            e.Inventory = c.Inventory; e.Receivables = c.Receivables; e.Goodwill = c.Goodwill;
            e.TotalDebt = c.TotalDebt; e.InterestExpense = c.InterestExpense; e.Ebit = c.Ebit;
            e.RetainedEarnings = c.RetainedEarnings; e.Source = c.Source;
        }
    }

}
