namespace RankCalculator.UnitTests;

public class CalculateRankTests
{
    public static TheoryData<string, double> TestData => new()
    {
        { "", 0.0 },
        { "Hello", 0.0 },
        { "123", 1.0 },
        { "abc1,.23d  ef456!!!?", 0.7 },
        { "Café naïve façade", 0.1176 },
        { "☺☹☠", 1.0 },
        { new string('a', 200), 0.0 },
        { "  leading and trailing  ", 0.25 }
    };

    [Theory]
    [MemberData(nameof(TestData))]
    public void CalculateRank_ReturnsExpected(string text, double expected)
    {
        var actual = RankCalculator.CalculateRank(text);
        Assert.Equal(expected, actual, 4);
    }
}