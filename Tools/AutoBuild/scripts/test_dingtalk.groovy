pipeline {
   agent any

   stages {
      stage('Hello') {
         steps {
            script {
              println("打包中")
            }
                   
         }
      }
   }


	post {
    success {
      script {
        dingtalk (
            robot: 'unity',
            atAll: false,
            at: [], // 指定at的人,填入手机号
            type:'MARKDOWN',
            title: "success: ${JOB_NAME}",
            text: ["- <font color=green>构建成功</font>:${JOB_NAME}项目!\n- 持续时间:${currentBuild.durationString}\n- 构建名:${currentBuild.displayName}"]
        )
      }
    }
    failure {
      script {
        dingtalk (
            robot: 'unity',
            atAll: false,
            at: [],
            type:'MARKDOWN',
            title: "fail: ${JOB_NAME}",
            text: ["- <font color=Red>构建失败</font>:${JOB_NAME}项目!\n- 持续时间:${currentBuild.durationString}\n- 构建名:${currentBuild.displayName}"]
        )
      }
    }
  }

}