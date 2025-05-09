using CommandLine;

namespace DerelictDimension.ModsTool;

/// <summary>
/// Represents option for <see cref="Parser"/>, which have <see cref="Run"/> method.
/// </summary>
public interface IRunnableOptions
{
    /// <summary>
    /// Runs the command with arguments based on <see langword="this"/>.
    /// </summary>
    /// <returns>0 on success, exit code otherwise.</returns>
    public int Run();
}
