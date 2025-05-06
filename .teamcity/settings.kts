import jetbrains.buildServer.configs.kotlin.*
import jetbrains.buildServer.configs.kotlin.buildFeatures.approval
import jetbrains.buildServer.configs.kotlin.buildFeatures.commitStatusPublisher
import jetbrains.buildServer.configs.kotlin.buildFeatures.perfmon
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetBuild
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetNugetPush
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetRestore
import jetbrains.buildServer.configs.kotlin.buildSteps.dotnetTest
import jetbrains.buildServer.configs.kotlin.buildSteps.script
import jetbrains.buildServer.configs.kotlin.failureConditions.BuildFailureOnMetric
import jetbrains.buildServer.configs.kotlin.failureConditions.failOnMetricChange
import jetbrains.buildServer.configs.kotlin.projectFeatures.githubIssues
import jetbrains.buildServer.configs.kotlin.triggers.vcs

/*
The settings script is an entry point for defining a TeamCity
project hierarchy. The script should contain a single call to the
project() function with a Project instance or an init function as
an argument.

VcsRoots, BuildTypes, Templates, and subprojects can be
registered inside the project using the vcsRoot(), buildType(),
template(), and subProject() methods respectively.

To debug settings scripts in command-line, run the

    mvnDebug org.jetbrains.teamcity:teamcity-configs-maven-plugin:generate

command and attach your debugger to the port 8000.

To debug in IntelliJ Idea, open the 'Maven Projects' tool window (View
-> Tool Windows -> Maven Projects), find the generate task node
(Plugins -> teamcity-configs -> teamcity-configs:generate), the
'Debug' option is available in the context menu for the task.
*/

version = "2025.03"

project {
    description = "Core library package for other Git2SemVer projects"

    buildType(BuildAndTest)
    buildType(DeployLocalTeamCityPackage)

    features {
        githubIssues {
            id = "PROJECT_EXT_4"
            displayName = "noetictools/git2semver.core"
            repositoryURL = "https://github.com/noetictools/git2semver.core"
            param("tokenId", "")
        }
    }
}

object BuildAndTest : BuildType({
    name = "Build and test"

    artifactRules = """
        +:Core/nupkg/NoeticTools.*.nupkg
        +:TestingCore/nupkg/NoeticTools.*.nupkg
        +:SolutionVersioningProject/obj/Git2SemVer.MSBuild.log
    """.trimIndent()

    vcs {
        root(DslContext.settingsRoot)

        cleanCheckout = true
    }

    steps {
        script {
            name = "Clear NuGet caches"
            id = "Clear_NuGet_caches_1"
            enabled = false
            scriptContent = "dotnet nuget locals all --clear"
        }
        dotnetRestore {
            name = "Restore - release/feature build"
            id = "Restore"

            conditions {
                matches("teamcity.build.branch", """^(main|(releases?|features?)(\/?.*?))${'$'}""")
            }
            sources = "https://api.nuget.org/v3/index.json"
        }
        dotnetRestore {
            name = "Restore - dev builds"
            id = "NoeticTools_Common_Restore_Dev"

            conditions {
                doesNotMatch("teamcity.build.branch", """^(main|(releases?|features?)(\/?.*?))${'$'}""")
            }
            sources = """
                https://api.nuget.org/v3/index.json
                http://10.1.10.78:8111/guestAuth/app/nuget/feed/_Root/TeamCity/v3/index.json
            """.trimIndent()
        }
        dotnetBuild {
            name = "Build"
            id = "dotnet"
            configuration = "Release"
            args = "-p:Git2SemVer_BuildNumber=%build.number% --verbosity normal"
        }
        dotnetTest {
            name = "Test"
            id = "dotnet_1"
            param("dotNetCoverage.dotCover.filters", """
                +:NoeticTools.*
                -:NoeticTools.*Tests
            """.trimIndent())
        }
    }

    triggers {
        vcs {
        }
    }

    failureConditions {
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.ARTIFACT_SIZE
            threshold = 25
            units = BuildFailureOnMetric.MetricUnit.PERCENTS
            comparison = BuildFailureOnMetric.MetricComparison.LESS
            compareTo = build {
                buildRule = lastSuccessful()
            }
        }
    }

    features {
        perfmon {
        }
        commitStatusPublisher {
            vcsRootExtId = "${DslContext.settingsRoot.id}"
            publisher = github {
                githubUrl = "https://api.github.com"
                authType = storedToken {
                    tokenId = "tc_token_id:CID_3de2c2727993edab40e4371046ac9db7:-1:5bff2d1b-5e00-4af5-9685-97f5e3833cad"
                }
            }
        }
    }

    requirements {
        exists("DotNetCLI_Path")
        exists("env.TEAMCITY_GIT_PATH")
    }
})

object DeployLocalTeamCityPackage : BuildType({
    name = "Deploy (local TeamCity) - package"
    description = "Deploy NuGet package"

    enablePersonalBuilds = false
    type = BuildTypeSettings.Type.DEPLOYMENT
    buildNumberPattern = "%build.counter% (${BuildAndTest.depParamRefs.buildNumber})"
    maxRunningBuilds = 1

    steps {
        dotnetNugetPush {
            name = "Push NuGet package"
            id = "Publish2"
            packages = "NoeticTools.*.nupkg"
            serverUrl = "http://10.1.10.78:8111/httpAuth/app/nuget/feed/_Root/TeamCity/v3/index.json"
            apiKey = "credentialsJSON:bd18b974-1188-423d-9efd-8836806c3669"
        }
    }

    failureConditions {
        failOnMetricChange {
            metric = BuildFailureOnMetric.MetricType.ARTIFACT_SIZE
            threshold = 25
            units = BuildFailureOnMetric.MetricUnit.PERCENTS
            comparison = BuildFailureOnMetric.MetricComparison.LESS
            compareTo = build {
                buildRule = lastSuccessful()
            }
        }
    }

    features {
        approval {
            approvalRules = "user:robert"
            manualRunsApproved = false
        }
    }

    dependencies {
        artifacts(BuildAndTest) {
            buildRule = lastSuccessful("""
                +:<default>
                +:*
            """.trimIndent())
            cleanDestination = true
            artifactRules = "+:NoeticTools.Git2SemVer.Core.*.nupkg"
        }
    }
})
