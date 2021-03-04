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

## Additions
For validating the BCP package that is created from the IDB there are additional tools:
* **bcpViewer**, which is part of the bcpToolkit
* **bcpValidator**, which can be found on https://github.com/coolOrangeLabs/bcpValidator

## Prerequsites
* Microsoft SQL Server 2014 or newer
* Windows 10
* AutoCAD and/or Inventor if the analyzer tools are needed

## Installation
Download the complete powerLoad package with the "Download ZIP" command from the "Code"-menu.
![Download powerLoad ZIP package](Images/PL-Dow![PL-Download](https://user-images.githubusercontent.com/36193155/109958680-c25b0780-7ce6-11eb-9c77-4be18e34f650.png)
nload.png)

Extract the ZIP to a folder 'powerLoad' anywhere on your client machine.!


## Description

The Intermediate Database (IDB) is a concept that can be used to fill a neutral database with data from Vault or any other PDM system or from Windows Explorer. This IDB can be transformed into a BCP format which can be imported into Vault Professional or Vault Workgroup.   
This repository contains the IDB structure as well as a tool to convert the database to the Autodesk Vault BCP format. This format is finally used by Autodesk Vault DTU to mass import data into Vault.

## DB Structure
![Database Schema](Images/DB_Schema.png)

## Product Documentation

[coolOrange bcpToolkit](https://www.coolorange.com/wiki/doku.php?id=bcptoolkit)

## Author
coolOrange s.r.l.
