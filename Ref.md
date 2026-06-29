LOGS:
------------------------------------------------------------------------------
You may only use the Microsoft Visual Studio .NET/C/C++ Debugger (vsdbg) with
Visual Studio Code, Visual Studio or Visual Studio for Mac software to help you
develop and test your applications.
------------------------------------------------------------------------------
MTM_Receiving_Application.exe (14888): Loaded 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App\10.0.9\System.Private.CoreLib.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Users\jkoll\source\repos\MTM_Receiving_Application\bin\x64\Debug\net10.0-windows10.0.22621.0\MTM_Receiving_Application.dll'. Symbols loaded.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App\10.0.9\System.Runtime.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App\10.0.9\System.Runtime.InteropServices.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Users\jkoll\source\repos\MTM_Receiving_Application\bin\x64\Debug\net10.0-windows10.0.22621.0\WinRT.Runtime.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App\10.0.9\System.Collections.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App\10.0.9\System.Collections.Concurrent.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Users\jkoll\source\repos\MTM_Receiving_Application\bin\x64\Debug\net10.0-windows10.0.22621.0\Microsoft.WinUI.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App\10.0.9\System.Threading.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App\10.0.9\System.Memory.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
MTM_Receiving_Application.exe (14888): Loaded 'C:\Program Files\dotnet\shared\Microsoft.NETCore.App\10.0.9\System.Runtime.CompilerServices.Unsafe.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
Exception thrown: 'System.Runtime.InteropServices.COMException' in System.Private.CoreLib.dll

ERROR:
Exception has occurred: CLR/System.Runtime.InteropServices.COMException
An unhandled exception of type 'System.Runtime.InteropServices.COMException' occurred in System.Private.CoreLib.dll: 'Class not registered (0x80040154 (REGDB_E_CLASSNOTREG))'
   at System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(Int32 errorCode)
   at WinRT.ActivationFactory.Get(String typeName, Guid iid)
   at Microsoft.UI.Xaml.Application.get__objRef_global__Microsoft_UI_Xaml_IApplicationStatics()
   at Microsoft.UI.Xaml.Application.Start(ApplicationInitializationCallback callback)
   at MTM_Receiving_Application.Program.Main(String[] args) in C:\Users\jkoll\source\repos\MTM_Receiving_Application\obj\x64\Debug\net10.0-windows10.0.22621.0\App.g.i.cs:line 26

