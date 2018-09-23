@rem @setlocal enabledelayedexpansion

@set BASE_DIR_=C:\KISS\%NAMEKEY%\
@set "INSTALL_PATH_REG_KEY=HKCU\Software\KISS\%REGKEY%"
@set "INSTALL_PATH_REG_VALUE=InstallPath"
@for /F "usebackq skip=2 tokens=1-2*" %%A in (`REG QUERY %INSTALL_PATH_REG_KEY% /v %INSTALL_PATH_REG_VALUE% 2^>nul`) do (
  @set "BASE_DIR=%%C"
)

@if not exist "%BASE_DIR%\GameData\csv.arc" (
  @set "BASE_DIR="
)

@if not exist "%BASE_DIR%" (
  @if not exist "%BASE_DIR_%" (
    @echo "正しい%NAMEKEY%のインストールディレクトリを設定してください。指定されたパスは存在しません。：%BASE_DIR%"
    @goto end
  ) else (
    @set BASE_DIR=%BASE_DIR_%
  )
)
@echo "Target: %BASE_DIR%"

@set MANAGED=%BASE_DIR%%NAMEKEY%x64_Data\Managed
@if "%NAMEKEY%" == "COM3D2" (
  @set LOADER_DIR=%BASE_DIR%Sybaris
  @set UI_DIR=%BASE_DIR%Sybaris\UnityInjector
) else (
  @set LOADER_DIR=%BASE_DIR%Sybaris\Loader
  @set UI_DIR=%BASE_DIR%Sybaris\Plugins\UnityInjector
)

@set OUT_NAME=%NAMEKEY%.EffekseerPlayer.Plugin.dll

@set OPTS=%OPTS% /noconfig /optimize+ /nologo /nostdlib+
@set OPTS=%OPTS% /t:library /lib:%MANAGED% /r:UnityEngine.dll /r:UnityEngine.UI.dll /r:Assembly-CSharp.dll /r:Assembly-CSharp-firstpass.dll

@if exist "%LOADER_DIR%" (
  @set OPTS=%OPTS% /lib:%LOADER_DIR% /r:UnityInjector.dll /r:ExIni.dll
) else (
  @set OPTS=%OPTS% /r:UnityInjector.dll /r:ExIni.dll
)

@if "%UI_OPTS%" neq "" (
  @set OPTS=%OPTS% /lib:%UI_DIR% %UI_OPTS%
)

@rem @set OPTS=%OPTS% /nowarn:618,168
@rem @set OPTS=%OPTS% /define:DEBUG

@rem @set OPTS=%OPTS% /r:%windir%\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll
@rem @set OPTS=%OPTS% /r:%windir%\Microsoft.NET\Framework\v2.0.50727\System.dll
@set OPTS=%OPTS% /r:%MANAGED%\mscorlib.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Core.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Data.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Xml.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Xml.Linq.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Drawing.dll
@set OPTS=%OPTS% /r:%MANAGED%\System.Windows.Forms.dll
@rem @set OPTS=%OPTS% /r:%MANAGED%\UnityEngine.dll
@rem @set OPTS=%OPTS% /r:%MANAGED%\UnityEngine.UI.dll

@set OPTS=%OPTS% /resource:Resource\check_on16.png,check_on.png
@set OPTS=%OPTS% /resource:Resource\check_off16.png,check_off.png
@set OPTS=%OPTS% /resource:Resource\check_part16.png,check_part.png
@set OPTS=%OPTS% /resource:Resource\node_plus16.png,node_plus.png
@set OPTS=%OPTS% /resource:Resource\node_minus16.png,node_minus.png
@set OPTS=%OPTS% /resource:Resource\frame16.png,frame.png
@set OPTS=%OPTS% /resource:Resource\delete16.png,delete.png
@set OPTS=%OPTS% /resource:Resource\pause16.png,pause.png
@set OPTS=%OPTS% /resource:Resource\play16.png,play.png
@set OPTS=%OPTS% /resource:Resource\reload16.png,reload.png
@set OPTS=%OPTS% /resource:Resource\repeat16.png,repeat.png
@set OPTS=%OPTS% /resource:Resource\repeat_off16.png,repeat_off.png
@set OPTS=%OPTS% /resource:Resource\stop16.png,stop.png
@set OPTS=%OPTS% /resource:Resource\stopr16.png,stopr.png
@set OPTS=%OPTS% /resource:Resource\circle16.png,circle.png
@set OPTS=%OPTS% /resource:Resource\cross16.png,cross.png
@set OPTS=%OPTS% /resource:Resource\circle20.png,preset_base.png
@set OPTS=%OPTS% /resource:Resource\empty20.png,preset_empty.png
@set OPTS=%OPTS% /resource:Resource\focus20.png,preset_focus.png
@set OPTS=%OPTS% /resource:Resource\copy24.png,copy.png
@set OPTS=%OPTS% /resource:Resource\paste24.png,paste.png

@rem @set CSC=%windir%\Microsoft.NET\Framework\v3.5\csc
@set CSC=%windir%\Microsoft.NET\Framework\v4.0.30319\csc

@if exist %OUT_NAME% (del %OUT_NAME%)
@%CSC% %OPTS% /out:%OUT_NAME% *.cs /recurse:Util\*.cs /recurse:Unity\*.cs /recurse:Effekseer\*.cs /recurse:CM3D2\*.cs %POST_OPTS%

@rem MOVE /Y  %OUT_NAME% %UI_DIR%
@if exist %OUT_NAME% (@echo.%OUT_NAME%を出力しました)

:end
@rem @pause
@rem @endlocal