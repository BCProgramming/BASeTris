; example2.nsi
;
; This script is based on example1.nsi, but it remember the directory, 
; has uninstall support and (optionally) installs start menu shortcuts.
;
; It will install example2.nsi into a directory that the user selects,

;--------------------------------
!include "x64IconHack.nsi"
!define PROGPATH "..\bin\Release"
!define PROGFILE "BASeTris"

!tempfile INC_FILE
!system '"!NSIS Version Getter.exe" "${PROGPATH}\${PROGFILE}".exe "${INC_FILE}"'
!include ${INC_FILE}

!undef INC_FILE



!define FULLNAME "${COMPANY}\${PACKAGE}\${PROGFILE}"

!define INSTNAME "BASeTris (${PROGDATE}).exe"

; The file to write
OutFile "${INSTNAME}"

; The name of the installer
Name "${PROGFILE}"
Icon "BASeTris.ico"

; Version and copyright information.
!searchparse /noerrors "${PROGVER}" '' V1 '.' V2 '.' V3 '.' V4
!if "${V4}" == ""
	VIProductVersion "${PROGVER}.0"
!else
	VIProductVersion "${PROGVER}"
!endif
VIAddVersionKey "ProductName" "${PROGFILE}"
VIAddVersionKey "ProductVersion" "${PROGVER}"
VIAddVersionKey "ProductDate" "${PROGDATE}"
VIAddVersionKey "CompanyName" "${COMPANY}"
VIAddVersionKey "LegalCopyright" "${COPYRIGHT}"
VIAddVersionKey "FileVersion" "${PROGVER}"
VIAddVersionKey "FileDescription" "${PROGNAME} Installer"
VIAddVersionKey "OriginalFilename" "${INSTNAME}"

; The default installation directory
InstallDir "$PROGRAMFILES\${FULLNAME}"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\${FULLNAME}" "Install_Dir"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

AutoCloseWindow true

;--------------------------------

; Pages

Page components
Page directory
Page instfiles "" "" InstComplete

UninstPage uninstConfirm
UninstPage instfiles

;--------------------------------





; The stuff to install
Section "${PROGFILE} (required)"

  SectionIn RO
  ReadRegStr $R0 HKLM \
   "Software\${FULLNAME}" \
   "Install_Dir"
  
   StrCmp $R0 "" done uninst

 ;Run the uninstaller
uninst:

  ExecWait '"$INSTDIR\uninstall.exe" /S _?=$INSTDIR'
Delete "$INSTDIR\uninstall.exe"
done:


  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  ; Put file there
  File "${PROGPATH}\*.dll"
  File "${PROGPATH}\*.exe"
  File "${PROGPATH}\*.pdb"
  File "${PROGPATH}\*.exe.config"


  ; Write the installation path into the registry
  WriteRegStr HKLM "Software\${FULLNAME}" "Install_Dir" "$INSTDIR"
  
  
  WriteUninstaller "uninstall.exe"
  





SectionEnd
SectionGroup /e "Shortcuts"
  Section "Start Menu"
    SectionIn RO
    SetShellVarContext all
    Delete "$DESKTOP\${PROGNAME}.lnk"
    CreateDirectory "$SMPROGRAMS\${FULLNAME}"
    
    CreateShortCut "$SMPROGRAMS\${FULLNAME}\${PROGNAME}.lnk" "$INSTDIR\${PROGNAME}.exe" "" "$INSTDIR\${PROGNAME}.exe" 0
	${lnkX64IconFix} "$SMPROGRAMS\${FULLNAME}\${PROGNAME}.lnk"

    
  SectionEnd
SectionGroupEnd

; Optional section (can be disabled by the user)

;--------------------------------

; Uninstaller

Section "Uninstall"
  SetShellVarContext all
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FULLNAME}"
  DeleteRegKey /ifempty HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANY}\${PACKAGE}"
  DeleteRegKey /ifempty HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${COMPANY}"
  DeleteRegKey HKLM "Software\${FULLNAME}"
  DeleteRegKey /ifempty HKLM "Software\${COMPANY}\${PACKAGE}"
  DeleteRegKey /ifempty HKLM "Software\${COMPANY}"

  ; Remove files and uninstaller
  Delete "$INSTDIR\*.exe"
  Delete "$INSTDIR\*.dll"
  Delete "$INSTDIR\*.pdb"
  Delete "$INSTDIR\*.xml"
  Delete "$INSTDIR\*.exe.config"
  
  ; Remove directories.  Not doing this recursively though so we'll need to break out of the defined variables
  ; (which have multi-level paths).

  RMDir "$INSTDIR"
  RMDir "$PROGRAMFILES\${COMPANY}\${PACKAGE}"
  RMDir "$PROGRAMFILES\${COMPANY}"

  ; Remove shortcuts, if any
  Delete "$DESKTOP\${PROGNAME}.lnk"
  Delete "$SMPROGRAMS\${FULLNAME}\*.*"
  Delete "$QUICKLAUNCH\${PROGNAME}.lnk"
  ; Remove directories.  Again, with some hard-coding needed.
  RMDir "$SMPROGRAMS\${FULLNAME}"
  RMDir "$SMPROGRAMS\${COMPANY}\${PACKAGE}"
  RMDir "$SMPROGRAMS\${COMPANY}"

  ;MessageBox MB_YESNO|MB_ICONQUESTION "Delete user settings?" IDNO NoDelete
  ;  Delete "$APPDATA\${PROGNAME}\*.*"
  ;  RMDir "$APPDATA\${PROGNAME}" ; skipped if no
  ;NoDelete:
SectionEnd

Function InstComplete

  NoRun:
FunctionEnd
Function .onInit

FunctionEnd