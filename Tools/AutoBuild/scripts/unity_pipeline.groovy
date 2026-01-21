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
      unityExePath = env.Unity2021_DefaultPath_Unix
    } else {
      unityExePath = env.Unity2021_DefaultPath
    }
  }
  return unityExePath
}

// XCode构建ipa的参数
class XcodeBuildProject {
    String xcodeProjectName
    String xcodeProjectPath
    String archivePath
    String ipaOutputPath
}

// XCode证书信息
class XcodeSigningParam {
    // 这个签名的包的前缀，和别的签名不要重复
    String filePrefix
    String codeSignIdentity
    String mobileprovisionFilePath
    // app-store, development, enterprise, ad-hoc
    String signingMethod
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

       //获取一些参数来自定义的构建名
          def buildDisplayName = currentBuild.displayName
          buildDisplayName = buildDisplayName.startsWith('#') ? buildDisplayName.substring(1) : buildDisplayName
          def formattedDate = new Date().format('yyyy-MM-dd')
          def buildVersionName = "${JOB_NAME}_${formattedDate}_${buildDisplayName}"
          echo "buildVersionName:${buildVersionName}"

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

              // iOS的特殊目录
              env.iOSIpaOutputPath = finalOutputPath + "/" + buildVersionName;
              // xcode project path
              finalOutputPath = projectPath + "_xcodeprj";
              env.iOSArchivePath = projectPath + "_xcode_archive";
              env.iOSXcodeProjectPath = finalOutputPath;

              // 如果没有选证书类型，失败
              List signingList = params.iOSSigningType.tokenize(',')
              if (signingList.size() <= 0) {
                error("iOS build,but no signing type to select!")
              }

              break
            default :
              error('Build Platform not support!')
              break
          }

          //调用unity的命令行参数
          env.unity_execute_arg = ("-quit -batchmode -nographics -logfile -projectPath \"${projectPath}\" -executeMethod ${buildMethod} -buildTarget ${buildTarget} "
          + "\"buildPlatform=${buildPlatform}\" \"outputPath=${finalOutputPath}\" \"buildVersionName=${buildVersionName}\" \"buildMode=${buildMode}\" "
          + "\"versionNumber=${versionNumber}\" \"enableIncrement=${enableIncrement}\" \"androidBuildOption=${androidBuildOption}\" \"enableBuildExcel=${enableBuildExcel}\" "
          + "\"enableUnityDevelopmen=${enableUnityDevelopment}\" \"enableGameDevelopment=${enableGameDevelopment}\" "
          )
        }
      }
    }

    stage('Unity构建') {
      when {
        expression {
          return params.SkipUnityBuild != true
        }
      }
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
    stage("XCode打包ipa") {
      when {
        expression {
          return isUnix() && params.buildPlatform == '2'
        }
      }
      steps {
        script {

          // 写入ipa信息
          def tempIpaInfoProperties = 'xcode_ipa.properties'

          // Xcode ipa构建
          def XcodeBuildIpaFunction = { XcodeBuildProject xcodeProject, XcodeSigningParam signingParam ->
            def mobileprovisionFilePath = signingParam.mobileprovisionFilePath;
            def signingMethod = signingParam.signingMethod;
            // 给shell写入变量值的文件
            def tempXcodeProperties = 'xcode.properties'
            def shellScripts = """
            # 获取mobileprovision的uuid和包名
            CurrentUUID=`/usr/libexec/PlistBuddy -c 'Print UUID' /dev/stdin <<< \$(security cms -D -i ${mobileprovisionFilePath})` || exit 1
            CurrentBundleId=`/usr/libexec/PlistBuddy -c 'Print :Entitlements:application-identifier' /dev/stdin <<< \$(security cms -D -i ${mobileprovisionFilePath}) | cut -d '.' -f2-` || exit 1
            CurrentDevelopmentTeam=`/usr/libexec/PlistBuddy -c 'Print TeamIdentifier:0' /dev/stdin <<< \$(security cms -D -i ${mobileprovisionFilePath})` || exit 1
            echo CurrentUUID=\${CurrentUUID} > ${tempXcodeProperties} || exit 1
            echo CurrentBundleId=\${CurrentBundleId} >> ${tempXcodeProperties} || exit 1
            echo CurrentDevelopmentTeam=\${CurrentDevelopmentTeam} >> ${tempXcodeProperties} || exit 1
            # 安装mobileprovision文件
            cp "${mobileprovisionFilePath}" "${env.HOME}/Library/MobileDevice/Provisioning Profiles/\${CurrentUUID}.mobileprovision" || exit 1
            """
            def exitCode = CallCmd(shellScripts)
            if (exitCode != 0) {
              error('get mobileprovision info failed!')
            }
            // 读取之前shell获取的变量值
            def xcodeProps = readProperties file: tempXcodeProperties
            // 生成exportOptionsPlist.plist文件，打ipa需要
            def exportOptionsContent = """<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>provisioningProfiles</key>
    <dict>
        <key>${xcodeProps.CurrentBundleId}</key>
        <string>${xcodeProps.CurrentUUID}</string>
    </dict>
    <key>method</key>
    <string>${signingMethod}</string>
</dict>
</plist>"""
            println(exportOptionsContent)
            writeFile file: "${env.WORKSPACE}/${signingParam.filePrefix}exportOptionsPlist.plist", text: exportOptionsContent

            // 开始调用xcode build
            def xcodeShell = """
            # replace bundle id 
            sed -E -i '' '/PRODUCT_BUNDLE_IDENTIFIER = "?com\\.unity3d\\..*;/!s/PRODUCT_BUNDLE_IDENTIFIER = .*;/PRODUCT_BUNDLE_IDENTIFIER = ${xcodeProps.CurrentBundleId};/g' "${xcodeProject.xcodeProjectPath}/${xcodeProject.xcodeProjectName}.xcodeproj/project.pbxproj" || exit 1

            # xcode build
            xcodebuild archive -project "${xcodeProject.xcodeProjectPath}/${xcodeProject.xcodeProjectName}.xcodeproj" \\
              -scheme ${xcodeProject.xcodeProjectName} -sdk iphoneos -configuration Release \\
              -archivePath "${xcodeProject.archivePath}/${signingParam.filePrefix}${xcodeProject.xcodeProjectName}.xcarchive" \\
              CODE_SIGN_IDENTITY="${signingParam.codeSignIdentity}" PROVISIONING_PROFILE_APP="${xcodeProps.CurrentUUID}" \\
              PRODUCT_BUNDLE_IDENTIFIER_APP="${xcodeProps.CurrentBundleId}" DEVELOPMENT_TEAM="${xcodeProps.CurrentDevelopmentTeam}" CODE_SIGN_STYLE=Manual || exit 1

            # export ipa
            xcodebuild -exportArchive -archivePath "${xcodeProject.archivePath}/${signingParam.filePrefix}${xcodeProject.xcodeProjectName}.xcarchive" \\
              -exportOptionsPlist "${env.WORKSPACE}/${signingParam.filePrefix}exportOptionsPlist.plist" \\
              -exportPath ${xcodeProject.ipaOutputPath} || exit 1  

            # mv
            iOSProductName=\$(basename "\$(find '${xcodeProject.archivePath}/${signingParam.filePrefix}${xcodeProject.xcodeProjectName}.xcarchive/Products/Applications' -name '*.app' | head -1 )" .app)
            iOSIpaName="${signingParam.filePrefix}\${iOSProductName}"
            mv "${xcodeProject.ipaOutputPath}/\${iOSProductName}.ipa" "${xcodeProject.ipaOutputPath}/\${iOSIpaName}.ipa" || exit 1
            
            echo iOSProductName=\${iOSProductName} > ${tempIpaInfoProperties} || exit 1
            echo iOSIpaName=\${iOSIpaName} >> ${tempIpaInfoProperties} || exit 1
            """
            exitCode = CallCmd(xcodeShell)
            if (exitCode != 0) {
              error('xcodebuild failed!')
            }

          }

          // 创建一个重签名ipa文件
          def ResignIpaFunction = { XcodeBuildProject xcodeProject,XcodeSigningParam signingParam -> 
            def resignShell = """
              echo "Start resign ipa......"
              source ${tempIpaInfoProperties}
    	        /bin/bash ${projectPath}/Tools/AutoBuild/resign.sh \\
                -s "${xcodeProject.ipaOutputPath}/\${iOSIpaName}.ipa" -c "${signingParam.codeSignIdentity}" \\
                -p ${signingParam.mobileprovisionFilePath} || exit 1
              ipaResignOutput="${xcodeProject.ipaOutputPath}/${signingParam.filePrefix}\${iOSProductName}.ipa"
              mv "${xcodeProject.ipaOutputPath}/\${iOSProductName}-resign.ipa" "\${ipaResignOutput}" || exit 1
            """
            def exitCode = CallCmd(resignShell)
            if (exitCode != 0) {
              error("ipa resgin failed: ${signingParam.filePrefix} !")
            }
          }

          // 读取证书的yaml配置
          def iOSYamlText = readFile(file: "${projectPath}/Tools/AutoBuild/iOSSigningConfig.yaml")   
          // 将JSON字符串反序列化为Groovy对象
          def iOSSigningConfig = readYaml text: iOSYamlText
          def signingRootPath = "${projectPath}/Tools/AutoBuild"
          if (iOSSigningConfig.signingIsRmote.toBoolean()) {
            if (iOSSigningConfig.versionControlType == 1) {
              println("pull iOS signing for remote!")
              // svn拉取
              dir('iOSSigning') {
                svn url: iOSSigningConfig.signingScmUrl
              }
            }else{
              // Git拉取工程
              dir('iOSSigning') {
                git branch: iOSSigningConfig.signingScmBranch, url: iOSSigningConfig.signingScmUrl
              }
            }
            signingRootPath = "${env.WORKSPACE}/iOSSigning"
            iOSYamlText = readFile(file: "${env.WORKSPACE}/iOSSigning/${iOSSigningConfig.remoteSigningIndex}")   
            iOSSigningConfig = readYaml text: iOSYamlText
          }

          // 可用的证书map集合
          def xcodeSigningMap = [:]
          iOSSigningConfig.signings.each { key, value ->
            value.mobileprovisionFilePath = "${signingRootPath}/${value.mobileprovisionFilePath}"
            xcodeSigningMap[key] = value
          }

          def xcodeProject = new XcodeBuildProject(
            xcodeProjectName: "Unity-iPhone",
            xcodeProjectPath: env.iOSXcodeProjectPath,
            archivePath : env.iOSArchivePath,
            ipaOutputPath: env.iOSIpaOutputPath,
          )

          def cleanShellScripts = """
            rm -rf ${xcodeProject.archivePath}
            mkdir -p ${xcodeProject.archivePath} || exit 1
            mkdir -p ${xcodeProject.ipaOutputPath} || exit 1
    
            #修改CFBundleVersion
            /usr/libexec/PlistBuddy -c "Set :CFBundleVersion ${params.iOSBundleVersion}" "${xcodeProject.xcodeProjectPath}/Info.plist" || exit 1
            # clean project
            xcodebuild clean -project "${xcodeProject.xcodeProjectPath}/${xcodeProject.xcodeProjectName}.xcodeproj"  -configuration Release -alltargets || exit 1
          """
        
          def exitCode = CallCmd(cleanShellScripts)
          if (exitCode != 0) {
            error('xcode project clean failed!')
          }

          // 根据证书打包
          List signingList = params.iOSSigningType.tokenize(',')
          def count = 0
          signingList.each { String signingType ->
            def signingParam = xcodeSigningMap.get(signingType.toInteger())
            if (signingParam == null) {
              error("iOS Signing Type not found:${signingType}")
            }
            if (params.iOSIpaResign && count > 0) {
              // 使用重签名ipa
              ResignIpaFunction(xcodeProject,signingParam)
            } else {
              XcodeBuildIpaFunction(xcodeProject,signingParam)
            }
            count = count + 1
          }

        }
      }
    }

	// todo 可以添加打包后stage处理

  }
}
