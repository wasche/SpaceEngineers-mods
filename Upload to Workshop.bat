set SEInstallDir="H:\Steam\steamapps\common\SpaceEngineers"
for %%I in (.) do set ParentDirName=%%~nxI
%SEInstallDir%\Bin64\SEWorkshopTool.exe --upload --compile --mods "%ParentDirName%" --exclude .bat .psd
pause