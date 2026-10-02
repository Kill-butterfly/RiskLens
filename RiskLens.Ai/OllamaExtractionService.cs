using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;


namespace RiskLens.Ai
{
    /// <summary>直连本地 Ollama HTTP API。零额外依赖，数据不出本机。</summary>
    public class OllamaExtractionService : IExtractionService
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private readonly HttpClient _http;
        private readonly string _endpoint;
        private readonly string _model;

        public OllamaExtractionService(HttpClient http, string endpoint, string model)
        {
            _http = http;
            _endpoint = endpoint.TrimEnd('/');
            _model = model;
        }

        public async Task<ExtractionResult<T>> ExtractAsync<T>(
            string documentText, string sourceName, CancellationToken ct)
        {
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                var json = await CallModel(BuildPrompt(documentText), ct);
                var data = TryParse<T>(json);
                if (data is not null && EvidenceValidator.Validate(data, documentText))
                    return new ExtractionResult<T>(true, data, "", attempt);
            }
            return new ExtractionResult<T>(false, default,
                "模型输出无法通过原文校验（可能是幻觉），已重试 2 次仍失败。请缩短输入文本后重试。", 2);
        }

        private static string BuildPrompt(string documentText)
        {
            // 提取前12000字符，防止模型上下文溢出
            var textSnippet = documentText.Length > 12000
                ? documentText.Substring(0, 12000)
                : documentText;

            return $$"""
            你是财务尽调信息抽取助手。从下面的审计报告文本中抽取信息，只输出 JSON，不要输出任何其他内容。

            JSON 格式要求：
            {
              "audit_opinion_type": "标准无保留意见 | 保留意见 | 否定意见 | 无法表示意见 四选一",
              "key_audit_matters": ["关键审计事项，逐字摘自原文，最多5条"],
              "top_customers": [{"name": "客户名称", "revenue_share_percent": 数字或null}],
              "evidence": [{"quote": "支撑上述结论的原文逐字摘录", "section": "所在章节名"}]
            }

        铁律：
        1. key_audit_matters 和 evidence.quote 必须逐字摘自原文，一字不差，禁止改写、禁止总结。
        2. 找不到的信息填 null 或空数组，禁止编造。
        3. 如果文本中根本没有审计意见相关内容，所有字段填空，并在 evidence 中留空。

        待抽取文本：
        "{{documentText[..Math.Min(documentText.Length, 12000)]}}"
        """;
        }

        private async Task<string> CallModel(string prompt, CancellationToken ct)
        {
            var request = new
            {
                model = _model,
                stream = false,
                format = "json",   // 强制 Ollama 输出合法 JSON
                messages = new[] { new { role = "user", content = prompt } }
            };
            var resp = await _http.PostAsJsonAsync($"{_endpoint}/api/chat", request, ct);
            resp.EnsureSuccessStatusCode();
            var doc = await resp.Content.ReadFromJsonAsync<JsonDocument>(cancellationToken: ct);
            return doc?.RootElement.GetProperty("message").GetProperty("content").GetString() ?? "";
        }

        private static T? TryParse<T>(string content)
        {
            var cleaned = content.Trim();
            if (cleaned.StartsWith("```"))   // 防御：剥掉 markdown 围栏
            {
                cleaned = cleaned.Replace("```json", "").Replace("```", "").Trim();
            }
            try { return JsonSerializer.Deserialize<T>(cleaned, JsonOpts); }
            catch { return default; }
        }
    }
}
