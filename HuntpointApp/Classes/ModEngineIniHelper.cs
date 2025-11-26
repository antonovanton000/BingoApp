namespace HuntpointApp.Classes;

using System;
using System.IO;

public static class ModEngineIniHelper
{
    /// <summary>
    /// Обновляет значения chainDInput8DLLPath и modOverrideDirectory в modengine.ini.
    /// Если ключа нет – можно при желании добавить его в конец файла.
    /// </summary>
    public static void UpdateModEngineIni(
        string iniPath,
        string newChainDInput8DLLPath,
        string newModOverrideDirectory)
    {
        if (!File.Exists(iniPath))
            throw new FileNotFoundException("Файл modengine.ini не найден", iniPath);

        var lines = File.ReadAllLines(iniPath);

        bool chainFound = false;
        bool overrideFound = false;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimStart();

            // Пропускаем комментарии и пустые строки
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#") || line.StartsWith(";"))
                continue;

            // chainDInput8DLLPath = ...
            if (line.StartsWith("chainDInput8DLLPath", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = SetIniValue(lines[i], "chainDInput8DLLPath", newChainDInput8DLLPath);
                chainFound = true;
            }

            // modOverrideDirectory = ...
            if (line.StartsWith("modOverrideDirectory", StringComparison.OrdinalIgnoreCase))
            {
                lines[i] = SetIniValue(lines[i], "modOverrideDirectory", newModOverrideDirectory);
                overrideFound = true;
            }
        }

        // Если нужно — можно добавить ключи, если их не было вовсе
        if (!chainFound)
        {
            Array.Resize(ref lines, lines.Length + 1);
            lines[^1] = $"chainDInput8DLLPath=\"{newChainDInput8DLLPath}\"";
        }

        if (!overrideFound)
        {
            Array.Resize(ref lines, lines.Length + 1);
            lines[^1] = $"modOverrideDirectory=\"{newModOverrideDirectory}\"";
        }

        File.WriteAllLines(iniPath, lines);
    }

    /// <summary>
    /// Заменяет значение после '=' в строке ini, сохраняя всё слева (включая пробелы).
    /// </summary>
    private static string SetIniValue(string originalLine, string key, string newValue)
    {
        int equalsIndex = originalLine.IndexOf('=');
        if (equalsIndex < 0)
        {
            // Строка битая, создаём нормальную
            return $"{key}=\"{newValue}\"";
        }

        string left = originalLine.Substring(0, equalsIndex + 1); // "key   ="
        string right = originalLine.Substring(equalsIndex + 1).Trim();

        // Если в ini значения в кавычках — тоже обернём в кавычки
        bool hadQuotes = right.StartsWith("\"") && right.EndsWith("\"");

        if (hadQuotes)
            return $"{left}\"{newValue}\"";
        else
            return $"{left}{newValue}";
    }
}

