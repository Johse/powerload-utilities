# IDB.Load.Files

[![bcpToolkit](https://img.shields.io/badge/coolOrange%20powerLoad-21-orange.svg)](https://www.coolorange.com/products/powerload/)

## Disclaimer

THE SAMPLE CODE ON THIS REPOSITORY IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.
THE USAGE OF THIS SAMPLE IS AT YOUR OWN RISK AND **THERE IS NO SUPPORT** RELATED TO IT.

## Description

Utility to load files from Windows folders to the Intermediate Database (IDB)

Prerequisite

* Microsoft SQL Server
* Windows 10

In the SQL Database (IDB) modify
* In table Files allow Null values for field Version.
* In table Folders remove Unique key constraint at Index name IX_Folders.

Configuration

In the configuration file DefaultBehaviors.xml the default behaviours can be set. 

![image](https://user-images.githubusercontent.com/62716091/81202972-a05dff00-8fc7-11ea-9a4e-c1ce65170e65.png)


ConnectionString: connect string to SQL server and database

DataPath: Folderpath that will be scanned. All names of subfolders and files will be transferred to the IDB.

Folders: Default settings for folder. The following fields in the IDB will be filled with the assigned value for all folders.
* Category
* CreateUser 

Files: Default settings for files. The following fields in the IDB will be filled with the assigned value for all files.
* Category
* RevisionDefinition
* LifeCycleState
* LifeCycleDefinition
* RevisionLabel
* Classification
* CreateUser
The elements must not be removed. To not fill the field just delete the value.
 
E.g. < CreateUser >< /CreateUser >

Please don`t rename XML files.

## Usage
 
Start the tool with double click the file IDB.Load.Files.exe.
In the open dialog specify the �Path� and SQL Database �Connection String� to import.

* Save: The specified path and connect string are written back to file DefaultBehaviors.xml.
* Refresh: Updating the path and connect string from the file DefaultBehaviors.xml.
* Start: Start scan and import
* Cancel: Stop the process. After clicking the button all records and unsaved data will be lost.
* Reset: Initialize for next run
* Scan records: File`s counter of entered folder path
* Import records: Counter of already inserted files

![image](https://user-images.githubusercontent.com/62716091/81194971-89b2aa80-8fbd-11ea-8374-c282ad0bbc2d.png)


## Logging

In the same folder as the exe a log file IDB.Load.Files.log is created. There you find information about successful inserts and errors. 

## Product Documentation

[coolOrange bcpToolkit](https://www.coolorange.com/wiki/doku.php?id=bcptoolkit)