using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.EntityFrameworkCore;
using RiskLens.Core.Indicators;
using RiskLens.Core.Models;
using RiskLens.Core.Scoring;
using RiskLens.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskLens.App.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly RiskLensDbContext _db;

        public ObservableCollection<RiskSignal> Signals { get; } = new();

        [ObservableProperty] private ISeries[] _radarSeries = [];
        [ObservableProperty] private PolarAxis[] _radarAngleAxes = [];
        [ObservableProperty] private ISeries[] _zSeries = [];
        [ObservableProperty] private string _summary = "请先录入数据";

        public DashboardViewModel(RiskLensDbContext db) => _db = db;

        public async Task ReloadAsync(string companyName)
        {
            var snaps = await _db.Snapshots
                .Include(s => s.Company)
                .Where(s => s.Company.Name == companyName)
                .OrderBy(s => s.Year)
                .Select(s => s.ToCore())
                .ToListAsync();

            Signals.Clear();
            RadarAngleAxes = [];
            if (snaps.Count == 0) { Summary = "该公司暂无数据，请先在「数据录入」页保存。"; return; }

            var latest = snaps[^1];
            var signals = RuleEngine.EvaluateAll(latest);
            foreach (var sig in signals) Signals.Add(sig);

            var reds = signals.Count(s => s.Level == SignalLevel.Red);
            var yellows = signals.Count(s => s.Level == SignalLevel.Yellow);
            Summary = $"{companyName}（{latest.Year} 年）：{signals.Count} 项指标，🔴 {reds} 项，🟡 {yellows} 项，🟢 {signals.Count - reds - yellows} 项";

            // 雷达图：信号 → 0-100 分（仅用于形状展示，不参与决策）
            var values = signals.Select(s => new ObservableValue(
        s.Level switch { SignalLevel.Red => 20, SignalLevel.Yellow => 60, _ => 90 }))
        .ToArray();
            RadarSeries =
[
    new PolarLineSeries<ObservableValue>
    {

        Values = values,
        IsClosed = true,   // 闭合就是雷达图
        Fill = new SolidColorPaint(SKColors.SteelBlue.WithAlpha(90))
    }
];
            RadarAngleAxes =
            [
                new PolarAxis
    {
        Labels = signals.Select(s => s.Name).ToArray(),
        MinStep = 1,
        ForceStepToMin = true
    }
            ];
           

            // Z 分数历年走势
            var zPoints = snaps
                .Select(s => (s.Year, Z: FinancialIndicators.AltmanZ(s)))
                .Where(p => p.Z is not null)
                .Select(p => new ObservablePoint(p.Year, (double)p.Z!.Value))
                .ToList();
            ZSeries =
            [
                new LineSeries<ObservablePoint>
            {
                Values = zPoints,
                GeometrySize = 10,
                LineSmoothness = 0
            }
            ];
        }
    }
}
