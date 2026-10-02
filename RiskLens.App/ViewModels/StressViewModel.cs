using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RiskLens.Core.Models;
using RiskLens.Core.Simulation;
using RiskLens.Data;

namespace RiskLens.App.ViewModels
{
    public partial class StressViewModel : ObservableObject
    {
        private readonly RiskLensDbContext _db;
        private FinancialSnapshot? _snapshot;

        [ObservableProperty] private double _revenueDelta;      // -0.30 ~ +0.30
        [ObservableProperty] private double _marginDelta;       // -0.10 ~ +0.10
        [ObservableProperty] private double _rateDelta;         // 0 ~ 1.0

        [ObservableProperty] private string _coverageText = "—";
        [ObservableProperty] private string _verdictText = "";

        // 蒙特卡洛参数
        [ObservableProperty] private string _growthMeanText = "0.0";
        [ObservableProperty] private string _growthStdText = "0.15";

        [ObservableProperty] private string _mcText = "";

        public StressViewModel(RiskLensDbContext db) => _db = db;

        public async Task LoadAsync(string companyName)
        {
            _snapshot = await _db.Snapshots
                .Include(s => s.Company)
                .Where(s => s.Company.Name == companyName)
                .OrderByDescending(s => s.Year)
                .Select(s => s.ToCore())
                .FirstOrDefaultAsync();
            Recalculate();
        }

        partial void OnRevenueDeltaChanged(double value) => Recalculate();
        partial void OnMarginDeltaChanged(double value) => Recalculate();
        partial void OnRateDeltaChanged(double value) => Recalculate();

        private void Recalculate()
        {
            if (_snapshot is null) { CoverageText = "无数据"; VerdictText = ""; return; }
            var assumption = new StressAssumption(
                (decimal)RevenueDelta, (decimal)MarginDelta, (decimal)RateDelta);
            var coverage = StressTest.StressedInterestCoverage(_snapshot, assumption);
            CoverageText = coverage is null ? "数据不足" : $"压力情景下利息保障倍数：{coverage:F2}";
            VerdictText = StressTest.Verdict(coverage);
        }

        [RelayCommand]
        private void RunMonteCarlo()
        {
            if (_snapshot is null) { McText = "请先选择公司并录入数据。"; return; }
            if (!double.TryParse(GrowthMeanText, out var mean)) mean = 0.0;
            if (!double.TryParse(GrowthStdText, out var std)) std = 0.15;

            var r = MonteCarlo.Run(_snapshot, mean, std);
            McText = $"模拟 10,000 次（收入增速 N({mean:F2}, {std:F2})）：\n" +
                     $"连利息都付不出的概率：{r.ProbabilityCoverageBelow1:P1}\n" +
                     $"保障倍数 5% 分位：{r.P5:F2}\n" +
                     $"中位数：{r.P50:F2}　95% 分位：{r.P95:F2}";
        }
    }
}
