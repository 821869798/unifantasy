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

pipelineJob('trunk-Pipeline') {
    // Define job properties
    description('trunk-Pipeline')
    //job parameters
    parameters {

        stringParam('projectPath', 'D:\\program\\my\\JenkinsUnityAutoBuild', '打包项目所在的目录，不存在通过url拉取')
        stringParam('outputPath', 'D:\\program\\my\\testout', '打包的输出目录')
        extendedChoice SimpleExtendedChoice('buildPlatform','PT_SINGLE_SELECT',3,',','0,1,2','0','Windows64,Android,iOS','选择打包平台')
        extendedChoice SimpleExtendedChoice('buildMode','PT_SINGLE_SELECT',3,',','0,1,2','0','全量打包,不打包AssetBundle，直接Build,打空包','选择打包模式')
        booleanParam('enableUnityDevelopment',false,'开启unity的developmentbuild')
        booleanParam('enableGameDevelopment',false,'Game的开发者模式,指代码的逻辑是开发者模式')
        extendedChoice SimpleExtendedChoice('versionControl','PT_SINGLE_SELECT',2,',','0,1','0','Git(需要安装Git),SVN(需要安装SVN，并有SVN命令可用)','版本控制软件')
        booleanParam('enableforceSetpupHybridCLR',false,'强制Setup一次HybridCLR')
        stringParam('scmUrl','','项目url(git|svn),直接填url或者执行git填url|branch')
        booleanParam('enableProjectUpdate',true,'使用Git或者SVN更新项目')
        booleanParam('enableBuildExcel',true,'是否导表')
        booleanParam('enableIncrement',false,'是否是增量打包')
        extendedChoice SimpleExtendedChoice('androidBuildOption','PT_SINGLE_SELECT',6,',','0,1,2,3,4,5','1','Mono,Il2cpp64,AABMode,Il2cpp64AndX86,Il2cpp32,AABAndX86','打包特殊选项')
		stringParam('versionNumber', '1.0.0.0', '打包版本')
		
    }

    definition {
        cps {
            script(readFileFromWorkspace('Tools/AutoBuild/unity_pipeline.groovy'))
            sandbox()
        }
    }
}

listView('trunk-view') {
    //description('All unstable jobs for project A')
    filterBuildQueue()
    filterExecutors()
    jobs {
        //name('trunk-Pipeline')
        regex(/trunk-.+/)
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
