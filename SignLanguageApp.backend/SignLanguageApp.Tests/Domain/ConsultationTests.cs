using SignLanguageApp.Domain.Entities;
using SignLanguageApp.Domain.Enums;
using SignLanguageApp.Domain.Exceptions;
using SignLanguageApp.Domain.ValueObjects;
using SignLanguageApp.Tests.Fakes;

namespace SignLanguageApp.Tests.Domain;

public class ConsultationTests
{
    private readonly Consultation _c = Consultation.Start(Guid.NewGuid(), TestData.Now);
    private readonly Sign _sign = TestData.SignWithReference();
    private readonly CertaintyThresholds _t = CertaintyThresholds.Default;

    [Fact]
    public void Starts_active_with_date_and_start_time()
    {
        Assert.Equal(ConsultationStatus.Active, _c.Status);
        Assert.Equal(TestData.Now, _c.StartedAt);
        Assert.Equal(DateOnly.FromDateTime(TestData.Now), _c.Date);
        Assert.Null(_c.EndedAt);
    }

    [Fact]
    public void Pause_and_resume_keep_the_history()
    {
        _c.AddRecord(_sign, 90, TestData.Now, _t);
        _c.Pause();
        Assert.Equal(ConsultationStatus.Paused, _c.Status);
        _c.Resume();
        Assert.Equal(ConsultationStatus.Active, _c.Status);
        Assert.Single(_c.Records);
    }

    [Fact]
    public void Invalid_transitions_are_rejected()
    {
        Assert.Throws<DomainException>(() => _c.Resume());
        _c.Pause();
        Assert.Throws<DomainException>(() => _c.Pause());
        _c.Finish(TestData.Now);
        Assert.Throws<DomainException>(() => _c.Resume());
        Assert.Throws<DomainException>(() => _c.Finish(TestData.Now));
    }

    [Fact]
    public void No_records_while_paused()
    {
        _c.Pause();
        Assert.Throws<DomainException>(() => _c.AddRecord(_sign, 90, TestData.Now, _t));
    }

    [Fact]
    public void High_score_is_accepted_with_the_word()
    {
        var r = _c.AddRecord(_sign, 92.456, TestData.Now, _t);

        Assert.Equal(RecordStatus.Accepted, r.Status);
        Assert.Equal(CertaintyLevel.High, r.CertaintyLevel);
        Assert.Equal("dolor", r.Word);
        Assert.Equal(92.46m, r.SimilarityScore);
        Assert.True(r.IsInPhrase);
    }

    [Fact]
    public void Low_score_is_not_recognized_and_has_no_word()
    {
        var r = _c.AddRecord(_sign, 45, TestData.Now, _t);

        Assert.Equal(RecordStatus.NotRecognized, r.Status);
        Assert.Null(r.SignId);
        Assert.Null(r.Word);
        Assert.False(r.IsInPhrase);
    }

    [Fact]
    public void Medium_score_waits_for_confirmation()
    {
        var r = _c.AddRecord(_sign, 72, TestData.Now, _t);
        Assert.Equal(RecordStatus.Pending, r.Status);
        Assert.False(r.IsInPhrase);

        r.Confirm();
        Assert.Equal(RecordStatus.Confirmed, r.Status);
        Assert.True(r.IsInPhrase);
    }

    [Fact]
    public void Only_pending_words_can_be_reviewed()
    {
        var accepted = _c.AddRecord(_sign, 95, TestData.Now, _t);
        Assert.Throws<DomainException>(() => accepted.Confirm());

        var pending = _c.AddRecord(_sign, 65, TestData.Now, _t);
        pending.Discard();
        Assert.Throws<DomainException>(() => pending.Confirm());
    }

    [Fact]
    public void Finish_marks_pending_words_as_not_confirmed()
    {
        var pending = _c.AddRecord(_sign, 70, TestData.Now, _t);
        var end = TestData.Now.AddMinutes(5);

        _c.Finish(end);

        Assert.Equal(RecordStatus.NotConfirmed, pending.Status);
        Assert.Equal(ConsultationStatus.Finished, _c.Status);
        Assert.Equal(end, _c.EndedAt);
    }

    [Fact]
    public void Recognized_record_requires_the_sign()
    {
        Assert.Throws<DomainException>(() => _c.AddRecord(null, 90, TestData.Now, _t));
    }

    [Fact]
    public void Clear_removes_the_history()
    {
        _c.AddRecord(_sign, 90, TestData.Now, _t);
        _c.ClearRecords();
        Assert.Empty(_c.Records);
    }
}
