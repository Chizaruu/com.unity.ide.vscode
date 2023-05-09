using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Unity.CodeEditor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace VSCodeEditor
{
    [InitializeOnLoad]
    public class VSCodeScriptEditor : IExternalCodeEditor
    {
        const string vscode_argument = "vscode_arguments";
        const string vscode_extension = "vscode_userExtensions";
        string m_EditorArguments;

        bool m_ShowEditorSection = false;
        bool m_ShowConfigSection = false;
        bool m_ShowProjectSection = true;

        readonly IDiscovery m_Discoverability;
        readonly IGenerator m_ProjectGeneration;
        readonly IConfigGenerator m_ConfigGeneration;

        static readonly string[] k_SupportedFileNames =
        {
            "code.exe",
            "visualstudiocode.app",
            "visualstudiocode-insiders.app",
            "vscode.app",
            "code.app",
            "code.cmd",
            "code-insiders.cmd",
            "code",
            "com.visualstudio.code"
        };

        static bool IsOSX => Application.platform == RuntimePlatform.OSXEditor;

        static string DefaultApp => EditorPrefs.GetString("kScriptsDefaultApp");

        static string ExternalEditorDefaultArgument { get; } =
            "\"$(ProjectPath)\" -g \"$(File)\":$(Line):$(Column)";

        static string ExternalEditorWorkplaceDefaultArgument { get; } =
            "\"$(ProjectPath)/$(ProjectName).code-workspace\" -g \"$(File)\":$(Line):$(Column)";

        string EditorArguments
        {
            get =>
                m_EditorArguments ??= EditorPrefs.GetString(
                    vscode_argument,
                    ExternalEditorDefaultArgument
                );
            set
            {
                m_EditorArguments = value;
                EditorPrefs.SetString(vscode_argument, value);
            }
        }

        bool ShowEditorSection
        {
            get => m_ShowEditorSection || EditorPrefs.GetBool("vscode_showEditorSection", false);
            set
            {
                m_ShowEditorSection = value;
                EditorPrefs.SetBool("vscode_showEditorSection", value);
            }
        }

        bool ShowConfigSection
        {
            get => m_ShowConfigSection || EditorPrefs.GetBool("vscode_showConfigSection", false);
            set
            {
                m_ShowConfigSection = value;
                EditorPrefs.SetBool("vscode_showConfigSection", value);
            }
        }

        bool ShowProjectSection
        {
            get => m_ShowProjectSection || EditorPrefs.GetBool("vscode_showProjectSection", false);
            set
            {
                m_ShowProjectSection = value;
                EditorPrefs.SetBool("vscode_showProjectSection", value);
            }
        }

        static string[] DefaultExtensions
        {
            get
            {
                var customExtensions = new[] { "json", "asmdef", "log" };
                return EditorSettings.projectGenerationBuiltinExtensions
                    .Concat(EditorSettings.projectGenerationUserExtensions)
                    .Concat(customExtensions)
                    .Distinct()
                    .ToArray();
            }
        }

        static string[] HandledExtensions
        {
            get
            {
                return HandledExtensionsString
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.TrimStart('.', '*'))
                    .ToArray();
            }
        }

        static string HandledExtensionsString
        {
            get => EditorPrefs.GetString(vscode_extension, string.Join(";", DefaultExtensions));
            set => EditorPrefs.SetString(vscode_extension, value);
        }

        public bool TryGetInstallationForPath(
            string editorPath,
            out CodeEditor.Installation installation
        )
        {
            var lowerCasePath = editorPath.ToLower();
            var filename = Path.GetFileName(lowerCasePath).Replace(" ", "");
            var installations = Installations;
            if (!k_SupportedFileNames.Contains(filename))
            {
                installation = default;
                return false;
            }

            if (!installations.Any())
            {
                installation = new CodeEditor.Installation
                {
                    Name = "Visual Studio Code",
                    Path = editorPath
                };
            }
            else
            {
                try
                {
                    installation = installations.First(inst => inst.Path == editorPath);
                }
                catch (InvalidOperationException)
                {
                    installation = new CodeEditor.Installation
                    {
                        Name = "Visual Studio Code",
                        Path = editorPath
                    };
                }
            }

            return true;
        }

        public void OnGUI()
        {
            RenderEditorSection();
            RenderConfigSection();
            RenderProjectSection();

            HandledExtensionsString = EditorGUILayout.TextField(
                new GUIContent("Extensions handled: "),
                HandledExtensionsString
            );
        }

        void RenderEditorSection()
        {
            ShowEditorSection = EditorGUILayout.BeginFoldoutHeaderGroup(
                ShowEditorSection,
                "Configure Editor Script Editor Arguments:"
            );

            if (ShowEditorSection)
            {
                EditorGUI.indentLevel++;
                EditorArguments = EditorGUILayout.TextField(
                    "External Script Editor Args",
                    EditorArguments
                );
                FlagButton(
                    ArgumentFlag.EditorArgument,
                    "Use Code-Workspace",
                    "",
                    (handler, flag) => handler.ArgumentFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleArgument(flag)
                );
                RegenerateButton("Regenerate editor arguments");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void RenderConfigSection()
        {
            ShowConfigSection = EditorGUILayout.BeginFoldoutHeaderGroup(
                ShowConfigSection,
                "Generate config files for:"
            );

            if (ShowConfigSection)
            {
                EditorGUI.indentLevel++;
                FlagButton(
                    ConfigFlag.Workspace,
                    "Workspace",
                    "",
                    (handler, flag) => handler.ConfigFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleConfig(flag)
                );
                FlagButton(
                    ConfigFlag.OmniSharp,
                    "OmniSharp",
                    "",
                    (handler, flag) => handler.ConfigFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleConfig(flag)
                );
                FlagButton(
                    ConfigFlag.EditorConfig,
                    "EditorConfig",
                    "",
                    (handler, flag) => handler.ConfigFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleConfig(flag)
                );

                RegenerateButton("Regenerate config files");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void RenderProjectSection()
        {
            ShowProjectSection = EditorGUILayout.BeginFoldoutHeaderGroup(
                ShowProjectSection,
                "Generate .csproj files for:"
            );

            if (ShowProjectSection)
            {
                EditorGUI.indentLevel++;
                FlagButton(
                    ProjectGenerationFlag.Embedded,
                    "Embedded packages",
                    "",
                    (handler, flag) => handler.ProjectGenerationFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleProjectGeneration(flag)
                );
                FlagButton(
                    ProjectGenerationFlag.Local,
                    "Local packages",
                    "",
                    (handler, flag) => handler.ProjectGenerationFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleProjectGeneration(flag)
                );
                FlagButton(
                    ProjectGenerationFlag.Registry,
                    "Registry packages",
                    "",
                    (handler, flag) => handler.ProjectGenerationFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleProjectGeneration(flag)
                );
                FlagButton(
                    ProjectGenerationFlag.Git,
                    "Git packages",
                    "",
                    (handler, flag) => handler.ProjectGenerationFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleProjectGeneration(flag)
                );
                FlagButton(
                    ProjectGenerationFlag.BuiltIn,
                    "Built-in packages",
                    "",
                    (handler, flag) => handler.ProjectGenerationFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleProjectGeneration(flag)
                );
                FlagButton(
                    ProjectGenerationFlag.LocalTarBall,
                    "Local tarball",
                    "",
                    (handler, flag) => handler.ProjectGenerationFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleProjectGeneration(flag)
                );
                FlagButton(
                    ProjectGenerationFlag.Unknown,
                    "Packages from unknown sources",
                    "",
                    (handler, flag) => handler.ProjectGenerationFlag.HasFlag(flag),
                    (handler, flag) => handler.ToggleProjectGeneration(flag)
                );
                RegenerateButton("Regenerate project files");
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void FlagButton<T>(
            T flag,
            string guiMessage,
            string toolTip,
            Func<IFlagHandler, T, bool> flagGetter,
            Action<IFlagHandler, T> flagToggler
        )
            where T : Enum
        {
            var previousValue = flagGetter(m_ConfigGeneration.FlagHandler, flag);
            var currentValue = EditorGUILayout.Toggle(
                new GUIContent(guiMessage, toolTip),
                previousValue
            );
            if (currentValue != previousValue)
            {
                flagToggler(m_ConfigGeneration.FlagHandler, flag);
            }
        }

        void RegenerateButton(string guiMessage)
        {
            var rect = EditorGUI.IndentedRect(
                EditorGUILayout.GetControlRect(new GUILayoutOption[] { })
            );
            rect.width = 252;
            if (GUI.Button(rect, new GUIContent(guiMessage)))
            {
                switch (guiMessage)
                {
                    case "Regenerate editor arguments":
                        if (
                            m_ConfigGeneration.FlagHandler.ArgumentFlag.HasFlag(
                                ArgumentFlag.EditorArgument
                            )
                        )
                        {
                            EditorArguments = ExternalEditorWorkplaceDefaultArgument;
                        }
                        else
                        {
                            EditorArguments = ExternalEditorDefaultArgument;
                        }
                        break;
                    case "Regenerate config files":
                        m_ConfigGeneration.Sync();
                        break;
                    case "Regenerate project files":
                        m_ProjectGeneration.Sync();
                        break;
                    default:
                        UnityEngine.Debug.LogError("Unknown button pressed");
                        break;
                }
            }
        }

        public void CreateIfDoesntExist()
        {
            if (!m_ProjectGeneration.SolutionExists())
            {
                m_ProjectGeneration.Sync();
            }

            if (!m_ConfigGeneration.TskFileExists())
            {
                m_ConfigGeneration.Sync(true);
            }
        }

        public void SyncIfNeeded(
            string[] addedFiles,
            string[] deletedFiles,
            string[] movedFiles,
            string[] movedFromFiles,
            string[] importedFiles
        )
        {
            (
                m_ProjectGeneration.AssemblyNameProvider as IPackageInfoCache
            )?.ResetPackageInfoCache();
            m_ProjectGeneration.SyncIfNeeded(
                addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles).ToList(),
                importedFiles
            );
        }

        public void SyncAll()
        {
            (
                m_ProjectGeneration.AssemblyNameProvider as IPackageInfoCache
            )?.ResetPackageInfoCache();
            AssetDatabase.Refresh();
            m_ProjectGeneration.Sync();
        }

        public bool OpenProject(string path, int line, int column)
        {
            if (path != "" && (!SupportsExtension(path) || !File.Exists(path))) // Assets - Open C# Project passes empty path here
            {
                return false;
            }

            if (line == -1)
                line = 1;
            if (column == -1)
                column = 0;

            var workspacePath =
                $"{m_ProjectGeneration.ProjectDirectory}/{Path.GetFileName(m_ProjectGeneration.ProjectDirectory)}.code-workspace";

            string arguments;
            if (
                EditorArguments != ExternalEditorDefaultArgument
                && EditorArguments != ExternalEditorWorkplaceDefaultArgument
            )
            {
                arguments =
                    m_ProjectGeneration.ProjectDirectory != path
                        ? CodeEditor.ParseArgument(EditorArguments, path, line, column)
                        : workspacePath;
            }
            else
            {
                arguments = m_ConfigGeneration.FlagHandler.ArgumentFlag.HasFlag(
                    ArgumentFlag.EditorArgument
                )
                    ? $@"""{workspacePath}"""
                    : $@"""{m_ProjectGeneration.ProjectDirectory}""";
                if (m_ProjectGeneration.ProjectDirectory != path && path.Length != 0)
                {
                    arguments += $@" -g ""{path}"":{line}:{column}";
                }
            }

            if (IsOSX)
            {
                return OpenOSX(arguments);
            }

            var app = DefaultApp;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = app,
                    Arguments = arguments,
                    WindowStyle = app.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase)
                        ? ProcessWindowStyle.Hidden
                        : ProcessWindowStyle.Normal,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                }
            };

            process.Start();
            return true;
        }

        static bool OpenOSX(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"-n \"{DefaultApp}\" --args {arguments}",
                    UseShellExecute = true,
                }
            };

            process.Start();
            return true;
        }

        static bool SupportsExtension(string path)
        {
            var extension = Path.GetExtension(path);
            if (string.IsNullOrEmpty(extension))
                return false;
            return HandledExtensions.Contains(extension.TrimStart('.'));
        }

        public CodeEditor.Installation[] Installations => m_Discoverability.PathCallback();

        public VSCodeScriptEditor(
            IDiscovery discovery,
            IGenerator projectGeneration,
            IConfigGenerator configGeneration
        )
        {
            m_Discoverability = discovery;
            m_ProjectGeneration = projectGeneration;
            m_ConfigGeneration = configGeneration;
        }

        static VSCodeScriptEditor()
        {
            string projectDirectory = Directory.GetParent(Application.dataPath).FullName;

            var editor = new VSCodeScriptEditor(
                new VSCodeDiscovery(),
                new ProjectGeneration(projectDirectory),
                new ConfigGeneration(projectDirectory)
            );
            CodeEditor.Register(editor);

            if (IsVSCodeInstallation(CodeEditor.CurrentEditorInstallation))
            {
                editor.CreateIfDoesntExist();
            }
        }

        static bool IsVSCodeInstallation(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            var lowerCasePath = path.ToLower();
            var filename = Path.GetFileName(
                    lowerCasePath
                        .Replace('\\', Path.DirectorySeparatorChar)
                        .Replace('/', Path.DirectorySeparatorChar)
                )
                .Replace(" ", "");
            return k_SupportedFileNames.Contains(filename);
        }

        public void Initialize(string editorInstallationPath) { }
    }
}