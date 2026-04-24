using System;
using System.Diagnostics;
using System.Management.Automation;

namespace MyConfig
{
    public class Init : IModuleAssemblyInitializer
    {
        public void OnImport()
        {
            Type PSConsoleReadLine = null;
            Type SetPSReadLineOption = null;

            var Asm = AppDomain.CurrentDomain.Load(
                "Microsoft.PowerShell.PSReadLine"
            );

            PSConsoleReadLine = Asm.GetType(
                    "Microsoft.PowerShell.PSConsoleReadLine"
            );
            SetPSReadLineOption = Asm.GetType(
                    "Microsoft.PowerShell.SetPSReadLineOption"
            );

            if (
                    PSConsoleReadLine is null
                    || SetPSReadLineOption is null
            )
                throw new Exception("Types not found");

            var inst = Activator.CreateInstance(SetPSReadLineOption);

            const int Vi = 2;
            var Prop = SetPSReadLineOption.GetProperty("EditMode");
            Prop.SetValue(inst, Vi);

            const int ListView = 1;
            Prop = SetPSReadLineOption.GetProperty("PredictionViewStyle");
            Prop.SetValue(inst, ListView);

            Prop = SetPSReadLineOption.GetProperty("MaximumHistoryCount");
            Prop.SetValue(inst, 12288);

            const int script = 3;
            Prop = SetPSReadLineOption.GetProperty("ViModeIndicator");
            Prop.SetValue(inst, script);

            var modeChange = ScriptBlock.Create(
                @"if ($args[0] -eq 'Command') {
    Write-Host -NoNewLine ""`e[1 q""
} else {
    Write-Host -NoNewLine ""`e[5 q""
}"
            );
            Prop = SetPSReadLineOption.GetProperty("ViModeChangeHandler");
            Prop.SetValue(inst, modeChange);

            Prop = SetPSReadLineOption.GetProperty("ContinuationPrompt");
            Prop.SetValue(inst, "> ");

            var SetOptions = PSConsoleReadLine.GetMethod("SetOptions");

            SetOptions.Invoke(null, new object[] {inst});

            var pwsh = PowerShell.Create(RunspaceMode.CurrentRunspace);
            pwsh.AddCommand("Set-PSReadlineKeyHandler")
                .AddParameter("Key", "Tab")
                .AddParameter("Function", "MenuComplete")

                // .AddStatement()
                // .AddCommand("Set-Alias")
                // .AddParameter("Name", "hist")
                // .AddParameter("Value", "Search-ShellServerHistory")

                .Invoke();

            pwsh.Dispose();

            Environment.SetEnvironmentVariable("PAGER", "less");

            Environment.SetEnvironmentVariable("PY_PYTHON", "3.13");
            Environment.SetEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en");
            Environment.SetEnvironmentVariable("VCPKG_ROOT", "A:\\Dev\\vcpkg");

            string[] paths_expand_path = {
                Environment.ExpandEnvironmentVariables("%userprofile%\\ps_scripts"),
                Environment.ExpandEnvironmentVariables("%VCPKG_ROOT%"),
                "A:\\Dev\\Odin",
                "D:\\henri\\programs\\unmanaged",
                "C:\\Program Files\\Windows Defender",
                "C:\\Program Files\\OpenSSL-Win64\\bin",
            };

            foreach (string p in paths_expand_path)
                add_to_path(p);
        }

        void add_to_path(string s) {
            string path = Environment.GetEnvironmentVariable("PATH");
            Debug.Assert(path is not null);

            foreach (string p in path.Split(';')) {
                if (p == s) {
                    return;
                }
            }

            string semi = path.EndsWith(';') ? "" : ";";
            Environment.SetEnvironmentVariable("PATH", $"{path}{semi}{s}");
        }
    }
}
