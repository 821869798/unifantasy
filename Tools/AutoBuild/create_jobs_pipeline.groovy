pipeline {
    agent any
    stages {
        stage('拉取脚本工程') {
            steps {
                git branch: 'jenkins_ci', url: 'https://github.com/821869798/unifantasy.git'
            }
        }
        stage('使用dsl创建jobs') {
            steps {
                jobDsl sandbox: true, targets: 'Tools/AutoBuild/create_jobs_dsl.groovy'
            }
        }
    }
}
