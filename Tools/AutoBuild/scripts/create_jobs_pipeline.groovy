pipeline {
    agent any
    stages {
        stage('拉取脚本工程') {
            steps {
                // git拉取工程
                git branch: 'main', url: 'https://github.com/821869798/unifantasy.git'
                // svn拉取工程
                //svn credentialsId: 'your-svn-credentials-id', url: 'http://your-svn-repository-url'
            }
        }
        stage('使用dsl创建jobs') {
            steps {
                script {
                    // 获取当前 job 所在的文件夹路径
                    def currentFolder = ''
                    def jobName = env.JOB_NAME
                    if (jobName.contains('/')) {
                        currentFolder = jobName.substring(0, jobName.lastIndexOf('/'))
                    }
                    
                    // 获取默认工作目录（根据操作系统判断）
                    def defaultWorkPath = 'D:/JenkinsHome/'
                    if (isUnix()) {
                        // macOS 和 Linux，使用 HOME 环境变量
                        def userHome = env.HOME
                        defaultWorkPath = "${userHome}/JenkinsHome/"
                    }
                    
                    def dslParams = [
                        dsl_pipelineName: 'trunk',
                        dsl_scmUrl: 'https://github.com/821869798/unifantasy.git|main',
                        dsl_parentFolder: currentFolder,
                        dsl_defaultWorkPath: defaultWorkPath
                    ]
                    jobDsl sandbox: true, targets: 'Tools/AutoBuild/scripts/create_jobs_dsl.groovy', additionalParameters: dslParams
                }
            }
        }
    }
}
