
#solution name to build
$solutionName = "DGDataConcurrencyHelper"

#set version
$versionMajor = "1"
$versionMinor = "2"
$versionBuild = GetVersionBuild
$versionRevision = "0"
#build version number
$version = GetVersion $versionMajor $versionMinor $versionBuild $versionRevision

#base folder for of the solution
$baseDir  = Resolve-Path .\..\

#release folder for of the solution
$releaseDir = Resolve-Path .\..\..\Release

#builder parameters
$buildDebugAndRelease = $false
$treatWarningsAsErrors = $true
$releaseDebugFiles = $false

#remove elder release builds for the actual version
$removeElderReleaseWithSameVersion = $true

#folders and files to exclude from the packaged release Source
$releaseSrcExcludeFolders = @('"_DevTools"', '".git"');
$releaseSrcExcludeFiles = @('".git*"');

#builds array
#include here all the solutions file to build	
$builds = @(
	@{
		#solutions filename (.sln)
		Name = "DGDataConcurrencyHelper";
		#msbuild optionals constants
		Constants = "";
		#projects to exclude from the release binary package
		ReleaseBinExcludeProjects = @(
			@{
				Name = "DGDataConcurrencyHelper.Test";
			}
		);
		#files to include in the release binary package
		ReleaseBinIncludeFiless = @(
			@{
				Name = "DGDataModel";
				Files = @(
					@{
						FileNameFrom = "..\License";
						FileNameTo = "."
					},
					@{
						FileNameFrom = "..\README.md";
						FileNameTo = "README.md"
					}
				)
			}
		);
		#unit tests to run
		Tests = @(
			@{
				Name = "DGDataConcurrencyHelper.Test";
				TestDll = "DGDataConcurrencyHelper.Test.dll"
			}
		);
		#commands to run before packaging of the release source
		ReleaseSrcCmd = @(
			@{
				Cmd = ".\dbsql-backuptsql.bat ..\..\ empty"
			},
			@{
				Cmd = ".\dbsql-backupsqlschema.bat ..\..\"
			},
			@{
				Cmd = "xcopy ..\..\_DBDump\* Working\Src\_DBDump\ /s /e /y"
			}			
		);
		#commands to run before packaging of the release source
		ReleaseBinCmd = @();
	};
)