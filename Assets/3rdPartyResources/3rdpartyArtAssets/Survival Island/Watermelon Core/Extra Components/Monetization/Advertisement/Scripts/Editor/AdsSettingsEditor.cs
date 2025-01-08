using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Watermelon
{
    [CustomEditor(typeof(AdsSettings))]
    public class AdsSettingsEditor : Editor
    {
        private SerializedProperty bannerTypeProperty;
        private SerializedProperty interstitialTypeProperty;
        private SerializedProperty rewardedVideoTypeProperty;

        private IEnumerable<SerializedProperty> gdprProperties;
        private IEnumerable<SerializedProperty> settingsProperties; 
        
        private readonly AdsContainer[] adsContainers = new AdsContainer[]
        {
            new DummyContainer("Dummy", "dummyContainer"),
            new AdMobContainer("AdMob", "adMobContainer"),
            new UnityAdsContainer("Unity Ads Legacy", "unityAdsContainer"),
            new IronSourceContainer("ironSource", "ironSourceContainer"),
        };

        private static GUIContent arrowDownContent;
        private static GUIContent arrowUpContent;
        private static GUIContent testIdContent;
        private static GUIStyle groupStyle;

        protected void OnEnable()
        {
            bannerTypeProperty = serializedObject.FindProperty("bannerType");
            interstitialTypeProperty = serializedObject.FindProperty("interstitialType");
            rewardedVideoTypeProperty = serializedObject.FindProperty("rewardedVideoType");

            gdprProperties = serializedObject.GetPropertiesByGroup("Privacy");
            settingsProperties = serializedObject.GetPropertiesByGroup("Settings");

            for (int i = 0; i < adsContainers.Length; i++)
            {
                adsContainers[i].Initialize(serializedObject);
            }

            serializedObject.ApplyModifiedProperties();

            PrepareStyles();
        }

        protected void PrepareStyles()
        {
            arrowDownContent = new GUIContent(EditorCustomStyles.GetIcon("icon_triangle_down"));
            arrowUpContent = new GUIContent(EditorCustomStyles.GetIcon("icon_triangle_up"));
            testIdContent = new GUIContent("", EditorCustomStyles.GetIcon("icon_warning"), "You are using test app id value.");
            groupStyle = new GUIStyle(EditorCustomStyles.Skin.label);
            groupStyle.fontStyle = FontStyle.Bold;
            groupStyle.alignment = TextAnchor.LowerLeft;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayoutCustom.BeginBoxGroup("Advertisement");

            EditorGUILayout.PropertyField(bannerTypeProperty);
            EditorGUILayout.PropertyField(interstitialTypeProperty);
            EditorGUILayout.PropertyField(rewardedVideoTypeProperty);

            EditorGUILayoutCustom.EndBoxGroup();

            EditorGUILayoutCustom.BeginBoxGroup("Settings");

            foreach (SerializedProperty prop in settingsProperties)
            {
                EditorGUILayout.PropertyField(prop);
            }

            EditorGUILayoutCustom.EndBoxGroup();

            EditorGUILayoutCustom.BeginBoxGroup("Privacy");

            foreach (SerializedProperty prop in gdprProperties)
            {
                EditorGUILayout.PropertyField(prop);
            }

            EditorGUILayoutCustom.EndBoxGroup();

            GUILayout.Space(8);

            for (int i = 0; i < adsContainers.Length; i++)
            {
                adsContainers[i].DrawContainer();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private abstract class AdsContainer
        {
            protected SerializedProperty containerProperty;
            private IEnumerable<SerializedProperty> containerProperties;

            protected string containerName;
            protected string propertyName;

            public AdsContainer(string containerName, string propertyName)
            {
                this.containerName = containerName;
                this.propertyName = propertyName;
            }

            public virtual void Initialize(SerializedObject serializedObject)
            {
                containerProperty = serializedObject.FindProperty(propertyName);
                containerProperties = containerProperty.GetChildren();
            }

            public virtual void DrawContainer()
            {
                containerProperty.isExpanded = EditorGUILayoutCustom.BeginExpandBoxGroup(containerName, containerProperty.isExpanded);

                if (containerProperty.isExpanded)
                {
                    foreach (SerializedProperty prop in containerProperties)
                    {
                        EditorGUILayout.PropertyField(prop);
                    }

                    SpecialButtons();
                }

                EditorGUILayoutCustom.EndBoxGroup();
            }

            protected abstract void SpecialButtons();
        }

        private class AdMobContainer : AdsContainer
        {
            //App section
            private const string SETTINGS_FILE_PATH = "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";
            private const string TEST_APP_ID = "ca-app-pub-3940256099942544~3347511713";
            private const string ANDROID_APP_ID_PROPERTY_PATH = "adMobAndroidAppId";
            private const string OUR_ANDROID_APP_ID_PROPERTY_PATH = "androidAppId";
            private const string IOS_APP_ID_PROPERTY_PATH = "adMobIOSAppId";
            private const string OUR_IOS_APP_ID_PROPERTY_PATH = "iosAppId";

            private SerializedProperty androidAppIdProperty;
            private SerializedProperty iOSAppIdProperty;
            private SerializedProperty ourAndroidAppIdProperty;
            private SerializedProperty ourIOSAppIdProperty;
            private UnityEngine.Object settingsFile;
            private SerializedObject serializedObject;
            private bool fileLoaded;

            //Add Units section 
            private const string BANNER_TYPE_PROPERTY_PATH = "bannerType";
            private const string BANNER_POSITION_PROPERTY_PATH = "bannerPosition";
            private const string ANDROID_BANNER_ID_PROPERTY_PATH = "androidBannerID";
            private const string ANDROID_INTERSTITIAL_ID_PROPERTY_PATH = "androidInterstitialID";
            private const string ANDROID_REWARDED_VIDEO_ID_PROPERTY_PATH = "androidRewardedVideoID";
            private const string IOS_BANNER_ID_PROPERTY_PATH = "iOSBannerID";
            private const string IOS_INTERSTITIAL_ID_PROPERTY_PATH = "iOSInterstitialID";
            private const string IOS_REWARDED_VIDEO_ID_PROPERTY_PATH = "iOSRewardedVideoID";
            private const string TEST_DEVICES_IDS_PROPERTY_PATH = "testDevicesIDs";


            private SerializedProperty bannerTypeProperty;
            private SerializedProperty bannerPositionProperty;
            private SerializedProperty androidBannerIdProperty;
            private SerializedProperty androidInterstitialIdProperty;
            private SerializedProperty androidRewardedVideoIdProperty;
            private SerializedProperty iOSBannerIdProperty;
            private SerializedProperty iOSInterstitialIdProperty;
            private SerializedProperty iOSRewardedVideoIdProperty;
            private SerializedProperty testDevicesIDsProperty;

            public AdMobContainer(string containerName, string propertyName) : base(containerName, propertyName)
            {
            }

            public override void Initialize(SerializedObject serializedObject)
            {
                base.Initialize(serializedObject);

                //for add units section
                bannerTypeProperty = containerProperty.FindPropertyRelative(BANNER_TYPE_PROPERTY_PATH);
                bannerPositionProperty = containerProperty.FindPropertyRelative(BANNER_POSITION_PROPERTY_PATH);
                androidBannerIdProperty = containerProperty.FindPropertyRelative(ANDROID_BANNER_ID_PROPERTY_PATH);
                androidInterstitialIdProperty = containerProperty.FindPropertyRelative(ANDROID_INTERSTITIAL_ID_PROPERTY_PATH);
                androidRewardedVideoIdProperty = containerProperty.FindPropertyRelative(ANDROID_REWARDED_VIDEO_ID_PROPERTY_PATH);
                iOSBannerIdProperty = containerProperty.FindPropertyRelative(IOS_BANNER_ID_PROPERTY_PATH);
                iOSInterstitialIdProperty = containerProperty.FindPropertyRelative(IOS_INTERSTITIAL_ID_PROPERTY_PATH);
                iOSRewardedVideoIdProperty = containerProperty.FindPropertyRelative(IOS_REWARDED_VIDEO_ID_PROPERTY_PATH);
                testDevicesIDsProperty = containerProperty.FindPropertyRelative(TEST_DEVICES_IDS_PROPERTY_PATH);

                // for app section
                fileLoaded = false;
                ourAndroidAppIdProperty = containerProperty.FindPropertyRelative(OUR_ANDROID_APP_ID_PROPERTY_PATH);
                ourIOSAppIdProperty = containerProperty.FindPropertyRelative(OUR_IOS_APP_ID_PROPERTY_PATH);

                LoadFile();
            }

            private void LoadFile()
            {
                settingsFile = AssetDatabase.LoadMainAssetAtPath(SETTINGS_FILE_PATH);

                if (settingsFile != null)
                {
                    serializedObject = new SerializedObject(settingsFile);
                    androidAppIdProperty = serializedObject.FindProperty(ANDROID_APP_ID_PROPERTY_PATH);
                    iOSAppIdProperty = serializedObject.FindProperty(IOS_APP_ID_PROPERTY_PATH);
                    fileLoaded = true;
                }
                else
                {
#if MODULE_ADMOB
                    GoogleMobileAds.Editor.GoogleMobileAdsSettingsEditor.OpenInspector(); // Creates admob settings file

                    LoadFile();

                    if (fileLoaded)
                    {

                        if ((androidAppIdProperty.stringValue.Length == 0) && (ourAndroidAppIdProperty.stringValue.Length != 0))
                        {
                            androidAppIdProperty.stringValue = ourAndroidAppIdProperty.stringValue;
                        }
                        else if ((androidAppIdProperty.stringValue.Length != 0) && (ourAndroidAppIdProperty.stringValue.Length == 0))
                        {
                            ourAndroidAppIdProperty.stringValue = androidAppIdProperty.stringValue;
                        }

                        if ((iOSAppIdProperty.stringValue.Length == 0) && (ourIOSAppIdProperty.stringValue.Length != 0))
                        {
                            iOSAppIdProperty.stringValue = ourIOSAppIdProperty.stringValue;
                        }
                        else if ((iOSAppIdProperty.stringValue.Length != 0) && (ourIOSAppIdProperty.stringValue.Length == 0))
                        {
                            ourIOSAppIdProperty.stringValue = iOSAppIdProperty.stringValue;
                        }


                        serializedObject.ApplyModifiedProperties();
                        ourAndroidAppIdProperty.serializedObject.ApplyModifiedProperties();
                    }
#endif
                }
            }

            public override void DrawContainer()
            {
                containerProperty.isExpanded = EditorGUILayoutCustom.BeginExpandBoxGroup(containerName, containerProperty.isExpanded);

                if (containerProperty.isExpanded)
                {
                    DrawAppSection();
                    DrawAddUnitsSection();

                    DrawUsefulSection();

                    containerProperty.serializedObject.ApplyModifiedProperties();

                    if (fileLoaded)
                    {
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                EditorGUILayoutCustom.EndBoxGroup();
            }

            private void DrawAppSection()
            {
                EditorGUILayout.LabelField("Application ID", groupStyle);

                DrawIdProperty(ourAndroidAppIdProperty, TEST_APP_ID);
                DrawIdProperty(ourIOSAppIdProperty, TEST_APP_ID);

                if (fileLoaded)
                {
                    androidAppIdProperty.stringValue = ourAndroidAppIdProperty.stringValue;
                    iOSAppIdProperty.stringValue = ourIOSAppIdProperty.stringValue;
                }
            }

            private void DrawIdProperty(SerializedProperty property, string testValue)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(property);

                if (property.stringValue.Equals(testValue))
                {
                    EditorGUILayout.LabelField(testIdContent, GUILayout.MaxWidth(24));
                }

                EditorGUILayout.EndHorizontal();
            }

            private void DrawAddUnitsSection()
            {
                EditorGUILayout.LabelField("Banner ID", groupStyle);
                DrawIdProperty(androidBannerIdProperty, Watermelon.AdMobContainer.ANDROID_BANNER_TEST_ID);
                DrawIdProperty(iOSBannerIdProperty, Watermelon.AdMobContainer.IOS_BANNER_TEST_ID);

                EditorGUILayout.PropertyField(bannerTypeProperty);
                EditorGUILayout.PropertyField(bannerPositionProperty);

                EditorGUILayout.LabelField("Interstitial ID", groupStyle);
                DrawIdProperty(androidInterstitialIdProperty, Watermelon.AdMobContainer.ANDROID_INTERSTITIAL_TEST_ID);
                DrawIdProperty(iOSInterstitialIdProperty, Watermelon.AdMobContainer.IOS_INTERSTITIAL_TEST_ID);

                EditorGUILayout.LabelField("Rewarded Video ID", groupStyle);
                DrawIdProperty(androidRewardedVideoIdProperty, Watermelon.AdMobContainer.ANDROID_REWARDED_VIDEO_TEST_ID);
                DrawIdProperty(iOSRewardedVideoIdProperty, Watermelon.AdMobContainer.IOS_REWARDED_VIDEO_TEST_ID);

                EditorGUILayout.LabelField("Debug", EditorCustomStyles.labelMediumBold);

                EditorGUILayout.PropertyField(testDevicesIDsProperty);

                if (GUILayout.Button("Set test app id", EditorCustomStyles.button))
                {
                    ourAndroidAppIdProperty.stringValue = TEST_APP_ID;
                    ourIOSAppIdProperty.stringValue = TEST_APP_ID;
                    containerProperty.serializedObject.ApplyModifiedProperties();

                    if (fileLoaded)
                    {
                        androidAppIdProperty.stringValue = ourAndroidAppIdProperty.stringValue;
                        iOSAppIdProperty.stringValue = ourIOSAppIdProperty.stringValue;
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                if (GUILayout.Button("Set test ids", EditorCustomStyles.button))
                {
                    androidBannerIdProperty.stringValue = Watermelon.AdMobContainer.ANDROID_BANNER_TEST_ID;
                    iOSBannerIdProperty.stringValue = Watermelon.AdMobContainer.IOS_BANNER_TEST_ID;
                    androidInterstitialIdProperty.stringValue = Watermelon.AdMobContainer.ANDROID_INTERSTITIAL_TEST_ID;
                    iOSInterstitialIdProperty.stringValue = Watermelon.AdMobContainer.IOS_INTERSTITIAL_TEST_ID;
                    androidRewardedVideoIdProperty.stringValue = Watermelon.AdMobContainer.ANDROID_REWARDED_VIDEO_TEST_ID;
                    iOSRewardedVideoIdProperty.stringValue = Watermelon.AdMobContainer.IOS_REWARDED_VIDEO_TEST_ID;
                }
            }

            private void DrawUsefulSection()
            {
                EditorGUILayout.LabelField("Useful", EditorCustomStyles.labelMediumBold);

                if (GUILayout.Button("Download AdMob plugin", EditorCustomStyles.button))
                {
                    Application.OpenURL(@"https://github.com/googleads/googleads-mobile-unity/releases");
                }

                if (GUILayout.Button("AdMob Dashboard", EditorCustomStyles.button))
                {
                    Application.OpenURL(@"https://apps.admob.com/v2/home");
                }

                if (GUILayout.Button("AdMob Quick Start Guide", EditorCustomStyles.button))
                {
                    Application.OpenURL(@"https://developers.google.com/admob/unity/start");
                }

                GUILayout.Space(8);

                EditorGUILayout.HelpBox("Tested with AdMob Plugin v9.1.0", MessageType.Info);
            }

            protected override void SpecialButtons()
            {
            }
        }

        private class UnityAdsContainer : AdsContainer
        {
            public UnityAdsContainer(string containerName, string propertyName) : base(containerName, propertyName)
            {
            }

            protected override void SpecialButtons()
            {
                GUILayout.Space(8);

                if (GUILayout.Button("Unity Ads Dashboard", EditorCustomStyles.button))
                {
                    Application.OpenURL(@"https://operate.dashboard.unity3d.com");
                }

                if (GUILayout.Button("Unity Ads Quick Start Guide", EditorCustomStyles.button))
                {
                    Application.OpenURL(@"https://unityads.unity3d.com/help/monetization/getting-started");
                }

                GUILayout.Space(8);

                EditorGUILayout.HelpBox("Tested with Advertisement v4.4.2", MessageType.Info);
            }
        }

        private class IronSourceContainer : AdsContainer
        {
            public IronSourceContainer(string containerName, string propertyName) : base(containerName, propertyName)
            {
            }

            protected override void SpecialButtons()
            {
                GUILayout.Space(8);

                if (GUILayout.Button("Getting Started Guide", EditorCustomStyles.button))
                {
                    Application.OpenURL(@"https://developers.is.com/ironsource-mobile/unity/levelplay-starter-kit/");
                }

                if (GUILayout.Button("Integration Testing", EditorCustomStyles.button))
                {
                    Application.OpenURL(@"https://developers.is.com/ironsource-mobile/unity/unity-levelplay-test-suite/#step-1");
                }
                
                GUILayout.Space(8);

                EditorGUILayout.HelpBox("Tested with ironSource v8.0.0", MessageType.Info);
            }
        }

        private class DummyContainer : AdsContainer
        {
            public DummyContainer(string containerName, string propertyName) : base(containerName, propertyName)
            {
            }

            protected override void SpecialButtons()
            {
            }
        }
    }
}