using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using RiskLens.Data;

namespace RiskLens.App.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public SnapshotEditorViewModel SnapshotEditor { get; }
        public DashboardViewModel Dashboard { get; }
        public ExtractionViewModel Extraction { get; }
        public StressViewModel Stress { get; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasCompany))]
        private string _companyName = "";

        public bool HasCompany => !string.IsNullOrWhiteSpace(CompanyName);

        public MainViewModel(RiskLensDbContext db, RiskLens.Ai.IExtractionService extractor)
        {
            SnapshotEditor = new SnapshotEditorViewModel(db);
            Dashboard = new DashboardViewModel(db);
            Extraction = new ExtractionViewModel(extractor, db);
            Stress = new StressViewModel(db);
            SnapshotEditor.SetOnSavedCallback(OnSnapshotsSavedAsync);
        }

        // 公司名变化时联动所有子模块
        partial void OnCompanyNameChanged(string value) => _ = RefreshAsync();

        public async Task RefreshAsync()
        {
            if (!HasCompany) return;
            await SnapshotEditor.LoadAsync(CompanyName);
            await Dashboard.ReloadAsync(CompanyName);
            await Stress.LoadAsync(CompanyName);
        }

        // 数据保存后由录入 VM 回调，刷新分析页
        public async Task OnSnapshotsSavedAsync() => await RefreshAsync();
    }
}
