;NSIS Modern User Interface

;--------------------------------
;Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
;General

  ;Name and file
  Name "Tracer2Server"
  
  !getdllversion "..\..\src\bin\Release\Tracer2Server.exe" expv_
  OutFile "Tracer2Server_Setup_${expv_1}.${expv_2}.exe"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\Tracer2Server"
  
  ;Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\Tracer2Server" ""

  ;Request application privileges for Windows Vista
  RequestExecutionLevel Highest

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING

  ;Show all languages, despite user's codepage
  !define MUI_LANGDLL_ALLLANGUAGES

;--------------------------------
;Language Selection Dialog Settings

  ;Remember the installer language
  !define MUI_LANGDLL_REGISTRY_ROOT "HKCU" 
  !define MUI_LANGDLL_REGISTRY_KEY "Software\Tracer2Server" 
  !define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_LICENSE "..\..\src\bin\Release\GPL-v2.0.txt"
  ;!insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
;Languages

  !insertmacro MUI_LANGUAGE "English" ;first language is the default language
  !insertmacro MUI_LANGUAGE "French"
  !insertmacro MUI_LANGUAGE "German"
  !insertmacro MUI_LANGUAGE "Spanish"
  !insertmacro MUI_LANGUAGE "SpanishInternational"
  !insertmacro MUI_LANGUAGE "SimpChinese"
  !insertmacro MUI_LANGUAGE "TradChinese"
  !insertmacro MUI_LANGUAGE "Japanese"
  !insertmacro MUI_LANGUAGE "Korean"
  !insertmacro MUI_LANGUAGE "Italian"
  !insertmacro MUI_LANGUAGE "Dutch"
  !insertmacro MUI_LANGUAGE "Danish"
  !insertmacro MUI_LANGUAGE "Swedish"
  !insertmacro MUI_LANGUAGE "Norwegian"
  !insertmacro MUI_LANGUAGE "NorwegianNynorsk"
  !insertmacro MUI_LANGUAGE "Finnish"
  !insertmacro MUI_LANGUAGE "Greek"
  !insertmacro MUI_LANGUAGE "Russian"
  !insertmacro MUI_LANGUAGE "Portuguese"
  !insertmacro MUI_LANGUAGE "PortugueseBR"
  !insertmacro MUI_LANGUAGE "Polish"
  !insertmacro MUI_LANGUAGE "Ukrainian"
  !insertmacro MUI_LANGUAGE "Czech"
  !insertmacro MUI_LANGUAGE "Slovak"
  !insertmacro MUI_LANGUAGE "Croatian"
  !insertmacro MUI_LANGUAGE "Bulgarian"
  !insertmacro MUI_LANGUAGE "Hungarian"
  !insertmacro MUI_LANGUAGE "Thai"
  !insertmacro MUI_LANGUAGE "Romanian"
  !insertmacro MUI_LANGUAGE "Latvian"
  !insertmacro MUI_LANGUAGE "Macedonian"
  !insertmacro MUI_LANGUAGE "Estonian"
  !insertmacro MUI_LANGUAGE "Turkish"
  !insertmacro MUI_LANGUAGE "Lithuanian"
  !insertmacro MUI_LANGUAGE "Slovenian"
  !insertmacro MUI_LANGUAGE "Serbian"
  !insertmacro MUI_LANGUAGE "SerbianLatin"
  !insertmacro MUI_LANGUAGE "Arabic"
  !insertmacro MUI_LANGUAGE "Farsi"
  !insertmacro MUI_LANGUAGE "Hebrew"
  !insertmacro MUI_LANGUAGE "Indonesian"
  !insertmacro MUI_LANGUAGE "Mongolian"
  !insertmacro MUI_LANGUAGE "Luxembourgish"
  !insertmacro MUI_LANGUAGE "Albanian"
  !insertmacro MUI_LANGUAGE "Breton"
  !insertmacro MUI_LANGUAGE "Belarusian"
  !insertmacro MUI_LANGUAGE "Icelandic"
  !insertmacro MUI_LANGUAGE "Malay"
  !insertmacro MUI_LANGUAGE "Bosnian"
  !insertmacro MUI_LANGUAGE "Kurdish"
  !insertmacro MUI_LANGUAGE "Irish"
  !insertmacro MUI_LANGUAGE "Uzbek"
  !insertmacro MUI_LANGUAGE "Galician"
  !insertmacro MUI_LANGUAGE "Afrikaans"
  !insertmacro MUI_LANGUAGE "Catalan"
  !insertmacro MUI_LANGUAGE "Esperanto"
  !insertmacro MUI_LANGUAGE "Asturian"
  !insertmacro MUI_LANGUAGE "Pashto"
  !insertmacro MUI_LANGUAGE "ScotsGaelic"
!ifdef NSIS_UNICODE
  !insertmacro MUI_LANGUAGE "Georgian"
!endif

  VIProductVersion "${expv_1}.${expv_2}.${expv_3}.${expv_4}"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "Tracer2Server"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "A test comment"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" ""
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "Tracer2Server Setup"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${expv_1}.${expv_2}.${expv_3}.${expv_4}"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${expv_1}.${expv_2}.${expv_3}.${expv_4}"

;--------------------------------
;Reserve Files
  
  ;If you are using solid compression, files that are required before
  ;the actual installation should be stored first in the data block,
  ;because this will make your installer start faster.
  
  !insertmacro MUI_RESERVEFILE_LANGDLL

;--------------------------------
;Installer Sections

Section "Dummy Section" SecDummy
  SetShellVarContext All
  
  SetOutPath "$INSTDIR"
  File ..\..\src\bin\Release\Tracer2Server.exe
  File ..\..\src\bin\Release\GPL-v2.0.txt
  File ..\..\src\bin\Release\GPL-v3.0.txt
  File ..\..\src\bin\Release\README
  
  WriteRegStr HKCU "Software\Tracer2Server" "" $INSTDIR
  
  WriteUninstaller "$INSTDIR\Uninstall.exe"
SectionEnd

;--------------------------------
;Installer Functions

Function .onInit

  !insertmacro MUI_LANGDLL_DISPLAY

FunctionEnd

;--------------------------------
;Descriptions

  ;USE A LANGUAGE STRING IF YOU WANT YOUR DESCRIPTIONS TO BE LANGAUGE SPECIFIC

  ;Assign descriptions to sections
  ;!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  ;  !insertmacro MUI_DESCRIPTION_TEXT ${SecDummy} "A test section."
  ;!insertmacro MUI_FUNCTION_DESCRIPTION_END


; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\Tracer2Server"
  CreateShortCut "$SMPROGRAMS\Tracer2Server\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortCut "$SMPROGRAMS\Tracer2Server\Tracer2Server.lnk" "$INSTDIR\Tracer2Server.exe"
  
SectionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"
  SetShellVarContext All

  Delete $INSTDIR\Tracer2Server.exe
  Delete $INSTDIR\GPL-v2.0.txt
  Delete $INSTDIR\GPL-v3.0.txt
  Delete $INSTDIR\README
  Delete "$INSTDIR\Uninstall.exe"
  RMDir "$INSTDIR"
  
  Delete "$SMPROGRAMS\Tracer2Server\*.*"
  RMDir "$SMPROGRAMS\Tracer2Server"
  
  RMDir /r "$APPDATA\Tracer2Server"
  
  DeleteRegKey /ifempty HKCU "Software\Tracer2Server"
SectionEnd

;--------------------------------
;Uninstaller Functions

Function un.onInit

  !insertmacro MUI_UNGETLANGUAGE
  
FunctionEnd