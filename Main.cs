using System;
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
                "Microsoft.PowerShell.PSReadLine2"
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

            // Set-PSReadlineKeyHandler -Key Tab -Function MenuComplete
            // Set-Alias -Name hist -Value Search-ShellServerHistory
            // Set-Alias -Name swopt -Value Switch-ShellServerOptions
            // Set-Alias -Name swtheme -Value Switch-ShellServerTheme

            var pwsh = PowerShell.Create(RunspaceMode.CurrentRunspace);
            pwsh.AddCommand("Set-PSReadlineKeyHandler")
                .AddParameter("Key", "Tab")
                .AddParameter("Function", "MenuComplete")
                .AddStatement()

                .AddCommand("Set-Alias")
                .AddParameter("Name", "hist")
                .AddParameter("Value", "Search-ShellServerHistory")
                .AddStatement()

                .AddCommand("Set-Alias")
                .AddParameter("Name", "swopt")
                .AddParameter("Value", "Switch-ShellServerOptions")
                .AddStatement()

                .AddCommand("Set-Alias")
                .AddParameter("Name", "swtheme")
                .AddParameter("Value", "Switch-ShellServerTheme")
                .Invoke();

            pwsh.Dispose();

            string current_path = Environment.GetEnvironmentVariable("path");
            if (current_path is null) return;

            string to_add = Environment.ExpandEnvironmentVariables("%userprofile%\\ps_scripts");
            foreach (string path in current_path.Split(';'))
            {
                if (path == to_add) return;
            }

            string semi = current_path[current_path.Length - 1] == ';' ? "" : ";";
            Environment.SetEnvironmentVariable("path", $"{current_path}{semi}{to_add}");
        }
    }
}
