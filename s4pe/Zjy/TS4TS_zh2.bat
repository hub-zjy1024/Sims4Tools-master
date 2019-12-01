SET out1=%~dpnx1
SET outDir=%out1%_zh
echo outDir=%outDir%

TS4TranslateSupporter.exe -b2a "%out1%\zh_exported.xml" "%out1%"  "%outDir%"

pause