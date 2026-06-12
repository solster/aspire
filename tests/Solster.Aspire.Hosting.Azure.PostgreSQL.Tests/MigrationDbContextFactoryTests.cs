using Aspire.Hosting.Azure;
using Microsoft.EntityFrameworkCore;

namespace Solster.Aspire.Hosting.Azure.PostgreSQL.Tests;

public class MigrationDbContextFactoryTests
{
    [Fact]
    public void Create_UsesTypedOptionsConstructor()
    {
        var options = new DbContextOptionsBuilder<TypedOptionsContext>().Options;

        using var context = MigrationDbContextFactory<TypedOptionsContext>.Create(options);

        Assert.Same(options, context.Options);
    }

    [Fact]
    public void Create_UsesUntypedOptionsConstructor()
    {
        var options = new DbContextOptionsBuilder<UntypedOptionsContext>().Options;

        using var context = MigrationDbContextFactory<UntypedOptionsContext>.Create(options);

        Assert.Same(options, context.Options);
    }

    [Fact]
    public void Create_ThrowsTargetedErrorWhenOptionsConstructorIsMissing()
    {
        var options = new DbContextOptionsBuilder<MissingOptionsContext>().Options;

        var exception = Assert.Throws<InvalidOperationException>(
            () => MigrationDbContextFactory<MissingOptionsContext>.Create(options));

        Assert.Contains("must expose a public constructor", exception.Message);
        Assert.Contains("DbContextOptions<MissingOptionsContext>", exception.Message);
    }
}

public sealed class TypedOptionsContext(DbContextOptions<TypedOptionsContext> options) : DbContext(options)
{
    public DbContextOptions<TypedOptionsContext> Options { get; } = options;
}

public sealed class UntypedOptionsContext(DbContextOptions options) : DbContext(options)
{
    public DbContextOptions Options { get; } = options;
}

public sealed class MissingOptionsContext : DbContext
{
    public MissingOptionsContext()
    {
    }
}
