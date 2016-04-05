#
# Adaptive BI Framework 3.0
#
#
# File Revision: 1
# File Date: 2015-08-10
#

#
# Setup DB 
#

#
# Initialize
#
[System.Reflection.Assembly]::LoadWithPartialName('Microsoft.SqlServer.SMO')  | out-null

#
# Configuration
#

# Target Server/Instance
$targetServer = "localhost";

# Root database name
$databaseName = "AdaptiveBI30";

# Database and schema configuration
$conf = "{ 
  'databases':[
    {
      'name': 'HLP_1',
      'schemas': ['bi']
    },
	 {
      'name': 'HLP_2',
      'schemas': ['bi']
    },
    {
      'name': 'DWH',
      'schemas': ['dwh', 'olap', 'util']
    },
	{
      'name': 'STG',
      'schemas': ['etl', 'stg', 'tmp', 'util', 'err', 'proxy_dwh', 'proxy_cfg', 'proxy_aux', 'proxy_md', 'proxy_log']
    },
	{
      'name': 'CFG',
      'schemas': ['cfg']
    },
    {
      'name': 'AUX',
      'schemas': ['aux']
    },
    {
      'name': 'MD',
      'schemas': ['md']
    },
        {
      'name': 'DM',
      'schemas': ['dm']
    },
    {
      'name': 'DI',
      'schemas': ['di']
    },
    {
      'name': 'LOG',
      'schemas': ['log']
    }
	]
}"

# Starting File Size in MB
$dbFileSize = 100

# AutoGrow size in MB
$dbFileGrowth = 100

# Data & Log apth
$dataPath = "D:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA";
$logPath = "D:\Program Files\Microsoft SQL Server\MSSQL12.MSSQLSERVER\MSSQL\DATA";

#
# Start
#

# Set Error Trap

# Handle any errors that occur
Trap 
{
    # Handle the error
    $err = $_.Exception
    Write-Output $err.Message
    while( $err.InnerException ) 
    {
  	    $err = $err.InnerException
  	    Write-Output $err.Message
  	};
    
    break;
}

# Convert JSON to Object
$jsonConf = $conf | ConvertFrom-Json

# Creates a new database using our specifications
$srv = new-object ('Microsoft.SqlServer.Management.Smo.Server') $targetServer

$c = 0
$jsonConf.databases | ForEach-Object {
	$c += 1
	$dbname = $databaseName + "_" + $_.name
	
	Write-Progress -Activity "Creating Databases" -Status "Creating $dbname" -CurrentOperation "Creating Database" -PercentComplete (($c / $jsonConf.databases.Count) * 100)

	# Instantiate the database object
	$db = new-object ('Microsoft.SqlServer.Management.Smo.Database') ($srv, $dbname)

	# Create Primary FileGroup
	$primaryFG = new-object ('Microsoft.SqlServer.Management.Smo.FileGroup') ($db, 'PRIMARY')
	$db.FileGroups.Add($primaryFG)

	# Create Secondary FileGroup
	$secondaryFG = new-object ('Microsoft.SqlServer.Management.Smo.FileGroup') ($db, 'SECONDARY')
	$db.FileGroups.Add($secondaryFG)

	# Create the file for the system tables
	$sysDataFile = $dbname + '_sys'
	$dbdsysfile = new-object ('Microsoft.SqlServer.Management.Smo.DataFile') ($primaryFG, $sysDataFile)
	$primaryFG.Files.Add($dbdsysfile)

	$dbdsysfile.FileName = [System.IO.Path]::Combine($dataPath, $sysDataFile + '.mdf')
	$dbdsysfile.Size = [double]($dbFileSize * 1024.0)
	$dbdsysfile.GrowthType = 'None'
	$dbdsysfile.IsPrimaryFile = 'True'

	# Create the file for the Application tables
	$dataDataFile = $dbname + '_data'
	$dbddatafile = new-object ('Microsoft.SqlServer.Management.Smo.DataFile') ($secondaryFG, $dataDataFile)
	$secondaryFG.Files.Add($dbddatafile)

	$dbddatafile.FileName = [System.IO.Path]::Combine($dataPath, $dataDataFile + '.ndf')
	$dbddatafile.Size = [double]($dbFileSize * 1024.0)
	$dbddatafile.GrowthType = 'KB'
	$dbddatafile.Growth = [double]($dbFileGrowth * 1024.0)

	# Create the file for the log
	$logDataFile = $dbname + '_log'
	$dblfile = new-object ('Microsoft.SqlServer.Management.Smo.LogFile') ($db, $logDataFile)
	$db.LogFiles.Add($dblfile)

	$dblfile.FileName = [System.IO.Path]::Combine($dataPath, $logDataFile + '.ldf')
	$dblfile.Size = [double]($dbFileSize * 1024.0)
	$dblfile.GrowthType = 'KB'
	$dblfile.Growth = [double]($dbFileGrowth * 1024.0)

	# Create the database
	$db.Create()

	# Set the default filegroup to AppFG
	Write-Progress -Activity "Creating Databases" -Status "Creating $dbname" -CurrentOperation "Setting Default Filegroup"  -PercentComplete (($c / $jsonConf.databases.Count) * 100)	
	$secondaryFG = $db.FileGroups['SECONDARY']
	$secondaryFG.IsDefault = $true
	$secondaryFG.Alter()
	$db.Alter()
	
	# Set Owner
	Write-Progress -Activity "Creating Databases" -Status "Creating $dbname" -CurrentOperation "Setting Owner"  -PercentComplete (($c / $jsonConf.databases.Count) * 100)	
	$db.SetOwner("sa");
	$db.Alter();
	
	# Set Owner
	Write-Progress -Activity "Creating Databases" -Status "Creating $dbname" -CurrentOperation "Setting Simple Recovery Model"  -PercentComplete (($c /$jsonConf.databases.Count) * 100)	
	$db.RecoveryModel = 3;
	$db.Alter();

	# Create Schema
	Write-Progress -Activity "Creating Databases" -Status "Creating $dbname" -CurrentOperation "Creating Schemas"  -PercentComplete (($c / $jsonConf.databases.Count) * 100)	
	$dbSchemas = $_.schemas
	$dbSchemas | ForEach-Object {
		$schema = new-object ('Microsoft.SqlServer.Management.Smo.Schema') ($db, $_)
		$schema.Owner = 'dbo'
		$schema.Create()
	}
}

