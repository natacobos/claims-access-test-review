namespace Claims.Tests;

// Tests written by the AI agent for MYT-3301.
public class AgentTests
{
    private static ClaimsService Service() => new(Db.CreateSeeded());
    private static readonly Requester Ana = new(1, "member");
    private static readonly Requester Support = new(900, "support");

    [Fact]
    public void Member_SeesOwnClaim()
    {
        Assert.NotNull(Service().GetClaim(Ana, 101));
    }

    [Fact]
    public void Member_CannotSeeOtherMembersClaim()
    {
        Assert.Null(Service().GetClaim(Ana, 301));
    }

    [Fact]
    public void Support_SeesAnyClaim()
    {
        Assert.NotNull(Service().GetClaim(Support, 301));
    }

    [Fact]
    public void MissingClaim_ReturnsNull()
    {
        Assert.Null(Service().GetClaim(Ana, 999));
    }

    [Fact]
    public void Member_CannotListOtherMembersClaims()
    {
        var otherMemberId = 99;
        Assert.Empty(Service().ListClaims(Ana, otherMemberId));
    }
}
