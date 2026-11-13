using SignLanguageApp.Application.DTOs;
using SignLanguageApp.Application.Errors;
using SignLanguageApp.Application.UseCases.Auth;
using SignLanguageApp.Application.UseCases.Catalog;
using SignLanguageApp.Application.UseCases.Consultations;
using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.Exceptions;
using SignLanguageApp.Domain.ValueObjects;
using SignLanguageApp.Tests.Fakes;

namespace SignLanguageApp.Tests.Application;

public class LoginUseCaseTests
{
    private readonly InMemoryStore _store = new();
    private readonly LoginUseCase _login;

    public LoginUseCaseTests()
    {
        var user = User.Create("Dra. Ana", "Medico@Signlink.Local", "hash:secreta", UserRole.MedicalStaff, TestData.Now);
        _store.Users.Add(user);
        _login = new LoginUseCase(_store, new FakeHasher(), new FakeTokens());
    }

    [Fact]
    public async Task Valid_credentials_return_a_token_with_the_role()
    {
        var result = await _login.ExecuteAsync(new LoginCommand(" medico@signlink.local ", "secreta"));
        Assert.StartsWith("token-", result.Token);
        Assert.Equal(UserRole.MedicalStaff, result.Role);
    }

    [Theory]
    [InlineData("medico@signlink.local", "otra")]
    [InlineData("nadie@signlink.local", "secreta")]
    public async Task Wrong_credentials_give_the_same_error(string email, string password) =>
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _login.ExecuteAsync(new LoginCommand(email, password)));

    [Fact]
    public async Task Inactive_user_cannot_log_in()
    {
        _store.Users[0].Deactivate();
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _login.ExecuteAsync(new LoginCommand("medico@signlink.local", "secreta")));
    }
}

public class RegisterSignReferenceUseCaseTests
{
    private readonly InMemoryStore _store = new();
    private readonly RegisterSignReferenceUseCase _register;
    private readonly Guid _admin = Guid.NewGuid();

    public RegisterSignReferenceUseCaseTests()
    {
        var category = Category.Create("Síntomas");
        typeof(Category).GetProperty(nameof(Category.Id))!.SetValue(category, 1);
        _store.Categories.Add(category);
        _register = new RegisterSignReferenceUseCase(_store, _store, new FixedClock(TestData.Now));
    }

    private static RegisterReferenceCommand Command(string word = "Dolor", short hands = 1, int categoryId = 1, string? url = null) => new(
        word, categoryId, hands, url,
        TestData.Vector(HandGeometry.DistancesPerHand * hands),
        TestData.Vector(HandGeometry.AnglesPerHand * hands),
        TestData.Vector(HandGeometry.CoordinatesPerHand * hands), 5);

    [Fact]
    public async Task Registers_a_new_sign_with_its_reference()
    {
        var sign = await _register.ExecuteAsync(Command(), _admin);

        Assert.Equal("dolor", sign.Word);
        Assert.Single(sign.References);
        Assert.Equal(1, _store.SaveCount);
    }

    [Fact]
    public async Task Same_word_adds_a_variant()
    {
        await _register.ExecuteAsync(Command("dolor"), _admin);
        var sign = await _register.ExecuteAsync(Command(" DOLOR "), _admin);

        Assert.Single(_store.Signs);
        Assert.Equal(2, sign.References.Count);
    }

    [Fact]
    public async Task Variant_must_use_the_same_number_of_hands()
    {
        await _register.ExecuteAsync(Command("dolor", hands: 1), _admin);
        await Assert.ThrowsAsync<RequestValidationException>(() => _register.ExecuteAsync(Command("dolor", hands: 2), _admin));
    }

    [Fact]
    public async Task Fewer_than_five_samples_is_a_bad_request() =>
        await Assert.ThrowsAsync<RequestValidationException>(() => _register.ExecuteAsync(Command() with { SampleCount = 4 }, _admin));

    [Fact]
    public async Task Vectors_must_match_the_declared_hands() =>
        await Assert.ThrowsAsync<RequestValidationException>(() => _register.ExecuteAsync(Command() with { HandCount = 2 }, _admin));

    [Theory]
    [InlineData("", 1, null)]
    [InlineData("dolor", 99, null)]
    [InlineData("dolor", 1, "javascript:alert(1)")]
    public async Task Rejects_invalid_requests(string word, int categoryId, string? url) =>
        await Assert.ThrowsAsync<RequestValidationException>(() => _register.ExecuteAsync(Command(word, categoryId: categoryId, url: url), _admin));
}

public class ConsultationUseCaseTests
{
    private readonly InMemoryStore _store = new();
    private readonly FixedClock _clock = new(TestData.Now);
    private readonly Guid _doctor = Guid.NewGuid();
    private readonly Sign _sign = TestData.SignWithReference("ayuda");

    public ConsultationUseCaseTests() => _store.Signs.Add(_sign);

    private RecordTranslationUseCase Record() => new(_store, _store, _store, TestData.Certainty(), _clock);

    private async Task<ConsultationDto> StartAsync() => await new StartConsultationUseCase(_store, _store, _clock).ExecuteAsync(_doctor);

    [Fact]
    public async Task Server_recomputes_level_and_takes_the_word_from_the_catalog()
    {
        var c = await StartAsync();

        var high = await Record().ExecuteAsync(c.Id, _doctor, new RecordTranslationCommand(_sign.Id, 91, null));
        var medium = await Record().ExecuteAsync(c.Id, _doctor, new RecordTranslationCommand(_sign.Id, 70, null));
        var low = await Record().ExecuteAsync(c.Id, _doctor, new RecordTranslationCommand(_sign.Id, 30, null));

        Assert.Equal((RecordStatus.Accepted, "ayuda"), (high.Status, high.Word));
        Assert.Equal(RecordStatus.Pending, medium.Status);
        Assert.Equal((RecordStatus.NotRecognized, (string?)null), (low.Status, low.Word));
    }

    [Fact]
    public async Task Recognized_word_needs_a_sign_id()
    {
        var c = await StartAsync();
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Record().ExecuteAsync(c.Id, _doctor, new RecordTranslationCommand(null, 85, null)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.5)]
    [InlineData(double.NaN)]
    public async Task Score_must_be_between_0_and_100(double score)
    {
        var c = await StartAsync();
        await Assert.ThrowsAsync<RequestValidationException>(() =>
            Record().ExecuteAsync(c.Id, _doctor, new RecordTranslationCommand(_sign.Id, score, null)));
    }

    [Fact]
    public async Task Another_doctor_cannot_see_the_consultation()
    {
        var c = await StartAsync();
        var history = new ConsultationHistoryUseCase(_store, _store);
        await Assert.ThrowsAsync<NotFoundException>(() => history.GetAsync(c.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task Browser_time_far_from_server_time_is_ignored()
    {
        var c = await StartAsync();
        var r = await Record().ExecuteAsync(c.Id, _doctor, new RecordTranslationCommand(_sign.Id, 90, TestData.Now.AddHours(-3)));
        Assert.Equal(TestData.Now, r.RecognizedAt);
    }

    [Fact]
    public async Task Full_flow_pause_review_finish_and_export()
    {
        var c = await StartAsync();
        var pending = await Record().ExecuteAsync(c.Id, _doctor, new RecordTranslationCommand(_sign.Id, 65, null));
        var pending2 = await Record().ExecuteAsync(c.Id, _doctor, new RecordTranslationCommand(_sign.Id, 66, null));

        var review = new ReviewTranslationUseCase(_store, _store);
        Assert.Equal(RecordStatus.Confirmed, (await review.ConfirmAsync(c.Id, pending.Id, _doctor)).Status);

        var pr = new PauseResumeConsultationUseCase(_store, _store);
        await pr.PauseAsync(c.Id, _doctor);
        await Assert.ThrowsAsync<DomainException>(() => Record().ExecuteAsync(c.Id, _doctor, new RecordTranslationCommand(_sign.Id, 90, null)));
        await pr.ResumeAsync(c.Id, _doctor);

        _clock.Now = TestData.Now.AddMinutes(12);
        var finished = await new FinishConsultationUseCase(_store, _store, _clock).ExecuteAsync(c.Id, _doctor);
        Assert.Equal(ConsultationStatus.Finished, finished.Status);
        Assert.Equal(RecordStatus.NotConfirmed, finished.Records.Single(r => r.Id == pending2.Id).Status);

        var (fileName, text) = await new ExportSummaryUseCase(_store).ExecuteAsync(c.Id, _doctor, TimeZoneInfo.Utc);
        Assert.EndsWith(".txt", fileName);
        Assert.Contains("Hora de inicio: 18:00:00", text);
        Assert.Contains("Hora de fin:    18:12:00", text);
        Assert.Contains("ayuda", text);
        Assert.Contains("Media", text);
        Assert.Contains("(confirmada)", text);
        Assert.Contains("(no confirmada)", text);
    }

    [Fact]
    public async Task Summary_without_words_says_so()
    {
        var c = await StartAsync();
        var (_, text) = await new ExportSummaryUseCase(_store).ExecuteAsync(c.Id, _doctor, TimeZoneInfo.Utc);
        Assert.Contains("Sin palabras registradas", text);
        Assert.Contains("en curso", text);
    }
}
