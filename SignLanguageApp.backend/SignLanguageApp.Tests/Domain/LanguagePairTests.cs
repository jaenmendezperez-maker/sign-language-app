using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.Exceptions;
using SignLanguageApp.Domain.ValueObjects;

namespace SignLanguageApp.Tests.Domain;

public class LanguagePairTests
{
    [Fact]
    public void Pairs_with_same_languages_are_equal()
    {
        Assert.Equal(new LanguagePair(Language.LSM, Language.LSU), new LanguagePair(Language.LSM, Language.LSU));
    }

    [Fact]
    public void Create_rejects_same_source_and_target()
    {
        Assert.Throws<UnsupportedLanguagePairException>(() => LanguagePair.Create(Language.LSM, Language.LSM));
    }

    [Fact]
    public void Create_rejects_text_as_source()
    {
        Assert.Throws<UnsupportedLanguagePairException>(() => LanguagePair.Create(Language.SpanishText, Language.LSM));
    }
}
