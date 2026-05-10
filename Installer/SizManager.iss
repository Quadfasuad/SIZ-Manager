; СИЗ Менеджер — Inno Setup Script
; Requires Inno Setup 6.0+ (https://jrsoftware.org/isinfo.php)

#define MyAppName "СИЗ Менеджер"
#define MyAppNameEn "SIZ Manager"
#ifndef MyAppVersion
  #define MyAppVersion "2.0.0"
#endif
#define MyAppPublisher "SIZ Manager"
#define MyAppExeName "SizManager.exe"

; Change this path to where dotnet publish output is located
#define PublishDir "..\SizManager\bin\Release\net8.0-windows\win-x64\publish"

[Setup]
AppId={{B8A3F2E1-5C7D-4A9B-8E6F-1D2C3B4A5E6F}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppNameEn}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=Output
OutputBaseFilename=SizManager_Setup_{#MyAppVersion}_x64
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
MinVersion=6.1sp1
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayName={#MyAppName}
SetupIconFile=..\siz.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main application files (from dotnet publish output)
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\Удалить {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Запустить {#MyAppName}"; Flags: nowait postinstall skipifsilent

[Code]
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  DataDir: string;
  MsgResult: Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    DataDir := ExpandConstant('{userappdata}\SizManager');
    if DirExists(DataDir) then
    begin
      MsgResult := MsgBox(
        'Удалить данные программы (база данных, резервные копии)?'#13#10 +
        'Путь: ' + DataDir,
        mbConfirmation, MB_YESNO or MB_DEFBUTTON2);
      if MsgResult = IDYES then
        DelTree(DataDir, True, True, True);
    end;
  end;
end;
