using RiskLens.Core.Indicators;
using RiskLens.Core.Models;
using System;

namespace RiskLens.Core.Tests
{
public class FinancialIndicatorsTests
{
    private static FinancialSnapshot Make(decimal currentAssets = 200, decimal currentLiab = 100) => new()
    {
        Year = 2024,
        Revenue = 1000,
        CurrentAssets = currentAssets,
        CurrentLiabilities = currentLiab,
        Inventory = 50
    };

    [Fact]
    public void CurrentRatio_IsNumeratorOverDenominator()
    {
        Assert.Equal(2m, FinancialIndicators.CurrentRatio(Make())!.Value);
    }

    [Fact]
    public void CurrentRatio_ReturnsNull_WhenDenominatorZero()
    {
        Assert.Null(FinancialIndicators.CurrentRatio(Make(currentLiab: 0)));
    }

    [Fact]
    public void AltmanZ_IsNotNull_ForCompleteData()
    {
        var s = Make() with
        {
            TotalAssets = 1000,
            TotalLiabilities = 400,
            RetainedEarnings = 200,
            Ebit = 150
        };
        Assert.NotNull(FinancialIndicators.AltmanZ(s));
    }
}
}

