using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskLens.Core.Models
{
    public enum SignalLevel { Green, Yellow, Red }

    /// <summary>一个指标的红黄绿结论。Explanation 是给家里人看的白话。</summary>
    public record RiskSignal(
        string Code,
        string Name,
        SignalLevel Level,
        decimal? Value,
        string Unit,
        string ThresholdDesc,   // 阈值依据，展示在卡片角落
        string Explanation);
}
