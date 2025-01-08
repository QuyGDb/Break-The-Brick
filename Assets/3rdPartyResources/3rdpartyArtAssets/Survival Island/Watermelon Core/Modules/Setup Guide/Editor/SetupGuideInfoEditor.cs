using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Watermelon
{
    [CustomEditor(typeof(SetupGuideInfo))]
    public class SetupGuideInfoEditor : Editor
    {    
        private static SetupGuideInfoEditor instance;

        private const string SITE_URL = @"https://wmelongames.com";

        private const string PROTOTYPE_URL = @"https://wmelongames.com/prototype/card.php";
        private const string MAIL_URL = "https://wmelongames.com/contact/";
        private const string DISCORD_URL = "https://discord.gg/xEGUnBg";
        private const string WATERMELON_CORE_FOLDER_NAME = "Watermelon Core";
        private const string CORE_CHANGELOG_PATH_SUFFIX = "/Core Changelog.txt";
        private const string DOCUMENTATION_PATH_SUFFIX = "/DOCUMENTATION.txt";
        private const string CHANGELOG_PATH_SUFFIX = "/Template Changelog.txt";
        private const string DEFAULT_VALUE = "[unknown]";
        private const string DOCUMENTATION_URL_PROPERTY_PATH = "documentationURL";
        private static readonly string PROJECT_DESCRIPTION = @"Thank you for purchasing {0}.\nBefore you start working with project, read the documentation.\nPlease, leave a review and rate the project.";

        private SetupGuideInfo setupGuideInfo;

        private GUIStyle descriptionStyle;
        private GUIStyle setupButtonStyle;
        private GUIStyle gameButtonStyle;
        private GUIStyle textGamesStyle;
        private GUIStyle logoStyle;
        private GUIStyle projectStyle;

        private GUIContent logoContent;
        private GUIContent mailButtonContent;
        private GUIContent discordButtonContent;
        private GUIContent documentationButtonContent;

        private string description;
        
        private static SetupButton[] setupButtons;
        private static FinishedProject finishedProject;

        private SerializedObject targetSerializedObject;

        private string coreVersion;
        private string projectVersion;
        private string documentationUrl;
        private float defaultLength;

        protected void OnEnable()
        {
            EditorCustomStyles.CheckStyles();

            instance = this;
            
            setupGuideInfo = target as SetupGuideInfo;
            targetSerializedObject = new SerializedObject(target);

            List<SetupButton> tempSetupButtons = new List<SetupButton>();
            if(!setupGuideInfo.windowButtons.IsNullOrEmpty())
                tempSetupButtons.AddRange(setupGuideInfo.windowButtons);
            if (!setupGuideInfo.folderButtons.IsNullOrEmpty())
                tempSetupButtons.AddRange(setupGuideInfo.folderButtons);
            if (!setupGuideInfo.fileButtons.IsNullOrEmpty())
                tempSetupButtons.AddRange(setupGuideInfo.fileButtons);

            setupButtons = tempSetupButtons.ToArray();

            string coreFolderPath =  EditorUtils.FindFolderPath(WATERMELON_CORE_FOLDER_NAME).Replace('\\','/');
            string coreChangelogPath = coreFolderPath + CORE_CHANGELOG_PATH_SUFFIX;
            string changelogPath = coreFolderPath.Substring(0, coreFolderPath.Length - 16) + CHANGELOG_PATH_SUFFIX; // 16 symbols in "/Watermelon Core"
            string documentationPath = coreFolderPath.Substring(0, coreFolderPath.Length - 16) + DOCUMENTATION_PATH_SUFFIX; // 16 symbols in "/Watermelon Core"

            try
            {
                using (System.IO.StreamReader fileReader = new System.IO.StreamReader(coreChangelogPath))
                {
                    coreVersion = fileReader.ReadLine();
                }
            }
            catch
            {
                coreVersion = DEFAULT_VALUE;
            }

            try
            {
                using (System.IO.StreamReader fileReader = new System.IO.StreamReader(changelogPath))
                {
                    projectVersion = fileReader.ReadLine();
                }
            }
            catch
            {
                projectVersion = DEFAULT_VALUE;
            }

            try
            {
                string[] lines = System.IO.File.ReadAllLines(documentationPath);
                string lastLine = lines[lines.Length - 1];
                documentationUrl = lastLine.Substring(lastLine.IndexOf("http"));

                SerializedObject serializedObject = new SerializedObject(target);
                serializedObject.FindProperty(DOCUMENTATION_URL_PROPERTY_PATH).stringValue = documentationUrl;
                serializedObject.ApplyModifiedProperties();

                AssetDatabase.SaveAssets();
            }
            catch
            {
                
            }

            PrepareStyles();
        }

        protected void PrepareStyles()
        {
            description = string.Format(PROJECT_DESCRIPTION, setupGuideInfo.gameName).Replace("\\n", "\n");

            logoContent = new GUIContent(EditorCustomStyles.GetIcon("logo", EditorGUIUtility.isProSkin ? new Color(1.0f, 1.0f, 1.0f) : new Color(0.2f, 0.2f, 0.2f)), SITE_URL);

            textGamesStyle = EditorCustomStyles.labelSmall.GetAligmentStyle(TextAnchor.MiddleCenter);
            textGamesStyle.alignment = TextAnchor.MiddleCenter;
            textGamesStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

            gameButtonStyle = EditorCustomStyles.button.GetPaddingStyle(new RectOffset(2, 2, 2, 2));

            descriptionStyle = new GUIStyle(EditorCustomStyles.Skin.label);
            descriptionStyle.wordWrap = true;

            setupButtonStyle = new GUIStyle(EditorCustomStyles.button);
            setupButtonStyle.imagePosition = ImagePosition.ImageAbove;
            setupButtonStyle.padding = new RectOffset(8, 8, 8, 8);

            mailButtonContent = new GUIContent(EditorCustomStyles.GetIcon("icon_mail"));
            discordButtonContent = new GUIContent(EditorCustomStyles.GetIcon("icon_discord"));
            documentationButtonContent = new GUIContent(EditorCustomStyles.ICON_SPACE + "Documentation", EditorCustomStyles.GetIcon("icon_documentation"));

            logoStyle = new GUIStyle(GUIStyle.none);
            logoStyle.alignment = TextAnchor.MiddleCenter;
            logoStyle.padding = new RectOffset(10, 10, 10, 10);

            projectStyle = new GUIStyle(EditorCustomStyles.Skin.label);
            projectStyle.alignment = TextAnchor.MiddleCenter;
            projectStyle.wordWrap = false;
            projectStyle.clipping = TextClipping.Overflow;

            for (int i = 0; i < setupButtons.Length; i++)
            {
                setupButtons[i].Init();
            }

            if (finishedProject == null)
            {
                EditorCoroutines.Execute(instance.GetRequest(PROTOTYPE_URL));
            }
            else
            {
                finishedProject.LoadTexture();
            }
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical(EditorCustomStyles.Skin.box);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(logoContent, logoStyle, GUILayout.Width(80), GUILayout.Height(80)))
            {
                Application.OpenURL(SITE_URL);
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal(EditorCustomStyles.padding05, GUILayout.Height(21), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("GREETINGS!", EditorCustomStyles.labelBold, GUILayout.ExpandHeight(true), GUILayout.Width(110));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(discordButtonContent, EditorCustomStyles.button, GUILayout.Width(22), GUILayout.Height(22)))
            {
                Application.OpenURL(DISCORD_URL);
            }

            if (GUILayout.Button(mailButtonContent, EditorCustomStyles.button, GUILayout.Width(22), GUILayout.Height(22)))
            {
                Application.OpenURL(MAIL_URL);
            }

            if (GUILayout.Button(documentationButtonContent, EditorCustomStyles.button, GUILayout.Height(22)))
            {
                Application.OpenURL(setupGuideInfo.documentationURL);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(description, descriptionStyle);

            defaultLength = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 115;

            EditorGUILayout.LabelField("Project version", projectVersion);
            EditorGUILayout.LabelField("Core version", coreVersion);

            EditorGUIUtility.labelWidth = defaultLength;
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);
            EditorGUILayout.LabelField(GUIContent.none, EditorCustomStyles.Skin.horizontalSlider);
            GUILayout.Space(-15);

            if (setupButtons.Length > 0)
            {
                EditorGUILayout.LabelField("LINKS", EditorCustomStyles.labelBold);

                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < setupButtons.Length; i++)
                {
                    setupButtons[i].Draw(setupButtonStyle);
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(20);
                EditorGUILayout.LabelField(GUIContent.none, EditorCustomStyles.Skin.horizontalSlider);
                GUILayout.Space(-10);
            }

            EditorGUILayout.LabelField("NEW TEMPLATE!", EditorCustomStyles.labelBold);

            EditorGUILayout.BeginHorizontal();

            if (finishedProject != null)
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.BeginVertical();
                if (GUILayout.Button(new GUIContent(finishedProject.gameTexture, finishedProject.name), gameButtonStyle, GUILayout.Height(246), GUILayout.Width(450)))
                {
                    Application.OpenURL(finishedProject.url);
                }
                EditorGUILayout.EndVertical();

                GUILayout.FlexibleSpace();
            }
            else
            {
                EditorGUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("Loading templates..", textGamesStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            targetSerializedObject.ApplyModifiedProperties();
        }
        
        #region Web
        private IEnumerator GetRequest(string uri)
        {
            UnityWebRequest www = UnityWebRequest.Get(uri);
            www.SendWebRequest();

            while (!www.isDone)
            {
                yield return null;
            }

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("[Setup Guide]: " + www.error);
            }
            else
            {
                // Or retrieve results as binary data
                byte[] results = www.downloadHandler.data;

                // For that you will need to add reference to System.Runtime.Serialization
                var jsonReader = JsonReaderWriterFactory.CreateJsonReader(results, new System.Xml.XmlDictionaryReaderQuotas());

                // For that you will need to add reference to System.Xml and System.Xml.Linq
                var root = XElement.Load(jsonReader);

                FinishedProject projectTemp = new FinishedProject(root.XPathSelectElement("name").Value, root.XPathSelectElement("url").Value, root.XPathSelectElement("image").Value);

                projectTemp.LoadTexture();

                finishedProject = projectTemp;
            }
        }
        #endregion

        private static void RepaintEditor()
        {
            if (instance != null)
                instance.Repaint();
        }

        [MenuItem("CONTEXT/SetupGuideInfo/Create Documentation")]
        static void CreateDocumentation(MenuCommand command)
        {
            SetupGuideInfo setupGuideInfo = (SetupGuideInfo)command.context;
            if (setupGuideInfo != null)
            {
                OnlineDocumentation onlineDocumentation = new OnlineDocumentation(setupGuideInfo.documentationURL);
                onlineDocumentation.SaveToFile();
            }
        }

        private class FinishedProject
        {
            public string name = "";
            public string url = "";

            public string imageUrl = "";
            public Texture2D gameTexture;

            public FinishedProject(string name, string url, string imageUrl)
            {
                this.name = name;
                this.url = url;
                this.imageUrl = imageUrl;
            }

            public void LoadTexture()
            {
                if(!string.IsNullOrEmpty(url))
                {
                    EditorCoroutines.Execute(GetTexture(imageUrl, (texture) =>
                    {
                        gameTexture = texture;

                        SetupGuideInfoEditor.RepaintEditor();
                        SetupGuideWindow.RepaintWindow();
                    }));
                }
            }

            private IEnumerator GetTexture(string uri, System.Action<Texture2D> onLoad)
            {
                UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri);
                www.SendWebRequest();

                while (!www.isDone)
                {
                    yield return null;
                }

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Texture2D myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    if (myTexture != null)
                    {
                        onLoad.Invoke(myTexture);
                    }
                }
            }
        }
    }
}

// -----------------
// Setup Guide v 1.0.2
// -----------------