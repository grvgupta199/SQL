clear
[System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO')| out-null

# Create an SMO connection to the instance
$s = new-object ('Microsoft.SqlServer.Management.Smo.Server') "TestServerName,1620" 
$s.ConnectionContext.LoginSecure=$true

#$s.ConnectionContext.StatementTimeout = 0
#$credential = get-Credential $securecred
#$s.ConnectionContext.LoginSecure = $false
#$s.ConnectionContext.Login=$username
#$s.ConnectionContext.set_SecurePassword($credential.Password)

$db = $s.Databases["Demo"]

$scripter = new-object ('Microsoft.SqlServer.Management.Smo.Scripter') ($s)

$scripter.Options.AllowSystemObjects = $false
$scripter.Options.AnsiFile = $true
$scripter.Options.AnsiPadding = $true # true = SET ANSI_PADDING statements
$scripter.Options.Default = $true
#$scripter.Options.DriAll = $true
$scripter.Options.Encoding = New-Object ("System.Text.ASCIIEncoding")
$scripter.Options.ExtendedProperties = $true
#$scripter.Options.IncludeDatabaseContext = $true # true = USE <databasename> statements
$scripter.Options.IncludeHeaders = $false
#$scripter.Options.Indexes = $true
#$scripter.Options.NoCollation = $true # true = don't script verbose collation info in table scripts
$scripter.Options.SchemaQualify = $true
$scripter.Options.ScriptDrops = $false
$scripter.Options.IncludeIfNotExists = $false;
#$scripter.Options.ScriptForCreateDrop=$false
$scripter.Options.ToFileOnly = $true
#$scripter.Options.Triggers = $true
#$scripter.Options.WithDependencies = $false
#$scripter.Options.ScriptForAlter=$true





Function ScriptOutDbObj($inObj)
{
    # Create a single element URN array
    $UrnCollection = New-Object ("Microsoft.SqlServer.Management.Smo.UrnCollection")
    $UrnCollection.Add($inObj.Urn)

    # get the valid Urn.Type string for the file name
    $typeName = $UrnCollection.Item(0).Type
    #Write-Host $typeName
    
   # Write-Host $inObj.Name
   $fileName="$($inObj.Schema+'.'+$inObj.Name).$typeName.sql"

    # tell the scripter object where to write it
    $path=''

    SWITCH($typeName)
    {

    StoredProcedure
    {
      $path=$ProcedurePath
    }

    UserDefinedFunction
    {
     # $path=Join-Path $SavePath 'Function'
    }

    }
    #write-host $path
    
    if(-not (Test-Path $path))
    {
    New-Item -Path $path -ItemType Directory -Force 
    }

    $SavePath = Resolve-Path $path # get the full path of passed in argument (for scripter object's benefit)
    
    #$scripter.Options.Filename = Join-Path $path "$($inObj.Schema + '.' -Replace '^\.','')$($inObj.Name -Replace '[\\\/\:]',' ').$typeName.sql"
   # Write-Host $path
    $scripter.Options.Filename=Join-Path $path $fileName

    # a bit of progress reporting...
    Write-Verbose $scripter.Options.FileName -Verbose

    #and write out the object to the specified file
    $scripter.Script($UrnCollection)
}



$SavePath='C:\Powershell_Build\Publish'


$gitCheckOutPath=JOin-path $SavePath 'Git_DB_Script_Checkout'
$gitScriptPath=JOin-path $gitCheckOutPath 'Scripts'
$gitMainDBScriptPath=JOin-path $gitScriptPath 'dbscript'

$ProcedurePath= Join-Path $gitMainDBScriptPath 'Procedures'
Set-Location  'C:\Windows\System32'


if((Test-Path $gitScriptPath))
    {
 Remove-Item –path $gitScriptPath -Recurse -Force 
 }


 
if(-not (Test-Path $gitScriptPath))
    {
    New-Item -Path $gitScriptPath -ItemType Directory -Force 
    }


$cloneUrl='git URL' # need to update

 Set-Location  $gitScriptPath
 git clone $cloneUrl --progress --verbose


$i=1
$procedures=$db.StoredProcedures | Select -First 3
$totalProcedureCount=$procedures.Count
foreach ($t in $procedures)
{

 Write-Progress -Activity 'Generating Prodecure Scripts' -status $t.Name -percentComplete ($i++ / $totalProcedureCount*100) 

 ScriptOutDbObj($t)

  if($i -eq $totalProcedureCount)
    {
    Write-Progress -Activity 'Prodecure Scripts Generated Successfully' -Status 'Finished' -Completed

    }
}

Set-Location $ProcedurePath
GIT ADD .



<#
$i=1
$functions=$db.UserDefinedFunctions | Select -First 3
$totalFunctionCount=$functions.Count
foreach ($t in $functions)
{

 Write-Progress -Activity 'Generating Function Scripts' -status $t.Name -percentComplete ($i++ / $totalFunctionCount*100) 

 ScriptOutDbObj($t)

  if($i -eq $totalFunctionCount)
    {
    Write-Progress -Activity 'Function Scripts Generated Successfully' -Status 'Finished' -Completed

    }
}

#>