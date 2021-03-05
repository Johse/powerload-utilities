# powerload-utilities

[![Windows](https://img.shields.io/badge/Platform-Windows-lightgray.svg)](https://www.microsoft.com/en-us/windows/)
[![.NET](https://img.shields.io/badge/.NET%20Framework-4.7.2-blue.svg)](https://dotnet.microsoft.com/)
[![Vault](https://img.shields.io/badge/Autodesk%20Vault%20DTU-2021-yellow.svg)](https://www.autodesk.com/products/vault/)

[![bcpToolkit](https://img.shields.io/badge/COOLORANGE%20powerLoad-21-orange.svg)](https://www.coolorange.com/products/powerLoad)

## Disclaimer

THE SAMPLE CODE ON THIS REPOSITORY IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.

THE USAGE OF THIS SAMPLE IS AT YOUR OWN RISK AND **THERE IS NO SUPPORT** RELATED TO IT.

## Overview
The powerLoad utilities are a set of utilities for loading the Autodesk Vault Workgroup or Professional which are based on the bcpToolkit.
The powerLoad utilites are:
* **powerLoad Intermediate Database (IDB):** SQL database for transforming the data to fit to the target Vault
* **IDB.Load.Files:** Utility to load files from a Windows folder to the Intermediate Database.
* **IDB.Load.BCP:** Utility to load files and folders from a BCP-package to the Intermediate Database (IDB).
* **IDB.Load.Vault:** Sample code to extract data from Vault and fill the Intermediate Database (IDB).
* **IDB.Analyzer.Inventor:** Scans Inventor files for missing references that are listed in the IDB in the field 'LocalFullFileName'. Additionally the RefID from the reference is extracted and written back to the IDB.
* **IDB.Analyzer.AutoCAD:** Scans AutoCAD DWGs files for missing Xrefs that are listed in the IDB in the field 'LocalFullFileName'. Additionally the RefID from the reference is extracted and written back to the IDB.
* **IDB.Discover.Vault:** Utility to query Vault for existing files and replace these files in the powerLoad Intermediate Database (IDB)
* **IDB.Translate.BCP:** Creates a valid BCP-package from the content of the Intermediate Database.

### Additions
For validating the BCP package that is created from the IDB there are additional tools:
* **bcpViewer**, which is part of the bcpToolkit
* **bcpValidator**, which can be found on https://github.com/coolOrangeLabs/bcpValidator

## Prerequsites
* Microsoft SQL Server 2014 or newer
* Windows 10
* AutoCAD and/or Inventor if the analyzer tools are needed

## Installation
Download the complete powerLoad package with the "Download ZIP" command from the "Code"-menu.

![Download powerLoad ZIP package](Images/PL-Download)

Extract the ZIP to a folder 'powerLoad' anywhere on your client machine.!


## Description

The Intermediate Database (IDB) is a concept that can be used to fill a neutral database with data from Vault or any other PDM system or from Windows Explorer. This IDB can be transformed into a BCP format which can be imported into Vault Professional or Vault Workgroup.   
This repository contains the IDB structure as well as tools to load the IDB and to convert the database to the Autodesk Vault BCP format. This format is finally used by Autodesk Vault DTU to mass import data into Vault.

## coolOrange powerLoad Intermediate Database
The powerLoad Intermediate Database for the Vault import (IDB) is a SQL database template provided by coolOrange that standardizes and simplifies the transition from an existing data and file source to Vault.
This section explains how to use and fill the Intermediate Database with the information from the legacy system.

### Setting up the Intermediate Database for Vault Import

1. Open SQL Management Studio and login
2. Open script Create_IntermediateDB.sql in SQL Management Studio
3. Modify path for mdf and ldf files for your SQL installation

    Sample:
    Modify ***FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.AUTODESKVAULT\MSSQL\DATA\Load.mdf'***
    to ***FILENAME = N'C:\Program Files\Microsoft SQL Server\<My SQL instance>\MSSQL\DATA\Load.mdf'***
 4. Create the coolOrange Intermediate Database with the sql script Create_IntermediateDB.sql. The default name of the database is "Load".
 5. Check and deactivate option *'Prevent saving changes that require table re-creation'* from the menu *"Tools > Options..."*. 
 
 ![SQL Options](DLG_Options_PreventSavingChanges.gif)
 
 6. Refer to the Description of the coolOrange Intermediate Database to understand the database model.
7. Enhance the Files, Folders, Items and/or CustomObjects table with additional UDP fields for transferring metadata to user defined properties (UDPs) in Vault.

   * For each property (field) of the legacy system that you want to transfer to Vault an UDP-field in the object table must be added. The name of the UDP-field can be renamed.
 
### DB Structure
![Database Schema](Images/DB_Schema.png)

   
## IDB.Load.Files
Utility to load files from Windows folders to the Intermediate Database (IDB)

### Configuration
In the configuration file ***DefaultBehaviors.xml*** the default behaviours can be set.
![image](https://user-images.githubusercontent.com/62716091/81202972-a05dff00-8fc7-11ea-9a4e-c1ce65170e65.png)


* ConnectionString: Connect string to SQL server and database
* DataPath: Folderpath that will be scanned. All names of subfolders and files will be transferred to the IDB.

* Folders: Default settings for folder. The following fields in the IDB will be filled with the assigned value for all folders.
	* Category
	* CreateUser 

* Files: Default settings for files. The following fields in the IDB will be filled with the assigned value for all files.
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

### Usage
Start the tool with double click the file IDB.Load.Files.exe.
In the open dialog specify the ***Path*** and ***SQL Database Connection String*** to import files from the selected folder and sub-folders into the named database.

* Save: The specified path and connect string are written back to file DefaultBehaviors.xml.
* Refresh: Updating the path and connect string from the file DefaultBehaviors.xml.
* Start: Start scan and import
* Cancel: Stop the process. After clicking the button all records and unsaved data will be lost.
* Reset: Initialize for next run
* Scan records: File`s counter of entered folder path
* Import records: Counter of already inserted files

![image](https://user-images.githubusercontent.com/62716091/81194971-89b2aa80-8fbd-11ea-8374-c282ad0bbc2d.png)


### Logging
The default location for the log file IDB.Load.Files.log is '*C:\Users\coolOrange\AppData\Local\coolOrange\powerLoad*'. 
There you find information about successful inserts and errors.

## IDB.Load.BCP
(Description is in preparation)

## IDB.Load.Vault
Sample code to extract data from Vault and fill the Intermediate Database (IDB). The example uses an Excel table to identify the files to be read out.


## IDB.Analyzer.Inventor
Utility to scan Inventor files for missing references that are listed in the IDB in the field 'LocalFullFileName'. Additionally the RefID from the reference is extracted and written back to the IDB.

### Prerequsite
This tool uses Inventor Apprentice. So at least Inventor View must be installed on the machine where this tool is used.

### Configuration
In the configuration file ***IDB.Analyzer.Inventor.exe.config*** the connection to the SQL database must be set.
* At the setting ***name="ConnectionString"*** the connect string to SQL server and database must be set. Use the login information, that you use when you login with the Microsoft SQL Server Management Studio.
* At the setting ***name="WorkingDirectory*** the working directory can be modified if needed. The deflaut is ***C:\temp\IDBAnalyze\InventorData***.

Don`t rename configuration files.

### Usage
Start the tool with double click the file IDB.Analyzer.Inventor.exe. A Windows console will start and the tools scans the Inventor files that are listed in the IDB in the field 'LocalFullFileName'.
The IDB.Analyzer.Inventor scans:
* File not exist
* File invalid (File cannot be opened)
* Missing references / Contains missing references

### Logging
The default location for the log file IDB.Load.Files.log is '*C:\Users\coolOrange\AppData\Local\coolOrange\powerLoad*'. 

## Product Documentation

[coolOrange bcpToolkit](https://www.coolorange.com/wiki/doku.php?id=bcptoolkit)

## Author
coolOrange s.r.l.
