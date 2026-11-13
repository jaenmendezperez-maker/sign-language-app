using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Exceptions;
using SignLanguageApp.Domain.ValueObjects;
using SignLanguageApp.Tests.Fakes;

namespace SignLanguageApp.Tests.Domain;

public class SignTests
{
    private static Sign NewSign(short hands = 1) => Sign.Create("  Fiebre ", 1, hands, null, Guid.NewGuid(), TestData.Now);

    private static GeometricReference Add(Sign s, int hands, short samples = 5, double value = 0.5) => s.AddReference(
        TestData.Vector(HandGeometry.DistancesPerHand * hands, value),
        TestData.Vector(HandGeometry.AnglesPerHand * hands),
        TestData.Vector(HandGeometry.CoordinatesPerHand * hands), samples, Guid.NewGuid(), TestData.Now);

    [Fact]
    public void Word_is_normalized() => Assert.Equal("fiebre", NewSign().Word);

    [Fact]
    public void Requires_at_least_five_samples() =>
        Assert.Throws<DomainException>(() => Add(NewSign(), 1, samples: 4));

    [Fact]
    public void Vectors_must_match_the_number_of_hands()
    {
        Assert.Throws<DomainException>(() => Add(NewSign(hands: 1), hands: 2));

        var twoHands = NewSign(hands: 2);
        Add(twoHands, hands: 2);
        Assert.Single(twoHands.References);
    }

    [Fact]
    public void Rejects_invalid_numbers() =>
        Assert.Throws<DomainException>(() => Add(NewSign(), 1, value: double.NaN));

    [Fact]
    public void A_sign_can_have_variants()
    {
        var s = NewSign();
        Add(s, 1);
        Add(s, 1);
        Assert.Equal(2, s.References.Count);
    }

    [Fact]
    public void Hand_count_is_one_or_two() =>
        Assert.Throws<DomainException>(() => Sign.Create("x", 1, 3, null, Guid.NewGuid(), TestData.Now));
}
