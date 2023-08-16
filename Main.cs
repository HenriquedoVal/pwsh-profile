using System;
using System.Management.Automation;

namespace MyConfig
{
    public class Init : IModuleAssemblyInitializer
    {
        public void OnImport()
		{
			Type ReadLine = null;
			Type SetPSReadLineOption = null;

			var Asm = AppDomain.CurrentDomain.Load(
				"Microsoft.PowerShell.PSReadLine2"
			);

			foreach (Type t in Asm.GetExportedTypes())
			{
				if (t.Name == "PSConsoleReadLine") ReadLine = t;
				if (t.Name == "SetPSReadLineOption") SetPSReadLineOption = t;
			}

			if (
					ReadLine is null
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

			var SetOptions = ReadLine.GetMethod("SetOptions");

			SetOptions.Invoke(null, new object[] {inst});

			// Set-PSReadlineKeyHandler -Key Tab -Function MenuComplete
			// Set-Alias -Name hist -Value Search-ShellServerHistory
			// Set-Alias -Name swopt -Value Switch-ShellServerOptions
			// Set-Alias -Name swtheme -Value Switch-ShellServerTheme

			var pwsh = PowerShell.Create(RunspaceMode.CurrentRunspace);
			// pwsh.AddCommand("Set-PSReadlineKeyHandler")
			// 	.AddParameter("Key", "Tab")
			// 	.AddParameter("Function", "MenuComplete")
			// 	.AddStatement()

			pwsh.AddCommand("Set-Alias")
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


			// inst = Activator.CreateInstance(SetPSReadLineKeyHandlerCommand);
			// var field = SetPSReadLineKeyHandlerCommand.GetField("Key");
			// Console.WriteLine(field is null);
        }
    }
}
