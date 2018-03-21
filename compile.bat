@setlocal enabledelayedexpansion

@set "INSTALL_PATH_REG_KEY=HKCU\Software\KISS\カスタムオーダーメイド3D2"
@set "INSTALL_PATH_REG_VALUE=InstallPath"
@for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %INSTALL_PATH_REG_KEY% /v %INSTALL_PATH_REG_VALUE% 2^>nul`) do (
  @set "BASE_DIR=%%C"
)

@if not exist "%BASE_DIR%\GameData\csv.arc" (
  @set "BASE_DIR="
)

@SET BASE_DIR_=C:\Games\KISS\COM3D2

@if not exist "%BASE_DIR%" (
  @if not exist "%BASE_DIR_%" (
    @echo "正しいCOM3D2のインストールディレクトリを設定してください。指定されたパスは存在しません。：%BASE_DIR%"
    @goto end
  ) else (
    @SET BASE_DIR=%BASE_DIR_%
  )
)
@echo "target: %BASE_DIR%"

@SET MANAGED=%BASE_DIR%\COM3D2x64_Data\Managed
@SET LOADER_DIR=%BASE_DIR%\Sybaris
@SET UI_DIR=%BASE_DIR%\Sybaris\UnityInjector

@SET OUT_NAME=COM3D2.EffekseerPlayer.Plugin.dll
@SET OPTS=%OPTS% /define:UNITY_5_6_OR_NEWER;UNITY_5_5_OR_NEWER;COM3D2;
@SET OPTS=%OPTS% /noconfig /optimize+ /nologo /nostdlib+
@SET OPTS=%OPTS% /t:library /lib:%MANAGED% /r:UnityEngine.dll /r:UnityEngine.UI.dll /r:JsonFx.Json.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll

@if exist "%LOADER_DIR%" (
  @SET OPTS=%OPTS% /lib:%LOADER_DIR% /r:UnityInjector.dll /r:ExIni.dll
) else (
  @SET OPTS=%OPTS% /r:UnityInjector.dll /r:ExIni.dll
)
@rem @SET OPTS=%OPTS% /lib:%UI_DIR% /r:CM3D2.DynamicLoader.Plugin.dll

@rem @SET OPTS=%OPTS% /nowarn:618,168
@rem @SET OPTS=%OPTS% /define:DEBUG

@rem @SET OPTS=%OPTS% /r:%windir%\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll
@rem @SET OPTS=%OPTS% /r:%windir%\Microsoft.NET\Framework\v2.0.50727\System.dll
@SET OPTS=%OPTS% /r:%MANAGED%\mscorlib.dll
@SET OPTS=%OPTS% /r:%MANAGED%\System.dll
@SET OPTS=%OPTS% /r:%MANAGED%\System.Core.dll
@SET OPTS=%OPTS% /r:%MANAGED%\System.Data.dll
@SET OPTS=%OPTS% /r:%MANAGED%\System.Xml.dll
@SET OPTS=%OPTS% /r:%MANAGED%\System.Xml.Linq.dll
@SET OPTS=%OPTS% /r:%MANAGED%\System.Drawing.dll
@SET OPTS=%OPTS% /r:%MANAGED%\System.Windows.Forms.dll
@rem @SET OPTS=%OPTS% /r:%MANAGED%\UnityEngine.dll
@rem @SET OPTS=%OPTS% /r:%MANAGED%\UnityEngine.UI.dll

@SET OPTS=%OPTS% /resource:Resource\check_on16.png,check_on.png
@SET OPTS=%OPTS% /resource:Resource\check_off16.png,check_off.png
@SET OPTS=%OPTS% /resource:Resource\check_part16.png,check_part.png
@SET OPTS=%OPTS% /resource:Resource\node_plus16.png,node_plus.png
@SET OPTS=%OPTS% /resource:Resource\node_minus16.png,node_minus.png
@SET OPTS=%OPTS% /resource:Resource\frame16.png,frame.png
@SET OPTS=%OPTS% /resource:Resource\delete16.png,delete.png
@SET OPTS=%OPTS% /resource:Resource\pause16.png,pause.png
@SET OPTS=%OPTS% /resource:Resource\play16.png,play.png
@SET OPTS=%OPTS% /resource:Resource\reload16.png,reload.png
@SET OPTS=%OPTS% /resource:Resource\repeat16.png,repeat.png
@SET OPTS=%OPTS% /resource:Resource\repeat_off16.png,repeat_off.png
@SET OPTS=%OPTS% /resource:Resource\stop16.png,stop.png
@SET OPTS=%OPTS% /resource:Resource\stopr16.png,stopr.png

@rem @SET CSC=%windir%\Microsoft.NET\Framework\v3.5\csc
@SET CSC=%windir%\Microsoft.NET\Framework\v4.0.30319\csc

@%CSC% %OPTS% /out:%OUT_NAME%  *.cs /recurse:Util\*.cs /recurse:Unity\*.cs /recurse:Effekseer\*.cs  /recurse:CM3D2\*.cs
@rem MOVE /Y  %OUT_NAME% %UI_DIR%
@if exist %OUT_NAME% @echo.%OUT_NAME%を出力しました

@if "%1" == "MOVE" (MOVE /Y  %OUT_NAME% %UI_DIR%)


:end
@rem @pause
@endlocal