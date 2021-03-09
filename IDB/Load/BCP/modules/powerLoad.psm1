function Convert-BcpToCsv{
    param(
        [Parameter(Mandatory=$true)]
        [string]$XmlPath,
        [Parameter(Mandatory=$true)]
        [string]$XslPath,
        [string]$OutputFilePath
    )
    if (-not $OutputFilePath){
        $CsvFileName = "{0}.csv" -f [io.path]::GetFileNameWithoutExtension($XslPath)
        $directoryName = [io.path]::GetDirectoryName($XslPath)
        $OutputFilePath = (Join-Path $directoryName $CsvFileName)
    }

    $XsltSettings = New-Object System.Xml.Xsl.XsltSettings
    $XsltSettings.EnableDocumentFunction = $true
    #$XsltSettings.EnableScript = $true

    #$writter = [System.Xml.XmlTextWriter]::Create($XMLOutputFile)
    $XmlResolver = New-Object System.Xml.XmlUrlResolver

    $XSLTCompiledTransform = New-Object System.Xml.Xsl.XslCompiledTransform
    $XSLTCompiledTransform.Load($XslPath,$XsltSettings,$XmlResolver)

    $XSLTCompiledTransform.Transform($XmlPath, $OutputFilePath)
}


function Convert-Xml{
    param(
        [Parameter(Mandatory=$true)]
        [string]$XmlPath,
        [Parameter(Mandatory=$true)]
        [string]$XslPath,
        [Parameter(Mandatory=$true)]
        [string]$OutputFilePath
    )
    #if (-not $OutputFilePath){
    #    $CsvFileName = "{0}.csv" -f [io.path]::GetFileNameWithoutExtension($XslPath)
    #    $directoryName = [io.path]::GetDirectoryName($XslPath)
    #    $OutputFilePath = (Join-Path $directoryName $CsvFileName)
    #}

    $XsltSettings = New-Object System.Xml.Xsl.XsltSettings
    $XsltSettings.EnableDocumentFunction = $true
    #$XsltSettings.EnableScript = $true

    #$writter = [System.Xml.XmlTextWriter]::Create($XMLOutputFile)
    $XmlResolver = New-Object System.Xml.XmlUrlResolver

    $xslarguments = New-Object System.Xml.Xsl.XsltArgumentList
    $xslarguments.AddParam("documentName","","ExportFolderUDP2.xml")

    $XSLTCompiledTransform = New-Object System.Xml.Xsl.XslCompiledTransform
    $XSLTCompiledTransform.Load($XslPath,$XsltSettings,$XmlResolver)
    $sw = [io.StreamWriter] $OutputFilePath
    $XSLTCompiledTransform.Transform($XmlPath,$xslarguments, $sw)
    $sw.Close()
}

function Convert-VaultUdpXml{
    param(
        [Parameter(Mandatory=$true)]
        [string]$xmlpath,
        [Parameter(Mandatory=$true)]
        [string]$xslpath,
        [Parameter(Mandatory=$true)]
        [string]$outputxmlpath
    )
    Convert-BcpToCsv -XmlPath $xmlpath -XslPath $xslpath -OutputFilePath $outputxmlpath
}

function Convert-VaultFilesUdpXml{
    $xmlpath = "C:\xslt xml to csv\Vault.xml"
    $xslpath = "C:\xslt xml to csv\ExportFileUDP.xsl"
    $outputxmlpath = "C:\xslt xml to csv\VaultFileUdp.xml"

    Convert-VaultUdpXml -XmlPath $xmlpath -XslPath $xslpath -outputxmlpath $outputxmlpath
}

function Convert-VaultFolderUdpXml{
    $xmlpath = "C:\xslt xml to csv\Vault.xml"
    $xslpath = "C:\xslt xml to csv\ExportFolderUDP.xsl"
    $outputxmlpath = "C:\xslt xml to csv\VaultFolderUdp.xml"

    Convert-BcpToCsv -XmlPath $xmlpath -XslPath $xslpath -OutputFilePath $outputxmlpath
}

function Open-SqlConnection{
    param(
        [Parameter(Mandatory=$true)]
        [string]$Server,
        [Parameter(Mandatory=$true)]
        [string]$Database
    )
    $global:SqlConn = New-Object System.Data.SqlClient.SqlConnection
    $SqlConn.ConnectionString = “Data Source=($Server)\AUTODESKVAULT;Initial Catalog=$($Database);Trusted_Connection=True”
    $SqlConn.Open()

}

function Load-CsvToSqlDatabase{
    param(
        [string]$CsvPath,
        [string]$Table
    )

    $sqlCmd = New-Object System.Data.SqlClient.SqlCommand
    $sqlCmd.Connection = $SqlConn
    $sqlCmd.CommandText = "BULK INSERT dbo.$($Table) FROM '$($CsvPath)' WITH (DATAFILETYPE = 'char', FIRSTROW=2, FIELDTERMINATOR =';', ROWTERMINATOR = '\n');"
    $tables = @()

    $reader = $sqlCmd.ExecuteReader()

    $fieldCounts = New-Object Object[] $reader.FieldCount
    $tables = while($reader.Read()){
        $reader.GetValues($fieldCounts)
    }
    $reader.Close()
    $sqlCmd.Dispose()
    return $tables
}

function Run-SqlCommand{
    param(
        [Parameter(Mandatory=$true)]
        [string]$SqlQuery
    )
    $sqlCmd = New-Object System.Data.SqlClient.SqlCommand
    $sqlCmd.Connection = $SqlConn
    $sqlCmd.CommandText = $SqlQuery
    $tables = @()

    $result = $sqlCmd.ExecuteNonQuery()
    
    $sqlCmd.Dispose()
    return $result
}

function Create-FolderTable{
    $CommandText = "create table dbo.Folders(
                        FolderID int IDENTITY(1,1) NOT NULL,
                        FolderName nvarchar(250), 
                        Category nvarchar(max),
                        CreateUser nvarchar(max), 
                        CreateDate varchar(max),
                        Path nvarchar(max),
                        CONSTRAINT [PK_Foders] PRIMARY KEY CLUSTERED 
                        (
	                        [FolderID] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]"
    $tables = @()

    $reader = Run-SqlCommand -SqlQuery $CommandText

    $fieldCounts = New-Object Object[] $reader.FieldCount
    $tables = while($reader.Read()){
        $reader.GetValues($fieldCounts)
    }
}


function Remove-ColumnsFromTable{
    param(
        [Parameter(Mandatory=$true)]
        [string]$Table,
        [Parameter(Mandatory=$true)]
        [string[]]$ColumnNames
    )
    foreach ($ColumnName in $ColumnNames){
        $CommandText = "IF COL_LENGTH('$Table', '$ColumnName') IS NOT NULL
        Begin
	        alter table dbo.Folders Drop column $($ColumnName)
        End"

        Run-SqlCommand -SqlQuery $CommandText
    }
    

}
function Add-ColumnsToTable{
    param(
        [Parameter(Mandatory=$true)]
        [string]$Table,
        [Parameter(Mandatory=$true)]
        [string]$xmlpath
    )
    [xml]$xml = Get-Content $xmlpath
    $udps = @()
    foreach ($udp in $xml.list.UDP){
        $udps += "$($udp.'#text') $($udp.DataType)"
    }
    if ($udps.Count -gt 0)
    {
        $udplist = $udps -join ' null,'
        $CommandText = "Alter table dbo.$($Table)
                    Add $($udplist) null;"

        Run-SqlCommand -SqlQuery $CommandText
    }
}


function Load-FolderCsvToSql{
    param(       
        [Parameter(Mandatory=$true)]
        [string]$FolderUdpXmlPath,
        [Parameter(Mandatory=$true)]
        [string]$VaultFoldersCsvPath,
        [Parameter(Mandatory=$true)]
        [string]$Server,
        [Parameter(Mandatory=$true)]
        [string]$Database
    )
    Open-SqlConnection -Server $Server -Database $Database
    $job = [Job]::new($VaultFoldersCsvPath, $null)
    $columnNames = @("UDP_Title", "UDP_Description")
    $removeColumns = [RemoveColumns]::new("Folders",$columnNames)
    $job.AddStage($removeColumns)
    $AddColumns = [AddColumns]::new("Folders",$FolderUdpXmlPath)
    $job.AddStage($AddColumns)
    $LoadCsvToSqlTable = [LoadCsvToSqlTable]::new("Folders",$VaultFoldersCsvPath)
    $job.AddStage($LoadCsvToSqlTable)
    return $job.Invoke()
}

function New-Transform{
    param(
            [Parameter(Mandatory=$true)]
            [string]$XmlPath,
            [Parameter(Mandatory=$true)]
            [string]$FileUdpXslPath,            
            [Parameter(Mandatory=$true)]
            [string]$FolderUdpXslPath,
            [Parameter(Mandatory=$true)]
            [string]$VaultFilesXslPath,
            [Parameter(Mandatory=$true)]
            [string]$VaultFoldersXslPath,
            [Parameter(Mandatory=$true)]
            [string]$FileFileRelationsXslPath,
            [Parameter(Mandatory=$true)]
            [string]$OutputFilePath
        )
    $job = [Job]::new($XmlPath,$null)
    $fileUdps = [XmlTransform]::new($FileUdpXslPath,$null)
    $job.AddStage($fileUdps)
    $folderUdps = [XmlTransform]::new($FolderUdpXslPath,$null)
    $job.AddStage($folderUdps)
    $job.Invoke()
    $job = [Job]::new($XmlPath,$null)
    $vaultFiles = [CsvTransform]::new($VaultFilesXslPath,$null,$fileUdps.OutputFilePath)
    $job.AddStage($vaultFiles)
    $vaultFolders = [CsvTransform]::new($VaultFoldersXslPath,$null,$folderUdps.OutputFilePath)
    $job.AddStage($vaultFolders)
    $fileFileRelations = [CsvTransform]::new($FileFileRelationsXslPath,$null,$null)
    $job.AddStage($fileFileRelations)
    return $job.Invoke()
}

class Stage {
    [string] $Name
    hidden [DateTime] $StartTime = [DateTime]::Now

    Stage($Name) {
        $this.Name = $Name
    }

    [TimeSpan] GetElapsed(){
        return [DateTime]::Now - $this.StartTime
    }

    [string] GetHeader() {
        return "~ [{0}] ~" -f $this.Name
    }
}

class XmlTransform : Stage {
    [string] $Xslpath
    [string] $OutputFilePath
    
    XmlTransform ($Xslpath, $output) : base('Xml to Xml Transform') {
        $this.Xslpath = $Xslpath
        $this.OutputFilePath = $output
    }

    Invoke([job]$J) {

        $J.LogHeader($this.GetHeader())

        if (-not $this.OutputFilePath){
            $XmlFileName = "{0}.xml" -f [io.path]::GetFileNameWithoutExtension($this.XslPath)
            $directoryName = [io.path]::GetDirectoryName($this.XslPath)
            $this.OutputFilePath = (Join-Path $directoryName $XmlFileName)
        }

        $XsltSettings = New-Object System.Xml.Xsl.XsltSettings
        $XsltSettings.EnableDocumentFunction = $true
        #$XsltSettings.EnableScript = $true

        #$writter = [System.Xml.XmlTextWriter]::Create($XMLOutputFile)
        $XmlResolver = New-Object System.Xml.XmlUrlResolver

        $XSLTCompiledTransform = New-Object System.Xml.Xsl.XslCompiledTransform
        $XSLTCompiledTransform.Load($this.XslPath,$XsltSettings,$XmlResolver)
        #$sw = [io.StreamWriter] $this.OutputFilePath
        $XSLTCompiledTransform.Transform($j.Source, $this.OutputFilePath)

        $J.LogEntry("[in {0:N2}s]" -f $this.GetElapsed().TotalSeconds)
    }
}

class CsvTransform : Stage {
    [string] $Xslpath
    [string] $OutputFilePath
    [string] $UdpXmlfilepath

    CsvTransform ($Xslpath, $output, $UdpXmlfilepath) : base('Xml to Csv Transform') {
        $this.Xslpath = $Xslpath
        $this.OutputFilePath = $output
        $this.UdpXmlfilepath = $UdpXmlfilepath
    }

    Invoke([job]$J) {

        $J.LogHeader($this.GetHeader())

        if (-not $this.OutputFilePath){
            $XmlFileName = "{0}.csv" -f [io.path]::GetFileNameWithoutExtension($this.XslPath)
            $directoryName = [io.path]::GetDirectoryName($this.XslPath)
            $this.OutputFilePath = (Join-Path $directoryName $XmlFileName)
        }

        $XsltSettings = New-Object System.Xml.Xsl.XsltSettings
        $XsltSettings.EnableDocumentFunction = $true
        #$XsltSettings.EnableScript = $true

        #$writter = [System.Xml.XmlTextWriter]::Create($XMLOutputFile)
        $XmlResolver = New-Object System.Xml.XmlUrlResolver

        $xslarguments = New-Object System.Xml.Xsl.XsltArgumentList
        $xslarguments.AddParam("documentName","",$this.UdpXmlfilepath)

        $XSLTCompiledTransform = New-Object System.Xml.Xsl.XslCompiledTransform
        $XSLTCompiledTransform.Load($this.XslPath,$XsltSettings,$XmlResolver)
        $sw = [io.StreamWriter] $this.OutputFilePath
        $XSLTCompiledTransform.Transform($j.Source,$xslarguments, $sw)
        $sw.Close()

        $J.LogEntry("[in {0:N2}s]" -f $this.GetElapsed().TotalSeconds)
    }
}

class RemoveColumns: Stage{
    [string[]] $Columns
    [string] $Table
    RemoveColumns ($Table, $Columns) : base('Remove Columns from table') {
        $this.Columns = $Columns
        $this.Table = $Table    
    }

    Invoke([job]$J) {

        $J.LogHeader($this.GetHeader())
        
        foreach ($ColumnName in $this.Columns){
            $CommandText = "IF COL_LENGTH('dbo.$($this.Table)', '$ColumnName') IS NOT NULL
            Begin
	            alter table dbo.Folders Drop column $($ColumnName)
            End"

            Run-SqlCommand -SqlQuery $CommandText
        }
        $J.LogEntry("[in {0:N2}s]" -f $this.GetElapsed().TotalSeconds)
    }
}

class AddColumns: Stage{
    [string[]] $XmlPath
    [string] $Table
    AddColumns ($Table, $xmlpath) : base('Add Columns to table') {
        $this.XmlPath = $xmlpath
        $this.Table = $Table    
    }

    Invoke([job]$J) {

        $J.LogHeader($this.GetHeader())
        [xml]$xml = Get-Content $this.XmlPath
        $udps = @()
        foreach ($udp in $xml.list.UDP){
            $udps += "$($udp.'#text') $($udp.DataType)"
        }        
        if ($udps.Count -gt 0)
        {
            $udplist = $udps -join ' null,'
            $CommandText = "Alter table dbo.$($this.Table)
                        Add $($udplist) null;"

            Run-SqlCommand -SqlQuery $CommandText
        }
        $J.LogEntry("[in {0:N2}s]" -f $this.GetElapsed().TotalSeconds)
    }
}

class LoadCsvToSqlTable: Stage{
    [string[]] $CsvPath
    [string] $Table
    LoadCsvToSqlTable ($Table, $CsvPath) : base('Add Columns to table') {
        $this.CsvPath = $CsvPath
        $this.Table = $Table    
    }

    Invoke([job]$J) {

        $J.LogHeader($this.GetHeader())

        $CommandText = "BULK INSERT dbo.$($this.Table) FROM '$($this.CsvPath)' WITH (DATAFILETYPE = 'char', FIRSTROW=2, FIELDTERMINATOR =';', ROWTERMINATOR = '0x0a');"
        
        Run-SqlCommand -SqlQuery $CommandText
        $J.LogEntry("[in {0:N2}s]" -f $this.GetElapsed().TotalSeconds)
    }
}

class Job {
    [string] $Source
    [string] $Destination

    hidden [array] $Result
    hidden [DateTime] $StartTime = [DateTime]::Now
    hidden [Stage[]] $Stages = @()

    Job ($Source, $Destination) {
        $this.Source = $Source
        $this.Destination = $Destination
    }

    [TimeSpan] GetElapsed(){
        return [DateTime]::Now - $this.StartTime
    }

    [void] LogHeader([string]$S) {
        $this.Result = $S
    }

    [void] LogEntry([string]$S) {
        $this.Result = "`t{0}" -f $S
    }

    [void] LogError([string]$S) {
        $this.LogEntry("`!![{0}]!!" -f $S)
    }

    [Job] AddStage([Stage]$S) {
        $this.Stages += $S

        return $this
    }

    [Job] Invoke() {
        $this.Stages | ForEach-Object {
            try {
                $_.Invoke($this)
                $this.Result | Out-String
            }
            catch {
                $this.LogError($_.Exception.Message)
                break
            }
        }

        return $this
    }

    [string] GetResult() {
        return $this.Result | Out-String
    }
}
