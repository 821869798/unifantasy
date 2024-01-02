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
                    def dslParams = [
                        dsl_pipelineName: 'trunk',
                        dsl_scmUrl: 'https://github.com/821869798/unifantasy.git|main'
                    ]
                    jobDsl sandbox: true, targets: 'Tools/AutoBuild/create_jobs_dsl.groovy', additionalParameters: dslParams
                }
            }
        }
    }
}
