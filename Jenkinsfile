// 원정대 캠프 QA 파이프라인
// 빠른 검사를 앞에 두고, 오래 걸리는 Unity 실행을 뒤에 둡니다.

def py = ''

def runCmd(String command) {
    if (isUnix()) {
        sh command
    } else {
        bat command
    }
}

pipeline {
    agent any

    options {
        timestamps()
        timeout(time: 60, unit: 'MINUTES')
    }

    stages {
        stage('0 준비') {
            steps {
                dir('reports') {
                    deleteDir()
                }
                script {
                    if (isUnix()) {
                        sh 'python3 -m venv qa/.venv'
                        py = 'qa/.venv/bin/python'
                    } else {
                        bat 'py -m venv qa\\.venv'
                        py = 'qa\\.venv\\Scripts\\python'
                    }
                    runCmd("${py} -m pip install -q -r qa/requirements.txt")
                }
            }
        }

        stage('1 데이터 검사') {
            steps {
                dir('qa') {
                    script {
                        def qaPy = isUnix() ? '.venv/bin/python' : '.venv\\Scripts\\python'
                        runCmd("${qaPy} -m pytest tests/test_tables.py --junitxml=../reports/junit_tables.xml")
                    }
                }
            }
        }

        stage('2 프로젝트 파일 검사') {
            steps {
                dir('qa') {
                    script {
                        def qaPy = isUnix() ? '.venv/bin/python' : '.venv\\Scripts\\python'
                        runCmd("${qaPy} -m pytest tests/test_project_files.py --junitxml=../reports/junit_project_files.xml")
                    }
                }
            }
        }
    }

    // 6 결과 보고: 어느 단계에서 멈춰도 그때까지의 결과를 Jenkins 화면에 남깁니다.
    post {
        always {
            junit allowEmptyResults: true, testResults: 'reports/junit_*.xml'
            archiveArtifacts allowEmptyArchive: true, artifacts: 'reports/*.log'
        }
    }
}
