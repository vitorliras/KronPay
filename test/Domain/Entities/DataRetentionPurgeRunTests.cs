using Domain.Entities.DataRetention;
using Shouldly;

namespace Tests.Domain.Entities;

public class DataRetentionPurgeRunTests
{
    [Fact]
    public void Comeca_com_LastRunAt_igual_a_MinValue()
    {
        var run = new DataRetentionPurgeRun();

        run.LastRunAt.ShouldBe(DateTime.MinValue);
    }

    [Fact]
    public void MarkRun_atualiza_LastRunAt()
    {
        var run = new DataRetentionPurgeRun();
        var when = new DateTime(2026, 7, 11, 10, 0, 0);

        run.MarkRun(when);

        run.LastRunAt.ShouldBe(when);
    }
}
