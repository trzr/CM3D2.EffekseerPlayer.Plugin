@setlocal enabledelayedexpansion

@set REGKEY=カスタムオーダーメイド3D2
@set NAMEKEY=COM3D2
@set OPTS=/define:UNITY_5_6_OR_NEWER;UNITY_5_5_OR_NEWER;UNITY_5_4_OR_NEWER;COM3D2;

@call %~dp0\compile_base.bat

@rem @move /Y %OUT_NAME% %UI_DIR%
@rem @if exist %UI_DIR%\DynamicPlugins\%OUT_NAME% (
@rem   @del /Q %UI_DIR%\DynamicPlugins\%OUT_NAME%
@rem )
@rem @explorer %UI_DIR%

@endlocal

@pause
