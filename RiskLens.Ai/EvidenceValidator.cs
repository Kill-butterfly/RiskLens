using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskLens.Ai
{
    /// <summary>防幻觉核心：AI 给出的每条引用必须逐字存在于原文中。</summary>
    public static class EvidenceValidator
    {
        public static bool Validate<T>(T data, string sourceText) => data switch
        {
            AuditExtraction a =>
                a.Evidence.Where(e => !string.IsNullOrWhiteSpace(e.Quote))
                          .All(e => sourceText.Contains(e.Quote, StringComparison.Ordinal))
                && a.KeyAuditMatters.All(k => sourceText.Contains(k, StringComparison.Ordinal)),
            _ => true   // 其他类型默认放行（新增类型时在此补充校验）
        };
    }
}
