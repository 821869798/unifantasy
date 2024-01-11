import org.apache.commons.lang.SystemUtils

// dsl中没法用isUnix函数，只能自己写一个
def DSL_IsUnix() {
    if (SystemUtils.IS_OS_UNIX || SystemUtils.IS_OS_MAC) {
        return true;
    }
    else {
        return false
    }
}


//定义choice的函数
def SimpleExtendedChoice(n, t, count, delimiter, v, dv, descValue, desc) {
    def choice = {
        name(n)
        type(t)
        value(v)
        defaultValue(dv)
        visibleItemCount(count)
        multiSelectDelimiter(delimiter)
        descriptionPropertyValue(descValue)
        description(desc)
        saveJSONParameterToFile(false)
        quoteValue(false)
        // The name of the parameter.
        projectName('')
        propertyFile('')
        groovyScript('')
        groovyScriptFile('')
        bindings('')
        groovyClasspath('')
        propertyKey('')
        defaultPropertyFile('')
        defaultGroovyScript('')
        defaultGroovyScriptFile('')
        defaultBindings('')
        defaultGroovyClasspath('')
        defaultPropertyKey('')
        descriptionPropertyFile('')
        descriptionGroovyScript('')
        descriptionGroovyScriptFile('')
        descriptionBindings('')
        descriptionGroovyClasspath('')
        descriptionPropertyKey('')
        javascriptFile('')
        javascript('')
    }
    choice
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
def defaultWorkPath = "D:\\program\\my\\";
if (DSL_IsUnix()) {
    // macos linux，获取User目录
    def userHome = System.getProperty('user.home')
    // 下面这行代码效果是一样的，dsl中能直接用env，需要用System.getenv
    // def userHome = System.getenv('HOME')
    defaultWorkPath = "${userHome}/program/my/";
}

// TODO，推荐把输出目录修改成打包机本地挂载的局域网共享盘（windows和macos都支持挂载的），这样远程打包完直接去共享盘取。
def defaultOutputPath = defaultWorkPath;

projects.each { project ->
    pipelineJob("${project.name}") {
        // Define job properties
        description("${project.description}")
        //job parameters
        parameters {
            stringParam('projectPath', "${defaultWorkPath}${project.name}", '打包项目所在的目录，不存在通过url拉取')
            stringParam('outputPath', "${defaultOutputPath}testout", '打包的输出目录')
            extendedChoice SimpleExtendedChoice('buildPlatform','PT_SINGLE_SELECT',3,',','0,1,2',project.buildPlatform,'Windows64,Android,iOS','选择打包平台')
            extendedChoice SimpleExtendedChoice('buildMode','PT_SINGLE_SELECT',3,',','0,1,2','0','全量打包,不打包AssetBundle，直接Build,打空包','选择打包模式')
            booleanParam('enableUnityDevelopment',false,'开启unity的developmentbuild')
            booleanParam('enableGameDevelopment',false,'Game的开发者模式,指代码的逻辑是开发者模式')
            extendedChoice SimpleExtendedChoice('versionControl','PT_SINGLE_SELECT',2,',','0,1','0','Git(需要安装Git),SVN(需要安装SVN，并有SVN命令可用)','版本控制软件')
            stringParam('scmUrl',dsl_scmUrl,'项目url(git|svn),直接填url或者执行git填url|branch')
            booleanParam('enableProjectUpdate',true,'使用Git或者SVN更新项目')
            booleanParam('enableBuildExcel',true,'是否导表')
            booleanParam('enableIncrement',false,'是否是增量打包')
            extendedChoice SimpleExtendedChoice('androidBuildOption','PT_SINGLE_SELECT',6,',','0,1,2,3,4,5','3','Mono,Il2cpp64,AABMode,Il2cpp64AndX86,Il2cpp32,AABAndX86','打包特殊选项')
            stringParam('versionNumber', '1.0.0.0', '打包版本')
            
        }

        definition {
            cps {
                script(readFileFromWorkspace('Tools/AutoBuild/unity_pipeline.groovy'))
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
