using System;

namespace WebDev.Tool.Tests.Helpers;

/// <summary>
/// Attribute to specify the execution order of tests within a collection
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class TestOrderAttribute : Attribute
{
    /// <summary>
    /// Gets the execution order of the test
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Initializes a new instance of the TestOrderAttribute class
    /// </summary>
    /// <param name="order">The execution order (lower numbers execute first)</param>
    public TestOrderAttribute(int order)
    {
        Order = order;
    }
}
