using System;

public class Tool
{
    const char DEFAULT_DELIMITER = '\n'; // デフォルトデリミタ

    public static string[] getSplitBytes(sbyte[] pbytes, char delim)
    {
        return _getSplitBytes(0, 0, pbytes, delim);
    }

    public static string[] getSplitBytes(sbyte[] pbytes)
    {
        return _getSplitBytes(0, 0, pbytes, DEFAULT_DELIMITER);
    }

    [Obsolete]
    private static string[] _getSplitBytes(int pindex, int pLength, sbyte[] pbytes, char delimiter)
    {
        return null;
    }
}
