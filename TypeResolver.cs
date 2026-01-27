namespace WebDev.Tool;

using System;
using Spectre.Console.Cli;

public sealed class TypeResolver(IServiceProvider provider) : ITypeResolver
{
    public object Resolve(Type type) => type == null ? null : provider.GetService(type);
}