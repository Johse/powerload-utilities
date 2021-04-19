# idb-load-bcp

[![Windows](https://img.shields.io/badge/Platform-Windows-lightgray.svg)](https://www.microsoft.com/en-us/windows/)
[![.NET](https://img.shields.io/badge/.NET%20Framework-4.7-blue.svg)](https://dotnet.microsoft.com/)
[![Vault](https://img.shields.io/badge/Autodesk%20Vault%20DTU-2020-yellow.svg)](https://www.autodesk.com/products/vault/)

[![bcpToolkit](https://img.shields.io/badge/coolOrange%20bcpToolkit-20-orange.svg)](https://www.coolorange.com/en-eu/load.html#bcpToolkit)

## Disclaimer

THE SAMPLE CODE ON THIS REPOSITORY IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.

THE USAGE OF THIS SAMPLE IS AT YOUR OWN RISK AND **THERE IS NO SUPPORT** RELATED TO IT.

## Description

Utility to load files from Vault.xml to the Intermediate Database (IDB)

Configuration

In the configuration file IDB.Load.BCP.Behaviors.config.xml the default behaviours can be set. 
![image](https://user-images.githubusercontent.com/62716091/81201175-43614980-8fc5-11ea-941f-f8c41eb5958f.png)


ConnectionString: connect string to SQL server and database.
DataPath: Folderpath that contains Vault.xml file. All files, folders, file-file relations will be transferred to the IDB.

The elements must not be removed. Please don`t rename files.

## Usage
 
Start the tool with double click the file IDB.Load.BCP.exe.
In the open dialog specify the Path to can and import and SQL Database Connection String. 
* ‘Save’: The specified path and connect string are written back to file IDB.Load.BCP.Behaviors.config.xml.
* ‘Refresh’: Updating the path and connect string from the file IDB.Load.BCP.Behaviors.config.xml.
* ‘Start’: Start scan and import.
* ‘Cancel’: Stop the process. After clicking the button all records and unsaved data will be lost.
* ‘Reset’: Initialize for next run.
* ‘Insert Item’: Insert items from ItemsWrapper.xml. 
* ‘Scan records’: File`s counter of entered folder path.
* ‘Import records’: Counter of already inserted files.

After starting the process program scan Vault.xml. It can take some time.
![image](https://user-images.githubusercontent.com/62716091/81200744-af8f7d80-8fc4-11ea-8e11-f31c601efa37.png)
  
## Logging
In the same folder as the exe a log file IDB.Load.BCP.log is created. There you find information about successful inserts and errors. 

## Caution
If you want to load several bcp-packages into 1 Intermediate Database (IDB) you must remove the UNIQUE for the index [IX_Files] in order the tool does not stop when duplicate files are imported.

## Product Documentation

[coolOrange bcpToolkit](https://www.coolorange.com/wiki/doku.php?id=bcptoolkit)

## Author
coolOrange s.r.l.

![coolOrange](https://user-images.githubusercontent.com/36075173/46519882-4b518880-c87a-11e8-8dab-dffe826a9630.png)

