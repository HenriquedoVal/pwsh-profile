### Compiled PowerShell Profile

Just an example of how to create a binary module that will act as the script profile but faster.  
  
Other things that made my startup faster too:
- functions defined on `$profile` that wasn't often used were moved to a separate directory under `$env:path`.
- functions that are always used became binary Cmdlets (which consists of [ShellServer](https://github.com/HenriquedoVal/shellserver)).

So the `$profile` became:
~~~PowerShell
Import-Module ShellServer
Import-Module BinProfile
~~~
