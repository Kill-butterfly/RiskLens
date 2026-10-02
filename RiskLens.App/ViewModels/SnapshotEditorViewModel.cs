using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using RiskLens.Core.Models;
using RiskLens.Data;
using RiskLens.Data.Entities;

namespace RiskLens.App.ViewModels
{
    /// <summary>DataGrid 的可编辑行。与 Core 的 record 分离，因为 WPF 编辑需要可变的 INPC 对象。</summary>
    public partial class SnapshotRow : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private int _year = DateTime.Now.Year - 1;
        [ObservableProperty] private decimal _revenue;
        [ObservableProperty] private decimal _grossProfit;
        [ObservableProperty] private decimal _netProfit;
        [ObservableProperty] private decimal _operatingCashFlow;
        [ObservableProperty] private decimal _totalAssets;
        [ObservableProperty] private decimal _totalLiabilities;
        [ObservableProperty] private decimal _currentAssets;
        [ObservableProperty] private decimal _currentLiabilities;
        [ObservableProperty] private decimal _inventory;
        [ObservableProperty] private decimal _receivables;
        [ObservableProperty] private decimal _goodwill;
        [ObservableProperty] private decimal _totalDebt;
        [ObservableProperty] private decimal _interestExpense;
        [ObservableProperty] private decimal _ebit;
        [ObservableProperty] private decimal _retainedEarnings;
        [ObservableProperty] private string _source = "";

        public FinancialSnapshot ToCore() => new()
        {
            Id = Id,
            Year = Year,
            Revenue = Revenue,
            GrossProfit = GrossProfit,
            NetProfit = NetProfit,
            OperatingCashFlow = OperatingCashFlow,
            TotalAssets = TotalAssets,
            TotalLiabilities = TotalLiabilities,
            CurrentAssets = CurrentAssets,
            CurrentLiabilities = CurrentLiabilities,
            Inventory = Inventory,
            Receivables = Receivables,
            Goodwill = Goodwill,
            TotalDebt = TotalDebt,
            InterestExpense = InterestExpense,
            Ebit = Ebit,
            RetainedEarnings = RetainedEarnings,
            Source = Source
        };
    }

    public partial class SnapshotEditorViewModel : ObservableObject
    {
        private readonly RiskLensDbContext _db;
        private Func<Task>? _onSaved;

        public ObservableCollection<SnapshotRow> Rows { get; } = new();

        public SnapshotEditorViewModel(RiskLensDbContext db) => _db = db;

        public void SetOnSavedCallback(Func<Task> onSaved) => _onSaved = onSaved;

        public async Task LoadAsync(string companyName)
        {
            Rows.Clear();
            var entities = await _db.Snapshots
                .Include(s => s.Company)
                .Where(s => s.Company.Name == companyName)
                .OrderBy(s => s.Year)
                .ToListAsync();

            foreach (var e in entities)
            {
                var c = e.ToCore();
                Rows.Add(new SnapshotRow
                {
                    Id = c.Id,
                    Year = c.Year,
                    Revenue = c.Revenue,
                    GrossProfit = c.GrossProfit,
                    NetProfit = c.NetProfit,
                    OperatingCashFlow = c.OperatingCashFlow,
                    TotalAssets = c.TotalAssets,
                    TotalLiabilities = c.TotalLiabilities,
                    CurrentAssets = c.CurrentAssets,
                    CurrentLiabilities = c.CurrentLiabilities,
                    Inventory = c.Inventory,
                    Receivables = c.Receivables,
                    Goodwill = c.Goodwill,
                    TotalDebt = c.TotalDebt,
                    InterestExpense = c.InterestExpense,
                    Ebit = c.Ebit,
                    RetainedEarnings = c.RetainedEarnings,
                    Source = c.Source
                });
            }
        }

        [RelayCommand]
        private async Task SaveAsync(string companyName) => await SaveForCompanyAsync(companyName);

        private string _companyName = "";

        public async Task SaveForCompanyAsync(string companyName)
        {
            _companyName = companyName;
            if (string.IsNullOrWhiteSpace(companyName) || Rows.Count == 0) return;

            var company = await _db.Companies
                .Include(c => c.Snapshots)
                .FirstOrDefaultAsync(c => c.Name == companyName)
                ?? new CompanyEntity { Name = companyName };

            foreach (var row in Rows)
            {
                var core = row.ToCore();
                var existing = company.Snapshots.FirstOrDefault(s => s.Year == core.Year);
                if (existing is null)
                {
                    var entity = new FinancialSnapshotEntity();
                    core.CopyTo(entity);
                    company.Snapshots.Add(entity);
                }
                else
                {
                    core.CopyTo(existing);
                }
            }

            if (company.Id == 0) _db.Companies.Add(company);
            await _db.SaveChangesAsync();
            if (_onSaved is not null) await _onSaved();
        }
    }
}
