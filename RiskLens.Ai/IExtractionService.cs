using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiskLens.Ai
{
    public interface IExtractionService
    {
        Task<ExtractionResult<T>> ExtractAsync<T>(string documentText, string sourceName, CancellationToken ct);
    }
}
