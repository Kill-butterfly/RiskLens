using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskLens.Ai
{
    /// <summary>防幻觉核心：AI 给出的每条引用必须逐字存在于原文中。
    /// 注意：模型返回的列表元素可能是 null，必须先过滤再 Contains，否则直接抛异常。</summary>
    public static class EvidenceValidator
    {
        public static bool Validate<T>(T data, string sourceText) => data switch
        {
            AuditExtraction a =>
                (a.Evidence ?? new List<EvidenceSpan>())
                    .Where(e => e is not null && !string.IsNullOrWhiteSpace(e.Quote))
                    .All(e => sourceText.Contains(e.Quote, StringComparison.Ordinal))
                && (a.KeyAuditMatters ?? new List<string>())
                    .Where(k => !string.IsNullOrWhiteSpace(k))
                    .All(k => sourceText.Contains(k, StringComparison.Ordinal)),
            _ => true
        };
    }
}
