using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RiskLens.Ai;
using RiskLens.Data;
using RiskLens.Data.Entities;


namespace RiskLens.App.ViewModels
{
    public partial class ExtractionViewModel : ObservableObject
    {
        private readonly IExtractionService _extractor;
        private readonly RiskLensDbContext _db;

        [ObservableProperty] private string _sourceName = "2024年度审计报告";
        [ObservableProperty] private string _documentText = "";
        [ObservableProperty] private string _resultText = "把年报中「审计报告」「经营情况讨论与分析」章节粘贴到左侧，点击抽取。";
        [ObservableProperty] private bool _isBusy;

        public ExtractionViewModel(IExtractionService extractor, RiskLensDbContext db)
        {
            _extractor = extractor;
            _db = db;
        }

        [RelayCommand]
        private async Task ExtractAsync()
        {
            if (string.IsNullOrWhiteSpace(DocumentText)) return;
            IsBusy = true;
            try
            {
                var result = await _extractor.ExtractAsync<AuditExtraction>(
                    DocumentText, SourceName, CancellationToken.None);

                if (result.Success && result.Data is not null)
                {
                    ResultText = JsonSerializer.Serialize(result.Data,
                        new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

                    // 存档：模型输出 + 校验结果，事后可复现
                    _db.ExtractionRecords.Add(new ExtractionRecordEntity
                    {
                        SourceName = SourceName,
                        ModelName = "ollama-local",
                        RawJson = ResultText,
                        EvidenceValidated = true,
                        CreatedAt = DateTime.Now
                    });
                    await _db.SaveChangesAsync();
                }
                else
                {
                    ResultText = $"❌ 抽取失败：{result.Error}";
                }
            }
            catch (Exception ex)
            {
                ResultText = $"❌ 调用本地模型失败：{ex.Message}\n请确认 Ollama 已启动（ollama serve）且已拉取模型。";
            }
            finally { IsBusy = false; }
        }
    }
}
