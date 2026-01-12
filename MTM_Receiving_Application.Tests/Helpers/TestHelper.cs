using System;

namespace MTM_Receiving_Application.Tests.Helpers;

public static class TestHelper
{
    public static string NewGuidString() => Guid.NewGuid().ToString("N");
}
