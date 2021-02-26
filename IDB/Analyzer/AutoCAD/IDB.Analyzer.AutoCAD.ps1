#=============================================================================#
# PowerShell script for coolOrange IDB.Analyzer.AutoCAD                       #
# Starts the AutoCAD accoreconsole.exe, loads the IDB.Analyzer.AutoCAD.dll    #
# and executes the analysis of AutoCAD references                             #
#                                                                             #
# Version: 1.1.0                                                              #
#=============================================================================#

#!!! IMPORTANT: MAKE SURE THAT THE LOCATION OF THE  TOOL (folder of file IDB.Analyzer.AutoCAD.dll) !!!
#!!! HAS BEEN ADDED TO THE "Trusted Locations" IN THE AUTOCAD SETTINGS !!!

# Settings--------------------------------------------------------------------#
$workingDirectory = "C:\Temp\IDBAnalyze\AcadXrefAnalysis"
$scriptfile = "$workingDirectory\IDB.Analyze.AutoCAD.scr" 
$batchfile = "$workingDirectory\IDB.Analyze.AutoCAD.bat"
$accoreconsole = "C:\Program Files\Autodesk\AutoCAD 2020\accoreconsole.exe"
$xrefAnalysisMode = "NORMAL"
# ----------------------------------------------------------------------------#

# create working folder
$workingFolder = New-Item -ItemType Directory -Path $workingDirectory -Force

# create script file for accoreconsole
';; Analyze AutoCAD Xrefs' | Out-File $scriptfile
'(command "_.netload" "{0}\\IDB.Analyzer.AutoCAD.dll")' -f $PSScriptRoot.Replace("\", "\\") | Out-File -Append $scriptfile
'(command "IDBAnalyzeDocs" "{0}")' -f $xrefAnalysisMode | Out-File -Append $scriptfile

# create and start batch file
'"{0}" /i "{1}" /s "{2}"' -f $accoreconsole, $file.LocalPath, $scriptfile | Out-File -Encoding ascii $batchfile
'pause' | Out-File -Encoding ascii -Append $batchfile

$p = Start-Process $batchfile -wait