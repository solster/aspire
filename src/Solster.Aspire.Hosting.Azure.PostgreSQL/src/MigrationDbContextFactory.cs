using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Aspire.Hosting.Azure;

internal static class MigrationDbContextFactory<TContext>
    where TContext : DbContext
{
    private static readonly Lazy<Func<DbContextOptions<TContext>, TContext>> Factory = new(CreateFactory);

    public static TContext Create(DbContextOptions<TContext> options) => Factory.Value(options);

    private static Func<DbContextOptions<TContext>, TContext> CreateFactory()
    {
        var constructor = FindOptionsConstructor();
        var options = Expression.Parameter(typeof(DbContextOptions<TContext>), "options");
        Expression argument = constructor.GetParameters()[0].ParameterType == options.Type
            ? options
            : Expression.Convert(options, constructor.GetParameters()[0].ParameterType);
        var create = Expression.New(constructor, argument);

        return Expression.Lambda<Func<DbContextOptions<TContext>, TContext>>(create, options).Compile();
    }

    private static ConstructorInfo FindOptionsConstructor()
    {
        var constructors = typeof(TContext).GetConstructors(BindingFlags.Instance | BindingFlags.Public);

        return constructors.FirstOrDefault(HasTypedOptionsParameter)
            ?? constructors.FirstOrDefault(HasUntypedOptionsParameter)
            ?? throw new InvalidOperationException(
                $"{typeof(TContext).Name} must expose a public constructor accepting " +
                $"{nameof(DbContextOptions)}<{typeof(TContext).Name}> or {nameof(DbContextOptions)}.");
    }

    private static bool HasTypedOptionsParameter(ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();

        return parameters.Length == 1 && parameters[0].ParameterType == typeof(DbContextOptions<TContext>);
    }

    private static bool HasUntypedOptionsParameter(ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();

        return parameters.Length == 1 && parameters[0].ParameterType == typeof(DbContextOptions);
    }
}
