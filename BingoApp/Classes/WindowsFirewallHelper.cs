using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BingoApp.Classes
{
    public class WindowsFirewallHelper
    {
        public static void OpenPortNetsh(int port, string ruleName = "BingoApp LocalServer Port")
        {
            var localPort = FirewallRuleExists(ruleName);
            if (localPort == port) return;
            
            if (localPort != 0)
            {                
                UpdateFirewallPort(port, ruleName);
            }
            else
            {
                var psi = new ProcessStartInfo("netsh", $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol=TCP localport={port}")
                {
                    Verb = "runas", // Запуск от администратора
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }

        public static void UpdateFirewallPort(int newPort, string ruleName = "BingoApp LocalServer Port")
        {
            // Удалить старое правило (если оно есть)
            var deletePsi = new ProcessStartInfo("netsh", $"advfirewall firewall delete rule name=\"{ruleName}\"")
            {
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true
            };
            Process.Start(deletePsi)?.WaitForExit();

            // Добавить новое правило с новым портом
            var addPsi = new ProcessStartInfo("netsh", $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=allow protocol=TCP localport={newPort}")
            {
                Verb = "runas",
                CreateNoWindow = true,
                UseShellExecute = true
            };
            Process.Start(addPsi);
        }

        public static int FirewallRuleExists(string ruleName)
        {
            var psi = new ProcessStartInfo("netsh", $"advfirewall firewall show rule name=\"{ruleName}\"")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                // Проверяем по ключевой фразе (можно добавить проверку для английской и русской локализации)
                if (output.Contains("Нет правил") || output.Contains("No rules"))
                    return 0;
                else
                {
                    // Ищем строку с портом
                    string portLine = output.Split('\n')
                        .FirstOrDefault(line => line.TrimStart().StartsWith("Локальный порт") || line.TrimStart().StartsWith("LocalPort"));

                    if (portLine != null)
                    {
                        var parts = portLine.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int port))
                            return port;
                        else
                            return 0;
                    }
                    return 0;
                }
            }
        }



    }
}
