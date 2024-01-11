// 执行命令并且获取返回值
def CallCmd(cmdline) {
  def isWindows = !isUnix()
  if (isWindows) {
    def result = bat(returnStatus: true, script: cmdline)
    return result
    }else {
    def result = sh(returnStatus: true, script: cmdline)
    return result
  }
}

// 获取Unity的exe文件目录
def GetUnityExePath() {
  // 优先使用jenkins环境变量中的Unity路径，其次才是代码中定义的默认值
  def unityExePath = ''
  if (env.Unity2021) {
    unityExePath = env.Unity2021
  } else {
    if (isUnix()) {
      unityExePath = Unity2021_DefaultPath_Unix
    } else {
      unityExePath = env.Unity2021_DefaultPath
    }
  }
  return unityExePath
}

pipeline {
  agent any

  environment {
    Unity2021_DefaultPath = 'C:/Program Files/Unity/Hub/Editor/2021.3.16f1/Editor/Unity.exe'
    Unity2021_DefaultPath_Unix = '/Applications/Unity/Hub/Editor/2021.3.16f1/Unity.app/Contents/MacOS/Unity'
  }

  stages {
    stage('检测本地工程') {
      steps {
        script {
          def projectExist = fileExists(params.projectPath)
          if (!projectExist) {
            // 不存在工程，需要拉取一下,|符号需要转义
            def scmUrlArray = params.scmUrl.split("\\|")
            def scmUrlRaw = ''
            def scmBranch = ''
            if (scmUrlArray.size() == 1) {
              scmUrlRaw = scmUrlArray[0]
            } else if (scmUrlArray.size() == 2) {
              scmUrlRaw = scmUrlArray[0]
              scmBranch = scmUrlArray[1]
            } else {
              error("check local project: scm url format error! ${params.scmUrl}")
            }
            def cmdArg = ''
            if (params.versionControl == '0') {
              if (scmBranch != '') {
                cmdArg = """
                git clone -b ${scmBranch} ${scmUrlRaw} "${projectPath}" || exit 1
                """
              } else {
                cmdArg = """
                git clone ${scmUrlRaw} "${projectPath}" || exit 1
                """
              }
            }
            else if (params.versionControl == '1') {
              cmdArg = """
                svn checkout ${scmUrlRaw} "${projectPath}" || exit 1
              """
            }
            if (cmdArg != '') {
              echo 'Start checkout or clone project...'
              def exitCode = CallCmd(cmdArg)
              if (exitCode != 0) {
                error('checkout or clone project failed!')
              }
            }
          }
          
        }
      }
    }
    stage('更新工程') {
      steps {
        script {
          // 调用工程更新
          // 特别注意，加了params.的jenkins参数booleanParam才是有类型的bool，否则是string。extendedChoice得到的参数都是string
          if (params.enableProjectUpdate) {
            def cmdArg = ''
            if (params.versionControl == '0') {
              cmdArg = """
                git -C "${projectPath}" restore -s HEAD -- "${projectPath}/Assets" || exit 1
                git -C "${projectPath}" restore -s HEAD -- "${projectPath}/ProjectSettings" || exit 1
                git -C "${projectPath}" pull || exit 1
              """
            }
            else if (params.versionControl == '1') {
              cmdArg = """
                svn revert -R "${projectPath}/Assets" || exit 1
                svn revert -R "${projectPath}/ProjectSettings" || exit 1
                svn update "${projectPath}" || exit 1
              """
            }
            if (cmdArg != '') {
              echo 'Start update project...'
              def exitCode = CallCmd(cmdArg)
              if (exitCode != 0) {
                error('update project failed!')
              }
              return
            }
          }

          echo 'Skip Update Project!'
        }
      }
    }

    stage('准备构建参数') {
      steps {
        script {
          // 判断打包平台,用于控制启动Unity执行的打包方法
          def buildMethod = 'AutoBuild.AutoBuildEntry.'
          // 启动Unity直接指定的目标平台 
          // Standalone Win Win64 OSXUniversal Linux64iOS Android WebGL WindowsStoreApps tvOS
          def buildTarget = ''
          // 打包输出目录
          def finalOutputPath = outputPath

          switch (params.buildPlatform) {
            case '0':
              //windows
              buildMethod += 'BuildWindows'
              finalOutputPath +=  '/Windows'
              buildTarget = 'Standalone'
              break
            case '1':
              //Android
              buildMethod += 'BuildAndroid'
              finalOutputPath += '/Android'
              buildTarget = 'Android'
              break
            case '2':
              //iOS
              buildMethod += 'BuildiOS'
              finalOutputPath += '/iOS'
              buildTarget = 'iOS'
              break
            default :
              error('Build Platform not support!')
              break
          }

          //获取一些参数来自定义的构建名
          def buildDisplayName = currentBuild.displayName
          buildDisplayName = buildDisplayName.startsWith('#') ? buildDisplayName.substring(1) : buildDisplayName
          def formattedDate = new Date().format('yyyy-MM-dd')
          def buildVersionName = "${JOB_NAME}_${buildDisplayName}_${formattedDate}"
          echo "buildVersionName:${buildVersionName}"

          //调用unity的命令行参数
          env.unity_execute_arg = ("-quit -batchmode -nographics -logfile -projectPath \"${projectPath}\" -executeMethod ${buildMethod} -buildTarget ${buildTarget} "
          + "\"buildPlatform|${buildPlatform}\" \"outputPath|${finalOutputPath}\" \"buildVersionName|${buildVersionName}\" \"buildMode|${buildMode}\" "
          + "\"versionNumber|${versionNumber}\" \"enableIncrement|${enableIncrement}\" \"androidBuildOption|${androidBuildOption}\" \"enableBuildExcel|${enableBuildExcel}\" "
          + "\"enableUnityDevelopment|${enableUnityDevelopment}\" \"enableGameDevelopment|${enableGameDevelopment}\" "
          )
        }
      }
    }

    stage('Unity构建') {
      steps {
        script {
          def unityExePath = GetUnityExePath()
          def cmdArg = "\"${unityExePath}\" ${env.unity_execute_arg}"
          def exitCode = CallCmd(cmdArg)
          if (exitCode != 0) {
            error('unity build target failed!')
          }
        }
      }
    }
	// todo 添加打包后stage处理，或者xcode打包stage
  }
}
