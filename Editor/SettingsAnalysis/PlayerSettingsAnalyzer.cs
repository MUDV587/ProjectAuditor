using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.ProjectAuditor.Editor.Core;
using Unity.ProjectAuditor.Editor.Diagnostic;
using Unity.ProjectAuditor.Editor.Modules;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.ProjectAuditor.Editor.SettingsAnalysis
{
    class PlayerSettingsAnalyzer : ISettingsModuleAnalyzer
    {
        internal const string PAS0002 = nameof(PAS0002);
        internal const string PAS0029 = nameof(PAS0029);
        internal const string PAS0033 = nameof(PAS0033);

        static readonly Descriptor k_AccelerometerDescriptor = new Descriptor(
            PAS0002,
            "Player (iOS): Accelerometer",
            new[] { Area.CPU },
            "The Accelerometer is enabled in iOS Player Settings.",
            "Consider setting <b>Accelerometer Frequency</b> to Disabled if your application doesn't make use of the device's accelerometer. Disabling this option will save a tiny amount of CPU processing time.")
        {
            platforms = new[] { BuildTarget.iOS.ToString() }
        };

        static readonly Descriptor k_SplashScreenDescriptor = new Descriptor(
            PAS0029,
            "Player: Splash Screen",
            new[] { Area.LoadTime },
            "<b>Splash Screen</b> is enabled and will increase the time it takes to load into the first scene.",
            "Disable the Splash Screen option in <b>Project Settings ➔ Player ➔ Splash Image ➔ Show Splash Screen</b>.");

        static readonly Descriptor k_SpeakerModeDescriptor = new Descriptor(
            PAS0033,
            "Audio: Speaker Mode",
            new[] { Area.BuildSize },
            "<b>UnityEngine.AudioSettings.speakerMode</b> is not set to <b>UnityEngine.AudioSpeakerMode.Mono</b>. The generated build will be larger than necessary.",
            "To reduce runtime memory consumption of AudioClips change <b>Project Settings ➔ Audio ➔ Default Speaker Mode</b> to <b>Mono</b>. This will half memory usage of stereo AudioClips. It is also recommended considering enabling the <b>Force To Mono</b> AudioClip import setting to reduce import times and build size.")
        {
            platforms = new[] { "Android", "iOS"},
            fixer = (issue => {
                FixSpeakerMode();
            })
        };

        public void Initialize(ProjectAuditorModule module)
        {
            module.RegisterDescriptor(k_AccelerometerDescriptor);
            module.RegisterDescriptor(k_SplashScreenDescriptor);
            module.RegisterDescriptor(k_SpeakerModeDescriptor);
        }

        public IEnumerable<ProjectIssue> Analyze(ProjectAuditorParams projectAuditorParams)
        {
            if (k_AccelerometerDescriptor.platforms.Contains(projectAuditorParams.platform.ToString()) && IsAccelerometerEnabled())
            {
                yield return ProjectIssue.Create(IssueCategory.ProjectSetting, k_AccelerometerDescriptor)
                    .WithLocation("Project/Player");
            }
            if (IsSplashScreenEnabledAndCanBeDisabled())
            {
                yield return ProjectIssue.Create(IssueCategory.ProjectSetting, k_SplashScreenDescriptor)
                    .WithLocation("Project/Player");
            }
            if (!IsSpeakerModeMono())
            {
                yield return ProjectIssue.Create(IssueCategory.ProjectSetting, k_SpeakerModeDescriptor)
                    .WithLocation("Project/Player");
            }
        }

        internal static bool IsAccelerometerEnabled()
        {
            return PlayerSettings.accelerometerFrequency != 0;
        }

        internal static bool IsSplashScreenEnabledAndCanBeDisabled()
        {
            if (!PlayerSettings.SplashScreen.show)
                return false;
            var type = Type.GetType("UnityEditor.PlayerSettingsSplashScreenEditor,UnityEditor.dll");
            if (type == null)
                return false;

            var licenseAllowsDisablingProperty = type.GetProperty("licenseAllowsDisabling", BindingFlags.Static | BindingFlags.NonPublic);
            if (licenseAllowsDisablingProperty == null)
                return false;
            return (bool)licenseAllowsDisablingProperty.GetValue(null, null);
        }

        internal static bool IsSpeakerModeMono()
        {
            return AudioSettings.GetConfiguration().speakerMode == AudioSpeakerMode.Mono;
        }

        internal static void FixSpeakerMode()
        {
            AudioConfiguration audioConfiguration = new AudioConfiguration
            {
                speakerMode = AudioSpeakerMode.Mono
            };

            AudioSettings.Reset(audioConfiguration);
        }
    }
}
