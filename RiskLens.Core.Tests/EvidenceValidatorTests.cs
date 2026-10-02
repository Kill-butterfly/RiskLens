using RiskLens.Ai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskLens.Core.Tests
{
    public class EvidenceValidatorTests
    {
        private const string Source = "审计意见：标准无保留意见。关键审计事项包括商誉减值测试。";

        [Fact]
        public void VerbatimQuote_Passes()
        {
            var data = new AuditExtraction(
                "标准无保留意见",
                ["商誉减值测试"],
                [],
                [new EvidenceSpan("审计意见：标准无保留意见", "审计报告")]);
            Assert.True(EvidenceValidator.Validate(data, Source));
        }

        [Fact]
        public void FabricatedQuote_Fails()
        {
            var data = new AuditExtraction(
                "标准无保留意见",
                [],
                [],
                [new EvidenceSpan("公司财务状况非常良好", "审计报告")]); // 原文没有
            Assert.False(EvidenceValidator.Validate(data, Source));
        }
    }
}
