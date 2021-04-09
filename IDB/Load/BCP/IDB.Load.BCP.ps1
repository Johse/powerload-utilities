Import-Module .\powerLoad.psm1

#Transform BCP Vault.xml into CSV Files
New-Transform -XmlPath .\Vault.xml

#importing folders csv file into the database
Import-FolderCsvToSql -FolderUdpXmlPath "C:\TEMP\load test\ExportFolderUDP.xml" -VaultFoldersCsvPath "C:\TEMP\load test\VaultFolders.csv" -Server local -Database load1

#importing files csv file into the database
Import-FilesCsvToSql -FileUdpXmlPath "C:\TEMP\load test\ExportFileUDP.xml" -VaultFilesCsvPath "C:\TEMP\load test\VaultFilesUDP.csv" -Server local -Database load1

#importing file file relations csv file into the database
Import-FileFileRelationsCsvToSql -FileFileRelationsCsvPath "C:\TEMP\load test\FileFileRelations.csv" -Server local -Database load1


