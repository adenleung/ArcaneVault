namespace ArcaneVault.Api.Services;

public static class EnvFileLoader
{
    public static void LoadFromParents(string fileName)
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            var path = Path.Combine(directory.FullName, fileName);
            if (File.Exists(path))
            {
                foreach (var raw in File.ReadAllLines(path))
                {
                    var line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith('#')) continue;
                    var separator = line.IndexOf('=');
                    if (separator <= 0) continue;
                    var name = line[..separator].Trim();
                    var value = line[(separator + 1)..].Trim().Trim('"', '\'');
                    if (Environment.GetEnvironmentVariable(name) is null)
                        Environment.SetEnvironmentVariable(name, value);
                }
                return;
            }
            directory = directory.Parent;
        }
    }
}
