#==============================================================================#
# (c) 2021 coolOrange s.r.l.                                                   #
#                                                                              #
# THIS SCRIPT/CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER    #
# EXPRESSED OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES  #
# OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.   #
#==============================================================================#

$csvFile = "C:\Temp\YourCsvData.csv"
$delimiter = ";"
$csv = Import-Csv -Path $csvFile -Delimiter $delimiter

$errors = @()
$unresolved = @()

for($i = 0; $i -lt $csv.Count; $i++ ) {
    $row = $csv[$i]
    Write-Progress -Activity Updating -Status 'Progress->' -PercentComplete ($i/$csv.Count*100) -CurrentOperation $row.Path | Out-Null

    # find your path to the Vault file by replacing parts of the path
    # $row.Path -> the variable "Path" is a column in the CSV data source. Needs to be changed if the column where the file is listed is different
    $filePath = $row.Path.Replace("C:\Import", "$/Designs/Inventor Sample Data/Models").Replace("\", "/")

    # find file by path
    $file = Get-VaultFile -File $filePath

    # instead of manipulating the string to find a file by it's full path in Vault, one could use 
    # the parameter -Properties of the cmdlet Get-VaultFile to find a file by name or any other search criteria
    # e.g. 
    #$fileName = [System.IO.Path]::GetFileName($row.Path)
    #$file = Get-VaultFile -Properties @{Name = $fileName}

    if ($file) {
        # update the properties
        $updatedFile = Update-VaultFile -File $file._FullPath -Properties @{
            "Author" = $row.Originator
            # more properties can be filled here...
        }  

        if (-not $updatedFile) {
            # write csv row to "errors"
            $errors += $row
        }
    } else {
        # write csv row to "unresolved"
        $unresolved += $row
    }
}

$errors | Export-Csv -Delimiter ";" -Path ($csvFile + "_errors.csv")
$unresolved | Export-Csv -Delimiter ";" -Path ($csvFile + "_unresolved.csv")