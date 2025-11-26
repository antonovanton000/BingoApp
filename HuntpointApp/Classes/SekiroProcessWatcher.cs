using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HuntpointApp.Classes
{
    public class SekiroProcessWatcher
    {
        private readonly MemoryReader mem;
        private readonly string processName = "sekiro";
        private Process? currentProcess;
        private IntPtr baseAddress = IntPtr.Zero;
        private int moduleSize = 0;

        public event Action? OnAttach;
        public event Action? OnDetach;

        private bool isAttached = false;
        private CancellationTokenSource? cts;

        public SekiroProcessWatcher(MemoryReader mem)
        {
            this.mem = mem;
        }

        /// <summary>
        /// Запускает фоновый мониторинг процесса sekiro.exe.
        /// </summary>
        public void Start()
        {
            cts = new CancellationTokenSource();
            Task.Run(() => WatchLoop(cts.Token), cts.Token);
        }

        /// <summary>
        /// Останавливает мониторинг.
        /// </summary>
        public void Stop()
        {
            cts?.Cancel();
            Detach();
        }

        private async Task WatchLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Проверяем, запущен ли процесс
                    var processes = Process.GetProcessesByName(processName);

                    if (processes.Length == 0)
                    {
                        // Процесс не найден
                        if (isAttached)
                        {
                            App.Logger.Info("🟥 Sekiro закрылся — отсоединяемся.");
                            Detach();
                        }
                    }
                    else
                    {
                        var proc = processes[0];
                        if (!isAttached)
                        {
                            await Task.Delay(5000);
                            Attach(proc);
                        }
                        else if (currentProcess != null && currentProcess.Id != proc.Id)
                        {
                            // старый процесс умер, а новый запущен
                            App.Logger.Info("🔁 Sekiro перезапущен — переаттач.");
                            Detach();
                            Attach(proc);
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.Logger.Error($"❌ WatchLoop error: {ex.Message}");
                }

                await Task.Delay(1000, token); // проверка раз в секунду
            }
        }

        private void Attach(Process proc)
        {
            try
            {
                currentProcess = proc;
                baseAddress = proc.MainModule!.BaseAddress;
                moduleSize = proc.MainModule.ModuleMemorySize;

                isAttached = mem.Attach(proc);
                App.Logger.Info($"🟩 Подключено к Sekiro (PID={proc.Id}, base=0x{baseAddress.ToInt64():X})");
                OnAttach?.Invoke();                
            }
            catch (Exception ex)
            {
                App.Logger.Info($"❌ Ошибка Attach: {ex.Message}");
                Detach();
            }
        }

        private void Detach()
        {
            try
            {
                currentProcess = null;
                baseAddress = IntPtr.Zero;
                moduleSize = 0;
                mem.Detach();
            }
            finally
            {
                isAttached = false;
            }
            OnDetach?.Invoke();
        }
    }
}
