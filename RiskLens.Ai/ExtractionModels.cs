using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskLens.Ai
{
    /// <summary>AI 的每一条结论必须携带原文证据，否则丢弃。</summary>
    public record EvidenceSpan(string Quote, string Section);

    public record CustomerConcentration(string Name, decimal? RevenueSharePercent);

    /// <summary>审计意见抽取 schema。新增抽取类型时照此模式加一个 record。</summary>
    public record AuditExtraction(
        string AuditOpinionType,                        // 标准无保留意见 / 保留意见 / 否定意见 / 无法表示意见
        List<string> KeyAuditMatters,                   // 关键审计事项（原文摘录）
        List<CustomerConcentration> TopCustomers,       // 前五大客户
        List<EvidenceSpan> Evidence);

    public record ExtractionResult<T>(bool Success, T? Data, string Error, int Attempts);
}
