
// 生成 Active Choice 的选项脚本（支持 Map 格式：显示值 -> 实际值）
def generateActiveChoiceScript(values, defaultValues, descriptions) {
    def valueList = values.split(',')
    def defaultList = defaultValues.split(',')
    def descList = descriptions.split(',')
    
    def lines = []
    for (int i = 0; i < valueList.size(); i++) {
        def val = valueList[i].trim()
        def desc = (i < descList.size()) ? descList[i].trim() : val
        def isSelected = defaultList.contains(val)
        def key = isSelected ? "${desc}:selected" : desc
        lines.add("\"${key}\": \"${val}\"")
    }
    return "return [${lines.join(', ')}]"
}

class PipelineProject {
    String name
    String buildPlatform
    String description
}

//传递过来的参数，不需访问additionalParameters，已在根环境中，直接访问
//def dsl_pipelineName = additionalParameters.dsl_pipelineName
//def dsl_scmUrl = additionalParameters.dsl_scmUrl

// 创建三个平台的构建流水线
def projects = [
    new PipelineProject(name: "${dsl_pipelineName}-Android",      buildPlatform: '1',     description: 'build Android'),
    new PipelineProject(name: "${dsl_pipelineName}-iOS",          buildPlatform: '2',     description: 'build iOS'),
    new PipelineProject(name: "${dsl_pipelineName}-Windows",      buildPlatform: '0',     description: 'build Windows')
]

// 默认的工作目录
def defaultWorkPath = "D:/JenkinsHome/";
if (isUnix()) {
    // macos linux，获取User目录
    def userHome = System.getProperty('user.home')
    // 下面这行代码效果是一样的，dsl中能直接用env，需要用System.getenv
    // def userHome = System.getenv('HOME')
    defaultWorkPath = "${userHome}/JenkinsHome/";
}

 // 默认的输出目录:TODO，推荐把输出目录修改成打包机本地挂载的局域网共享盘（windows和macos都支持挂载的），这样远程打包完直接去共享盘取。
def defaultOutputPath = "${defaultWorkPath}Output";

projects.each { project ->
    pipelineJob("${project.name}") {
        // Define job properties
        description("${project.description}")
        //job parameters
        parameters {
            stringParam('projectPath', "${defaultWorkPath}Project/${project.name}", '打包项目所在的目录，不存在通过url拉取')
            stringParam('outputPath', "${defaultOutputPath}", '打包的输出目录')
            activeChoiceParam('buildPlatform') {
                description('选择打包平台')
                choiceType('SINGLE_SELECT')
                groovyScript {
                    script(generateActiveChoiceScript('0,1,2', project.buildPlatform, 'Windows64,Android,iOS'))
                    fallbackScript('return ["0"]')
                }
            }
            activeChoiceParam('buildMode') {
                description('选择打包模式')
                choiceType('SINGLE_SELECT')
                groovyScript {
                    script(generateActiveChoiceScript('0,1,2', '0', '全量打包,不打包AssetBundle直接Build,打空包'))
                    fallbackScript('return ["0"]')
                }
            }
            booleanParam('enableUnityDevelopment',false,'开启unity的developmentbuild')
            booleanParam('enableGameDevelopment',false,'Game的开发者模式,指代码的逻辑是开发者模式')
            activeChoiceParam('versionControl') {
                description('版本控制软件')
                choiceType('SINGLE_SELECT')
                groovyScript {
                    script(generateActiveChoiceScript('0,1', '0', 'Git(需要安装Git),SVN(需要安装SVN并有SVN命令可用)'))
                    fallbackScript('return ["0"]')
                }
            }
            stringParam('scmUrl',dsl_scmUrl,'项目url(git|svn),直接填url或者执行git填url|branch')
            booleanParam('enableProjectUpdate',true,'使用Git或者SVN更新项目')
            booleanParam('enableBuildExcel',true,'是否导表')
            booleanParam('enableIncrement',false,'是否是增量打包')
            activeChoiceParam('androidBuildOption') {
                description('打包特殊选项')
                choiceType('SINGLE_SELECT')
                groovyScript {
                    script(generateActiveChoiceScript('0,1,2,3,4,5', '3', 'Mono,Il2cpp64,AABMode,Il2cpp64AndX86,Il2cpp32,AABAndX86'))
                    fallbackScript('return ["3"]')
                }
            }
            stringParam('versionNumber', '1.0.0.0', '打包版本(前三位为app版本,最后一位资源)')
            stringParam('iOSBundleVersion', '0', 'iOS构建版本号(数字)')
            activeChoiceParam('iOSSigningType') {
                description('iOS出包证书签名类型，可以多选')
                choiceType('CHECKBOX')
                groovyScript {
                    script(generateActiveChoiceScript('1,2,3', '1,2', 'appstore发布包,development开发者包,企业证书包'))
                    fallbackScript('return ["1"]')
                }
            }
            booleanParam('iOSIpaResign', true, 'iOS打包多个证书包时，后面的包使用重签名的方式加速')
            booleanParam('SkipUnityBuild', false, '跳过Unity打包,例如只测试Xcode打包')
        }

        definition {
            cps {
                script(readFileFromWorkspace('Tools/AutoBuild/scripts/unity_pipeline.groovy'))
                sandbox()
            }
        }
    }
}

listView("${dsl_pipelineName}-view") {
    //description('All unstable jobs for project A')
    filterBuildQueue()
    filterExecutors()
    jobs {
        //name('trunk-Pipeline')
        regex(/${dsl_pipelineName}-.+/)
    }
    columns {
        status()
        weather()
        name()
        lastSuccess()
        lastFailure()
        lastDuration()
        buildButton()
    }
}
