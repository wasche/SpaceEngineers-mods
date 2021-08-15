set SEInstallDir="H:\Steam\steamapps\common\SpaceEngineers"
for %%I in (.) do set ParentDirName=%%~nxI
%SEInstallDir%\Bin64\SEWorkshopTool.exe --mods "%ParentDirName%" --tags mod other modpack
pause