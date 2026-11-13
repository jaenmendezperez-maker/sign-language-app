using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Tests.Domain;

public class CertaintyThresholdsTests
{
    private readonly CertaintyThresholds _t = CertaintyThresholds.Default;

    [Theory]
    [InlineData(100, CertaintyLevel.High)]
    [InlineData(80, CertaintyLevel.High)]
    [InlineData(79.99, CertaintyLevel.Medium)]
    [InlineData(72, CertaintyLevel.Medium)] // caso de prueba de RF-06
    [InlineData(60, CertaintyLevel.Medium)]
    [InlineData(59.99, CertaintyLevel.Low)]
    [InlineData(45, CertaintyLevel.Low)]    // caso de prueba de RF-07
    [InlineData(0, CertaintyLevel.Low)]
    public void Level_follows_the_RF06_thresholds(double score, CertaintyLevel expected) =>
        Assert.Equal(expected, _t.LevelFor(score));

    [Theory]
    [InlineData(CertaintyLevel.High, RecordStatus.Accepted)]
    [InlineData(CertaintyLevel.Medium, RecordStatus.Pending)]
    [InlineData(CertaintyLevel.Low, RecordStatus.NotRecognized)]
    public void Initial_status_depends_on_the_level(CertaintyLevel level, RecordStatus expected) =>
        Assert.Equal(expected, CertaintyThresholds.InitialStatusFor(level));

    [Theory]
    [InlineData(60, 80)]
    [InlineData(80, 0)]
    [InlineData(120, 60)]
    public void Rejects_inconsistent_thresholds(double high, double medium) =>
        Assert.Throws<ArgumentException>(() => new CertaintyThresholds(high, medium));

    [Fact]
    public void Custom_thresholds_are_used()
    {
        var strict = new CertaintyThresholds(90, 70);
        Assert.Equal(CertaintyLevel.Medium, strict.LevelFor(85));
    }
}
