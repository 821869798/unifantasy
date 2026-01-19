pipeline {
    agent any
    stages {
        stage('拉取脚本工程') {
            steps {
                // git拉取工程
                git branch: 'jenkins_ci', url: 'https://github.com/821869798/unifantasy.git'
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
                    
                    def dslParams = [
                        dsl_pipelineName: 'trunk',
                        dsl_scmUrl: 'https://github.com/821869798/unifantasy.git|jenkins_ci',
                        dsl_parentFolder: currentFolder
                    ]
                    jobDsl sandbox: true, targets: 'Tools/AutoBuild/scripts/create_jobs_dsl.groovy', additionalParameters: dslParams
                }
            }
        }
    }
}
