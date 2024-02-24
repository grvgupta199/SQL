using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SqlScriptRunner
{
    public class PowerShell
    {
        #region PS

        const string PowerShellScript = @"<# testing 

C:\Projects\NewDBScript\Release\July2021_Release
DERNR874FF,1620
Dev

clear
git for-each-ref --format=' %(authorname), %09 %(refname)' --sort=authorname  |  Export-Csv  'C:\SqlQueryLog\viewOurView.csv' -NoTypeInformation -Delimiter ','


if nuget falied to download run this TLS client for security
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Install-PackageProvider -Name NuGet -RequiredVersion 2.8.5.201 -Force

Invoke-Sqlcmd -ServerInstance DERNR874FF -Database 'Master' -Query 'SELECT GETDATE()' -Verbose
 
C:\PowerShell\Sqlqueries

C:\PowerShell\Sqlqueries\Insert

C:\PowerShell\Sqlqueries\Insert\InsertPowerShelltesting.sql

C:\PowerShell\SqlAutoMate.ps1

Import-Module C:\PowerShell\SqlAutoMate.ps1 

2,7,1,4,8,3,5,6

5,1,3,6,2,4
6,1,4,7,3,5
5,1,3,6,2,4,7

#>

Write-Host 'Loading......' -ForegroundColor White



$null=Add-Type -AssemblyName System.Windows.Forms
$objUser = '' | Select-Object -Property ServerName,LoginMode,UserName,Password,DatabaseName,IsAuthenticated
$sqlConn=''  | Select-Object -Property Conn,ConnectionString
$File=''  | Select-Object -Property FileArray,FilePath,DirectoryPath,TotalFiles,DirectoryArray,OptionSelected,Order,OrderedList
$Log=''  | Select-Object -Property LogFilePath,LogSuccessPath,LogErrorPath
$ZipFileName='SqlQueryLog.zip'
$LineSeprater='_________________________________________________________________________________'
$UserNTLogin=[System.Security.Principal.WindowsIdentity]::GetCurrent().Name -replace 'LL\\',''
$UserPCName=$env:ComputerName
$ErrorMessage =''
$SaveSettings='' |Select-Object -Property ServerName,LoginMode,UserName,Password,DatabaseName,DirectoryPath,LogFilePath
#$SettingPath= JOIN-Path $env:temp '87DD3A8F-6FD9-4F20-91C4-1A6E4DD96E26.json'
$SettingFileName='Default'
$SettingsSaveDirectory=JOIN-Path $env:temp 'SqlRunnerSettings'

if(-not (Test-Path $SettingsSaveDirectory))
{
$null= New-Item -ItemType Directory -Force -Path $SettingsSaveDirectory
}



<#$UserFullName =$([adsi]'WinNT://$env:userdomain/$env:username,user').fullname
if(!$UserFullName)
{
$UserFullName=$UserNTLogin
}
#>


function Sql-GetDateTime
{
return Get-Date
}

$ScriptStartTime=(Sql-GetDateTime).ToString('MMM_dd_yyyy_hh_mm')


function Sql-InitilizeVariable()
{
$objUser.ServerName=''
$objUser.LoginMode=-1
$objUser.UserName=''
$objUser.Password=''
$objUser.DatabaseName=''
$objUser.IsAuthenticated=0
$sqlConn.Conn=''
$sqlConn.ConnectionString=''
$File.FileArray=''
$File.FilePath=''
$File.DirectoryPath=''
$File.TotalFiles=''
$File.DirectoryArray=''
$Log.LogFilePath=''
$Log.LogSuccessPath=''
$Log.LogErrorPath=''
$File.OptionSelected=''
}

function Sql-ResetDirectory()
{
$File.FileArray=''
$File.FilePath=''
$File.DirectoryPath=''
$File.TotalFiles=''
$File.DirectoryArray=''
$Log.LogFilePath=''
$Log.LogSuccessPath=''
$Log.LogErrorPath=''
$File.OptionSelected=''

Sql-GetFilesInDirectory
}

function Sql-ResetAll()
{
Process-Init
Sql-InitilizeVariable
Sql-Start

}

function Sql-ResetDatabaseName()
{
 $valid=0
 do {
    $objUser.DatabaseName = Read-Host 'Enter Database Name'
    
    if(-not $objUser.DatabaseName)
    {
    Write-Host 'Please Enter Valid Input ...' -ForegroundColor Red
    }
    else
    {
     $valid=1
      #Sql-GetFilesInDirectory
    }

} while ($valid -eq 0)

}


function Sql-Start()
{

try
{

if(-not $objUser.ServerName)
{
$objUser.ServerName =Read-Host  'Enter Server Name' 
}


Sql-AuthenticatedUser

#Sql-TestConnection


trap
   {
  # throw('Error')
   break
   } 

}

catch [Exception]
{

Write-Host  'Sql-Start:- Something went wrong..' -ForegroundColor Red
throw $_.exception.message

}

}

function Sql-AuthenticatedUser()
{

if(-not $objUser.UserName -or -not $objUser.Password -or $objUser.LoginMode -eq -1)
{

$objUser.IsAuthenticated=0
if(-not $objUser.LoginMode)
{
Write-Host 'Autentication mode?'-ForegroundColor Cyan
}

if(-not $objUser.LoginMode)
{
$LoginMode=1

do {
    $inputValid = [int]::TryParse((Read-Host '[1] Windows [2] SqlServer '), [ref]$LoginMode)
    if (-not $inputValid) {
        Write-Host 'Input must be an integer...' -ForegroundColor Red
    }
    $objUser.LoginMode=$LoginMode
    if($inputValid -and ($objUser.LoginMode -gt 2 -or $objUser.LoginMode -lt  1))
    {
    Write-Host 'Input value must be 1 or 2...' -ForegroundColor Red
    $inputValid=$false
    }

} while (-not $inputValid)

}

switch($objUser.LoginMode)
{
1{
$objUser.IsAuthenticated=1
}
2{
if(-not $objUser.UserName)
{
$objUser.UserName='sa'
}



$Credential = Get-Credential -Message 'Sql Server Autnentication ' -UserName $objUser.UserName
$IsValid=0
    $objUser.UserName=$Credential.UserName #Read-Host 'Enter User Name'
    $objUser.Password=$Credential.Password #Read-Host  'Enter Password' -AsSecureString

    if(-not $objUser.UserName  -and -not $objUser.Password)
    {
      $IsValid=1
    }
    else
    {
      Write-Host 'Please Enter Valid Input...' -ForegroundColor Red
    }

$objUser.IsAuthenticated=$IsValid





}

}

}
else
{
$objUser.IsAuthenticated=1
}
Sql-TestConnection


}


Function Sql-TestConnection()
{
try
{
IF(-not $objUser.ServerName)
{
Sql-Start
}

if($objUser.IsAuthenticated -eq 1)
{

[string] $connectionString=''
 Write-Host 'Connecting...' -ForegroundColor yellow

if($objUser.LoginMode -eq 1)
{
 $sqlConn.ConnectionString ='Data Source='+$objUser.ServerName+';Integrated Security=true;Initial Catalog=master;Connect Timeout=0;'
 }
 else
 {
 $pwd=Uitlity-DecryptPassword($objUser.Password)
$sqlConn.ConnectionString ='Data Source='+$objUser.ServerName+';Integrated Security=false;Initial Catalog=master; User Id='+$objUser.UserName+'; Password='+$pwd+';Connect Timeout=0;'
 }

 #write-host  $sqlConn.ConnectionString

$sqlConn.Conn = new-object ('Data.SqlClient.SqlConnection') $sqlConn.ConnectionString
$sqlConn.Conn.Open()


if ($sqlConn.Conn.State -eq 'Open')
{
$sqlConn.Conn.Close();
 Write-Host  'Connected successfully.' -ForegroundColor green
 
 IF(-not $objUser.DatabaseName)
{

 $valid=0
 do {
    $objUser.DatabaseName = Read-Host 'Enter Database Name'
    
    if(!$objUser.DatabaseName)
    {
    Write-Host 'Please Enter Valid Input ...' -ForegroundColor Red
    }
    else
    {
     $valid=1
      Sql-GetFilesInDirectory
    }

} while ($valid -eq 0)
}
else
{
 Sql-GetFilesInDirectory
}



}

trap
   {
   break
   } 

   }
   else
   {
   Sql-AuthenticatedUser
   }

}
catch [Exception]
{
Write-Host  'Sql-TestConnection:- Something went wrong..' -ForegroundColor Red
Write-Host $_.exception.message
throw $_.exception.message
}
finally{
    $sqlConn.Conn.Dispose() 
    #Write-Host 'Finally Called.' 
}


}


function Sql-GetFilesInDirectory()
{

try
{
$File.FileArray=$null
$selectionOption=0



if (-not $File.DirectoryPath)
{
if(-not $File.OptionSelected)
{
Write-Host 'Do you want to select Folder/File?'


do {
    $inputValid = [int]::TryParse((Read-Host '[1] Folder [2] File ?'), [ref]$selectionOption)
    if (-not $inputValid) {
        Write-Host 'Input must be an integer...' -ForegroundColor Red
    }
    
    if($inputValid -and ($selectionOption -gt 2 -or $selectionOption -lt  1))
    {
    Write-Host 'Input value must be 1 or 2...' -ForegroundColor Red
    $inputValid=$false
    }

} while (-not $inputValid)

$File.OptionSelected=$selectionOption
}
else
{
#$selectionOption=$File.OptionSelected
}


if($selectionOption -eq 1)
{

$File.DirectoryPath = (Sql-GetSelectedFolderPath)

}
elseif($selectionOption -eq 2)
{
$File.DirectoryPath =(Sql-GetSelectedFilePath)
}

IF(-not $File.DirectoryPath)
{
$File.DirectoryPath=Read-Host 'Enter Directory/File Path '
}

}

$LineSeprater
if(-not $Log.LogFilePath)
{
Write-Host 'Enter Log Path :'
$Log.LogFilePath = (Sql-GetSelectedFolderPath)
}

IF(-not $Log.LogFilePath)
{
$Log.LogFilePath=Read-Host 'Enter Log Path '
}

Write-Host 'Selected Dir Path' $File.DirectoryPath
$LineSeprater
Write-Host 'Selected Log Path' $Log.LogFilePath

$Dir=@()
$Dir=(Get-ChildItem -Path $File.DirectoryPath -Directory -Recurse)
#$File.DirectoryArray
$hashTable=$null
$hashTable=New-Object 'system.collections.generic.dictionary[string,string]'
$count=0

$File.DirectoryArray=@()


foreach($N in $Dir)
{

IF ((Get-ChildItem -Path $N.FullName | Where-Object {$_.Extension -eq '.sql'}).Length -gt 0)
{
$File.DirectoryArray=$File.DirectoryArray+=$N
}

}


if($File.DirectoryArray.Count -gt 1)
{

foreach($d in $File.DirectoryArray)
{


if($count -eq 0)
{
Write-Host '------------------------------------------------------'
Write-Host 'Select Folder Execution Order Comma(,) Seprated' -ForegroundColor Cyan
Write-Host 'Example- 3,2,4,1 ' -ForegroundColor Yellow
Write-Host 'Execution Order     Folder Name' 
Write-Host '---------------     -----------' 
}

$count++
Write-Host  $count '                  ' $d.Parent'/'$d.Name

 $hashTable.add($count,$d.FullName)
}


$executionOrder=Read-Host 'Enter Folder Execution Order'
$File.Order=$executionOrder
#Write-Host $executionOrder

$OrderedList=New-Object 'system.collections.generic.List[string]'#@{}

ForEach ($o in $executionOrder.Split(',')) 
{

If($hashTable.ContainsKey($o))
{
   # Write-Host $hashTable[$o]
   $OrderedList.Add($hashTable[$o])
  
 }
 
 }
 $OrderedList = $OrderedList | select -Unique
 $count=0
 $File.OrderedList=$OrderedList
# Write-Host $OrderedList
 foreach($o in $OrderedList)
{
if($count -eq 0)
{
$count++
Write-host $LineSeprater
Write-Host 'Script will Execute in Below Order' -ForegroundColor Yellow
}


 foreach($d in $File.DirectoryArray)
{

IF($d.FullName -eq $o)
{
Write-Host  $d.Parent'/'$d.Name
#$d.FullName
 [System.IO.FileSystemInfo[]]$File.FileArray +=(Get-ChildItem -Path $d.FullName | Where-Object {$_.Extension -eq '.sql'}) #| Sort-Object LastWriteTime -Descending

}


}

}



}

else
{

$File.FileArray =(Get-ChildItem -Path $File.DirectoryPath | Where-Object {$_.Extension -eq '.sql'} )#| Sort-Object LastWriteTime -Descending

}

$count=$File.FileArray.Count
Write-Host $LineSeprater
Write-Host 'Total Scripts to Run- '$count
$File.TotalFiles=$count
if($count -gt 0 )
{
$runScript= ''
$valid=0
do {
    $runScript = Read-Host 'Do you want to run Script? [Y] Yes [N] No'
    #$runScript=$runScript.ToUpper()
   
    IF(($runScript -ne 'Y' -and $runScript -ne 'YES' ) -and ($runScript -ne 'N' -and $runScript -ne 'NO'))
     {
        Write-Host 'Please Enter Valid Input...' -ForegroundColor Red
    }
    else 
    {
     IF(($runScript -eq 'N' -or $runScript -eq 'NO'))
    {
    Write-Host 'You have Select No' -ForegroundColor White -BackgroundColor Red
    $valid++
    }

     IF(($runScript -eq 'Y' -or $runScript -eq 'YES' ))
     {
     # Write-Host $runScript
        $valid=2
        Sql-RunSqlScript
    }

   

    }
   

} while ($valid -lt 2)

}

}
catch
{
Write-Host  'Sql-GetFilesInDirectoryGetFilesInDirectory:- Something went wrong..' -ForegroundColor Red
Write-Host $_.exception.message -ForegroundColor Red
}




}


function Sql-RunSqlScript()
{

$fileName=''

Try
{


IF(-not $objUser.ServerName )
{
Sql-Start
}

IF(-not $File.FileArray)
{
Sql-GetFilesInDirectory
}

Sql-RefreshLogDirectory

$ScriptStartTime=(Sql-GetDateTime).ToString('MMM_dd_yyyy_hh_mm')


#$scope = New-Object -TypeName System.Transactions.TransactionScope
$timeout = New-Object System.TimeSpan -ArgumentList @(0,10,0) # 10 minute
$options = [System.Transactions.TransactionScopeOption]::Required
$scope = New-Object -TypeName System.Transactions.TransactionScope -ArgumentList @($options,$timeout)

$i=1
$hashDictTable=$null
$hashDictTable=New-Object 'system.collections.generic.dictionary[string,string]'
$hashDictExecutionCheck=$null
$hashDictExecutionCheck=New-Object 'System.Collections.Generic.List[string]'
$hashDictExecutedCheck=$null
$hashDictExecutedCheck=New-Object 'System.Collections.Generic.List[string]'

 foreach($dd in $File.OrderedList)
 {
 $hashDictTable.add($dd,$i)
 
 $i++
 }

 $i=0

  foreach ($s in $File.FileArray)
 {
# Write-Host 'Running Script : ' $s.FullName -BackgroundColor Yellow -ForegroundColor black
     $FiletoExcute=$s.FullName
   $fileName=$FiletoExcute

   Write-Host $LineSeprater
   #Write-Host $LineSeprater
    $runningDirScript=$s.Directory.Name
   $dirName=$null
   $id=$null
   $currentId=$null

   If(-not $hashDictExecutionCheck.Contains($runningDirScript))
   {
   $hashDictExecutionCheck.Add($runningDirScript)
   $i=0
   }

  
 foreach($d in $hashDictTable.Keys)
 {

 $dirName=$d.Split('\')[$d.Split('\').Length-1]
 
 $id =$hashDictTable.Item($d)

 if($dirName -eq $runningDirScript)
 {
  $Filename='Executing ' +$dirName+' Scripts'
  $i++
  $currentId=$hashDictTable.Item($d)

 $totalFileCountInDir=(Get-ChildItem -Path $d | Where-Object {$_.Extension -eq '.sql'} ).Count
 $activity=$dirName+' | Total Script '+ $totalFileCountInDir+'/'+$i
 $count =($i / $totalFileCountInDir*100) 
 #Write-Host $totalFileCountInDir +' | '+ $i +' | '+$count

 
 if($i -eq $totalFileCountInDir)
 {
 $hashDictExecutedCheck.Add($runningDirScript)
  $Filename='Executed ' +$dirName+' Scripts'
 Write-Progress -Id $currentId -Activity $activity  -status $Filename -PercentComplete $count 
 }
 else
 {
  
 Write-Progress -Id $currentId -Activity $activity  -status $Filename -PercentComplete $count
 }

 }
 else
 {

 if(-not $hashDictExecutedCheck.Contains($dirName))
 {
 $Filename='Queued ....' +$dirName
 $totalFileCountInDir=(Get-ChildItem -Path $d | Where-Object {$_.Extension -eq '.sql'} ).Count
 $activity=$dirName+' | Total Script '+ $totalFileCountInDir
 
 Write-Progress -Id $id -Activity $activity  -status $Filename 
 }

 }



 }


   #Write-Progress -Activity 'Running Scripts' -status $s.Name -percentComplete ($i++ / $File.TotalFiles*100) 

   IF($objUser.LoginMode -eq 1)
   {

  Invoke-Sqlcmd -ServerInstance  $objUser.ServerName -Database $objUser.DatabaseName -InputFile $FiletoExcute -ErrorAction Stop
  
  }
  else
  {
  $pwd=Uitlity-DecryptPassword($objUser.Password)
   Invoke-Sqlcmd -ServerInstance  $objUser.ServerName -Database $objUser.DatabaseName -InputFile $FiletoExcute -Username $objUser.UserName -Password $pwd -ErrorAction Stop

  }
  Write-Host 'Script Executed Successfully : ' $s.FullName  -ForegroundColor Green
  
   Sql-GenerateLog $s.FullName.ToString() 'success'

   trap
   {
   break
   
   } 

  }

 $scope.Complete() #we tell transactionscope to commit when using ends (dispose)
 $scope.Dispose() #this is where the actual commit is sent to sql server

    Write-Host $LineSeprater

    Write-Progress -Activity 'Scripts Executed Successfully' -Status 'Finished' -Completed

    Mail-SendSuccessMail

    Write-Host  'All Scripts Executed Successfully.....' -ForegroundColor Cyan

    Read-Host  'Press Enter to Leave.....' 

}
catch{
 Write-Progress -Activity 'Something went wrong' -Status 'Error' -Completed
$ErrorMessage= $_.exception.message
   Write-Host $LineSeprater
 $scope.Dispose() #.Dispose() without .Complete() will roll back transaction
  Sql-GenerateLog $fileName.ToString() 'error'

  #  Mail-SendSuccessMail

 Write-Host  'Sql-RunSqlScript:- Something went wrong..' -ForegroundColor Red
Write-Host $_.exception.message -ForegroundColor Red
    

}

}

function Sql-RefreshLogDirectory
{
#$Log.LogFilePath=''
#$Log.LogSuccessPath=''
#$Log.LogErrorPath=''


if(-not $Log.LogFilePath)
{
$Log.LogFilePath='C:\' 
}


If((test-path $Log.LogFilePath))
{
#Remove-Item $Log.LogFilePath -Recurse
}


If(-not (test-path $Log.LogFilePath))
{

 $Log.LogFilePath=Join-Path $Log.LogFilePath 'SqlQueryLog'
 $null=  New-Item -ItemType Directory -Force -Path $Log.LogFilePath 

}


$Log.LogSuccessPath = Join-Path $Log.LogFilePath 'Success'

If((test-path $Log.LogSuccessPath)){
Remove-Item $Log.LogSuccessPath -Recurse
}

If(-not (test-path $Log.LogSuccessPath))
{
     $null= New-Item -ItemType Directory -Force -Path $Log.LogSuccessPath
}

$Log.LogErrorPath = Join-Path $Log.LogFilePath 'Failed'

If((test-path $Log.LogErrorPath)){
Remove-Item $Log.LogErrorPath -Recurse
}
If(!(test-path $Log.LogErrorPath))
{
     $null= New-Item -ItemType Directory -Force -Path $Log.LogErrorPath
}

}


function Sql-GenerateLog 
{
Param([Parameter(Mandatory=$false)][string] $sqlfile,[Parameter(Mandatory=$false)][string] $type )

TRY
{


if((test-path $sqlfile))
{

if($type -eq 'success')
{
Copy-Item -Path $sqlfile -Destination $Log.LogSuccessPath
}
else
{
Copy-Item -Path $sqlfile -Destination $Log.LogErrorPath

}

}

$name='QueryLog_'+$UserNTLogin+'_'+ $UserPCName+'_'+ $ScriptStartTime+'.txt'
$logFilePath= Join-Path $Log.LogFilePath $name


if($type -eq 'success')
{
$sqlfile='n'+$sqlfile+'---------Success'
}
else
{
$sqlfile='n'+$sqlfile+'---------Error'
}


if((test-path $logFilePath) -and $sqlfile)
{

Add-Content $logFilePath $sqlfile
}
else
{
$null=New-Item $logFilePath 
Set-content $logFilePath $sqlfile
}

}
catch{
Write-Host  'Sql-GenerateLog:- Something went wrong..' -ForegroundColor Red
Write-Host $_.exception.message -ForegroundColor Red
    

}

}


function Sql-GetSelectedFilePath
{
$OpenFileDialog = New-Object System.Windows.Forms.OpenFileDialog


if ($OpenFileDialog.ShowDialog() -ne 'Cancel')
 {
     #Write-Host $OpenFileDialog.FileName
    RETURN $OpenFileDialog.FileName
} 
$OpenFileDialog.Dispose()

}

function Sql-GetSelectedFolderPath()
{

$FolderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog 
 $FolderBrowser.ShowNewFolderButton = $True

$Topmost = New-Object System.Windows.Forms.Form
$Topmost.TopMost = $True
#$Topmost.MinimizeBox = $True
$Topmost.ShowInTaskbar = $True;
$Topmost.Width=50
$Topmost.Height=100

#$FolderBrowser.Description = 'Select Script Folder'
[void]$FolderBrowser.ShowDialog($Topmost)
RETURN $FolderBrowser.SelectedPath

$FolderBrowser.Dispose()

}


function Process-Init()
{
clear
if((System-CheckVPN))
{

#Process-Init

}
else
{

#Write-Host 'VPN is not connected.' -ForegroundColor RED
#Write-Host 'Note: If your system required then connect VPN otherwise ignore...' -ForegroundColor yellow
}

Write-Host $LineSeprater -ForegroundColor Magenta
write-host ''
Write-Host 'Welcome '$UserNTLogin -ForegroundColor Cyan
Sql-Help
#User-GetSettings
User-CheckSettingsExists
}


function Mail-SendSuccessMail()
{
try
{
return
$ZipPath=Join-Path $Log.LogFilePath $ZipFileName
Compress-Archive -Path $Log.LogFilePath  -DestinationPath $ZipPath

$SmtpServer=''
#$Port=25
$MailFrom=''
$MailTo=''
$MailCC=''
$MailAttachment=''
if((test-path $ZipPath))
{
$MailAttachment=$ZipPath
}
$MailSubject='Sql Query Executed'
$MailBody='Sql Query Exectued by $UserNTLogin on :-  '+$objUser.ServerName+' | '+$objUser.DatabaseName+'. See the attachment for more details. '+$ErrorMessage+''


Send-MailMessage -SmtpServer $SmtpServer -From $MailFrom -To $MailTo -Subject $MailSubject -Body $MailBody -Attachments $MailAttachment
Remove-Item $ZipPath

}
catch{
Write-Host  'Mail-SendSuccessMail:- Something went wrong..' -ForegroundColor Red
Write-Host $_.exception.message -ForegroundColor Red
    

}
}


function Uitlity-DecryptPassword($Password)
{
RETURN [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($Password))
}

function Sql-Help()
{
Write-Host 'Available commands :'
Write-Host $LineSeprater -ForegroundColor Magenta
Write-Host '.Get Commands            : Sql-Help'
Write-Host '.Initiate process        : Sql-Start'
Write-Host '.Reset directory         : Sql-ResetDirectory'
Write-Host '.Reset All               : Sql-ResetAll '
Write-Host '.Reset DatabaseName      : Sql-ResetDatabaseName'
Write-Host '.RunScript               : Sql-RunSqlScript'
Write-Host '.SaveSettings            : User-SaveSettings'
Write-Host '.GetSettings             : User-GetSettings'
Write-Host '.DeleteSettings          : User-DeleteSavedSettingFile'
Write-Host '.GetSavedSettingFileNames: User-GetSavedSettingFileNames'
Write-Host '.Stop Execution          : CTRL+C'
Write-Host '.If Invoke-Sqlcmd error  : Import-SqlModule'
Write-Host $LineSeprater -ForegroundColor Magenta


}




function User-SaveSettings()
{
#$SaveSettings= $null #Select-Object -Property User,FilePath,LogPath
#ServerName,LoginMode,UserName,Password,DatabaseName,DirectoryPath,LogFilePath
#$objUser.Password=''
$SaveSettings.ServerName=$objUser.ServerName
$SaveSettings.LoginMode=$objUser.LoginMode
$SaveSettings.DatabaseName=$objUser.DatabaseName
IF($objUser.LoginMode -eq 2)
{
$SaveSettings.UserName=$objUser.UserName
$SaveSettings.Password=$objUser.Password | ConvertFrom-SecureString #Uitlity-DecryptPassword($objUser.Password)
}
else
{
$SaveSettings.Password=''
$SaveSettings.UserName=''
}
$SaveSettings.DirectoryPath=$File.DirectoryPath
$SaveSettings.LogFilePath=$Log.LogFilePath

$content=ConvertTo-Json $SaveSettings # | ConvertTo-SecureString -AsPlainText  -Force | ConvertFrom-SecureString



if(-not (Test-Path $SettingsSaveDirectory))
{
$null= New-Item -ItemType Directory -Force -Path $SettingsSaveDirectory
}

$valid=0
 do {
    $settingFileName = Read-Host 'Enter File Name'
    
    if(-not $settingFileName)
    {
    Write-Host 'Please Enter Valid Input ...' -ForegroundColor Red
    }
    else
    {
     $valid=1
    }

} while ($valid -eq 0)


$SettingPath= Join-Path $SettingsSaveDirectory $UserNTLogin'_'$settingFileName'.json'

#Write-Host $SettingPath

User-DeleteSettings

$null=New-Item $SettingPath 
Set-content $SettingPath $content

$settingFileName=''
$SettingPath=''


}

function User-CheckSettingsExists()
{


$SettingFileDir =(Get-ChildItem -Path $SettingsSaveDirectory | Where-Object {$_.Extension -eq '.json'})

if($SettingFileDir.Count -gt 0 )
{
$valid=0
do {
    $loadSettings = Read-Host 'Do you want to load your saved settings? [Y] Yes [N] No'
   
    IF(($loadSettings -ne 'Y' -and $loadSettings -ne 'YES' ) -and ($loadSettings -ne 'N' -and $loadSettings -ne 'NO'))
     {
        Write-Host 'Please Enter Valid Input...' -ForegroundColor Red
    }
    else 
    {
     IF(($loadSettings -eq 'N' -or $loadSettings -eq 'NO'))
    {
    Write-Host 'You have Select No' -ForegroundColor White -BackgroundColor Red
    $valid++
    }

     IF(($loadSettings -eq 'Y' -or $loadSettings -eq 'YES' ))
     {
           $valid=2

IF ($SettingFileDir.Count -eq 1)
{
$path=(Get-ChildItem -Path $SettingsSaveDirectory | Where-Object {$_.Extension -eq '.json'}).FullName

$SettingPath=$path

User-GetSettings 
Sql-Start

}
elseIF ($SettingFileDir.Count -gt 1)
{
#$Table=$null
#$Table=New-Object 'system.collections.generic.dictionary[string,string]'
$count=0

Write-Host 'Please select which file setting you want to load?' -ForegroundColor Yellow

foreach($d in $SettingFileDir)
{

$count++
Write-Host  $d.BaseName

#$Table.add($count,$d.FullName)

}
#write-host $Table

write-host
$n = Read-Host 'Enter File Name'

$SettingPath=Join-Path $SettingsSaveDirectory $n'.json'

User-GetSettings 
Sql-Start

}
   

   }

   }


} while ($valid -lt 1)




}

}


function User-GetSettings()
{

if(-not $SettingPath)
{
$SettingPath=(Get-ChildItem -Path $SettingsSaveDirectory | Where-Object {$_.Extension -eq '.json'}).FullName


}

if((Test-Path $SettingPath))
{

Write-Host 'Loading User Settings....' -ForegroundColor Yellow

$content=Get-Content $SettingPath

$obj=$content | ConvertFrom-Json  


$objUser.ServerName=$obj.ServerName
$objUser.LoginMode=$obj.LoginMode
$objUser.DatabaseName=$obj.DatabaseName
IF($obj.LoginMode -eq 2)
{
$objUser.UserName=$obj.UserName
$objUser.Password=$obj.Password | ConvertTo-SecureString
}
else
{
$SaveSettings.Password=''
$SaveSettings.UserName=''
}
$File.DirectoryPath=$obj.DirectoryPath
$Log.LogFilePath=$obj.LogFilePath

#$obj | Select-Object -Property * | ConvertTo-Json | ConvertFrom-Json

Write-Host 'ServerName   :-' $objUser.ServerName -ForegroundColor Magenta
Write-Host 'DatabaseName :-' $objUser.DatabaseName -ForegroundColor Magenta
IF($obj.LoginMode -eq 2)
{
Write-Host 'Authentication Mode :- Sql Login'  -ForegroundColor Magenta
Write-Host 'UserName     :-' $objUser.UserName -ForegroundColor Magenta
}
elseif($obj.LoginMode -eq 1)
{
Write-Host 'Authentication Mode :- Windows Login'  -ForegroundColor Magenta
}
Write-Host 'DirectoryPath:-' $File.DirectoryPath -ForegroundColor Magenta
Write-Host 'LogFilePath:-' $Log.LogFilePath -ForegroundColor Magenta

Write-Host 'User Settings Loaded....' -ForegroundColor Gray

#Write-Host 'Processing....' -ForegroundColor Green
$LineSeprater


}

}


function User-DeleteSettings()
{
if((Test-Path $SettingPath))
{
Remove-Item $SettingPath

}
}

function User-DeleteSavedSettingFile()
{

User-GetSavedSettingFileNames

write-host
$n = Read-Host 'Enter File Name to Delete'

$p=Join-Path $SettingsSaveDirectory $n'.json'

if((Test-Path $p))
{
Remove-Item $p

}
else
{
Write-Host 'File does not exist..' -ForegroundColor Red
}

}
function User-GetSavedSettingFileNames()
{
$count=0
foreach($d in (Get-ChildItem -Path $SettingsSaveDirectory | Where-Object {$_.Extension -eq '.json'}))
{

if($count -eq 0)
{
Write-Host 'Settings Files......' -ForegroundColor Yellow
Write-Host $LineSeprater
}

$count++
Write-Host  $d.BaseName

}

}



function System-CheckVPN()
{

$vpnCheck = Get-WmiObject -Query 'Select Name,NetEnabled from Win32_NetworkAdapter where (Name like  ''%Check Point Virtual Network Adapter For Endpoint VPN Client%'') and NetEnabled=''True'''
$vpnCheck = [bool]$vpnCheck
 return $vpnCheck
 

}


#if((System-CheckVPN) -eq $false)
#{

#Process-Init

#}
#else
#{

#Write-Host 'VPN is not connected. Please connect VPN to proceed further...' -ForegroundColor RED
#Write-Host 'Note: If your system require then connect VPN otherwise ignore...' -ForegroundColor yellow
#}


Process-Init


#Invoke-Sqlcmd -ServerInstance $SQLServer -Database $db3 -InputFile $File
#Invoke-Sqlcmd -ServerInstance $SQLServer -Database $db3 -InputFile $File -Username $Username -Password $Password 

#Read-Host 'Press ennter to exit....'

#$file_data = Get-Content $FilePath
#write-host Forbackground $file_data

function Import-SqlModule()
{
CheckandLoad-Module 'SqlServer'
}

function CheckandLoad-Module ($moduleName)
 {
 [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
  Write-Verbose 'Checking Modules.....' -Verbose

    # If module is imported say that and do nothing
    if (Get-Module | Where-Object {$_.Name -eq $moduleName}) {
         #Write-Verbose 'Module $moduleName is already imported.' -Verbose
         Write-Verbose 'Initializing Process..' -Verbose
    }
    else {

        # If module is not imported, but available on disk then import
        if (Get-Module -ListAvailable | Where-Object {$_.Name -eq $moduleName}) {
         Write-Verbose 'Importing Module SqlServer' -Verbose
            Import-Module $moduleName -Verbose
           
        }
        else {

            # If module is not imported, not available on disk, but is in online gallery then install and import
            if (Find-Module -Name $moduleName | Where-Object {$_.Name -eq $moduleName}) {
               Write-Verbose 'Installing  Module SqlServer' -Verbose
                Install-Module -Name $moduleName -Force -Verbose 
                Write-Verbose 'Importing Module SqlServer' -Verbose
                Import-Module $moduleName -Verbose
                
            }
            else {

                # If module is not imported, not available and not in online gallery then abort
                Write-Verbose 'Module $moduleName not imported, not available and not in online gallery, exiting.' -Verbose
               
            }
        }
    }
    }";

        #endregion



        public static string GetFilePath()
        {
            string someText = PowerShellScript;
            string fileName = $"25B0438D-82AC-4DDE-8BFC-78B72DA592A9"; //Guid.NewGuid().ToString();
            string tempFolder = Path.GetTempPath();
            if (!Directory.Exists(tempFolder))
                Directory.CreateDirectory(tempFolder);

            var filePath = Path.Combine(tempFolder, fileName + ".ps1");
            if (File.Exists(filePath))
                File.Delete(filePath);
            File.WriteAllText(filePath, someText);
            return filePath;

        }


        // Function to encrypt the String 
        static String encryption(char[] s)
        {
            int l = s.Length;
            int b = (int)Math.Ceiling(Math.Sqrt(l));
            int a = (int)Math.Floor(Math.Sqrt(l));
            String encrypted = "";
            if (b * a < l)
            {
                if (Math.Min(b, a) == b)
                {
                    b = b + 1;
                }
                else
                {
                    a = a + 1;
                }
            }

            // Matrix to generate the 
            // Encrypted String 
            char[,] arr = new char[a, b];
            int k = 0;

            // Fill the matrix row-wise 
            for (int j = 0; j < a; j++)
            {
                for (int i = 0; i < b; i++)
                {
                    if (k < l)
                    {
                        arr[j, i] = s[k];
                    }
                    k++;
                }
            }

            // Loop to generate 
            // encrypted String 
            for (int j = 0; j < b; j++)
            {
                for (int i = 0; i < a; i++)
                {
                    encrypted = encrypted +
                                arr[i, j];
                }
            }
            return encrypted;
        }

        // Function to decrypt the String 
        static String decryption(char[] s)
        {
            int l = s.Length;
            int b = (int)Math.Ceiling(Math.Sqrt(l));
            int a = (int)Math.Floor(Math.Sqrt(l));
            String decrypted = "";

            // Matrix to generate the 
            // Encrypted String 
            char[,] arr = new char[a, b];
            int k = 0;

            // Fill the matrix column-wise 
            for (int j = 0; j < b; j++)
            {
                for (int i = 0; i < a; i++)
                {
                    if (k < l)
                    {
                        arr[j, i] = s[k];
                    }
                    k++;
                }
            }

            // Loop to generate 
            // decrypted String 
            for (int j = 0; j < a; j++)
            {
                for (int i = 0; i < b; i++)
                {
                    decrypted = decrypted +
                                arr[i, j];
                }
            }
            return decrypted;
        }


        public static string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        public static string BinaryToString(string data)
        {
            List<Byte> byteList = new List<Byte>();

            for (int i = 0; i < data.Length; i += 8)
            {
                byteList.Add(Convert.ToByte(data.Substring(i, 8), 2));
            }
            return Encoding.ASCII.GetString(byteList.ToArray());
        }


        public static string ToHexString(string str)
        {
            var sb = new StringBuilder();

            var bytes = Encoding.Unicode.GetBytes(str);
            foreach (var t in bytes)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }

        public static string FromHexString(string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }

            return Encoding.Unicode.GetString(bytes);
        }
    }

}