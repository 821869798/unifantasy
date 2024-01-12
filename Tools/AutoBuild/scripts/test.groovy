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
    stages {
        stage('使用dsl创建jobs') {
            steps {
                script {
                    // 证书列表，1:appstore分发证书 2:开发者证书 3: 企业证书
                    // 临时需要改成读取配置
                    def xcodeSigningMap = [
                        '1': new XcodeSigningParam(
                        filePrefix: "appstore_", 
                        mobileprovisionFilePath: "${env.HOME}/Downloads/test.mobileprovision",
                        codeSignIdentity: 'Apple Distribution: Yueyang Yuncai Engineering Labor Service Co., Ltd (LD9X6CBJK2)',
                        signingMethod: 'ad-hoc'),
                        '2': new XcodeSigningParam(
                        filePrefix: "dev_",
                        mobileprovisionFilePath: "${env.HOME}/Downloads/test.mobileprovision",
                        codeSignIdentity: 'Apple Distribution: Yueyang Yuncai Engineering Labor Service Co., Ltd (LD9X6CBJK2)',
                        signingMethod: 'ad-hoc'),
                    ]

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
                        echo CurrentUUID=\${CurrentUUID} > ${tempXcodeProperties} || exit 1
                        echo CurrentBundleId=\${CurrentBundleId} >> ${tempXcodeProperties} || exit 1
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
                          CODE_SIGN_IDENTITY="${signingParam.codeSignIdentity}" PROVISIONING_PROFILE_APP="${xcodeProps.CurrentUUID}" PRODUCT_BUNDLE_IDENTIFIER_APP="${xcodeProps.CurrentBundleId}" CODE_SIGN_STYLE=Manual || exit 1

                        # export ipa
                        xcodebuild -exportArchive -archivePath "${xcodeProject.archivePath}/${signingParam.filePrefix}${xcodeProject.xcodeProjectName}.xcarchive" \\
                          -exportOptionsPlist "${env.WORKSPACE}/${signingParam.filePrefix}exportOptionsPlist.plist" \\
                          -exportPath ${xcodeProject.ipaOutputPath} || exit 1  

                        IpaName=$(basename "$(find '${xcodeProject.archivePath}/${signingParam.filePrefix}${xcodeProject.xcodeProjectName}.xcarchive/Products/Applications' -name '*.app' | head -1 )" .app)

                        # mv 
                        IpaName=$(basename "$(find '${xcodeProject.ipaOutputPath}' -name '*.app' | head -1 )" .app)
                        mv "${xcodeProject.ipaOutputPath}/\${IpaName}.ipa" "${xcodeProject.ipaOutputPath}/${signingParam.filePrefix}\${IpaName}.ipa" || exit 1
                        """
                        exitCode = CallCmd(xcodeShell)
                        if (exitCode != 0) {
                        error('xcodebuild failed!')
                        }

                    }

                    def xcodeProject = new XcodeBuildProject(
                        xcodeProjectName: "Unity-iPhone",
                        xcodeProjectPath: "/Users/bufan/Documents/program/my/unifantasy/build_output/xcode_project",
                        archivePath : "/Users/bufan/Documents/program/my/iOSArchivePath",
                        ipaOutputPath: "/Users/bufan/Documents/program/my/iOSOutput",
                    )

                    def cleanShellScripts = """
                        rm -rf ${xcodeProject.archivePath}
                        mkdir -p ${xcodeProject.archivePath}
                
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
                    signingList.each { String signingType ->
                        def signingParam = xcodeSigningMap.get(signingType)
                        if (signingParam == null) {
                        error("iOS Signing Type not found:${signingType}")
                        }
                        XcodeBuildIpaFunction(xcodeProject,signingParam)
                    }
                }
            }
        }
    }
}
