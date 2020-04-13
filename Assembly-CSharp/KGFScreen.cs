using KGFUtils.Settings;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class KGFScreen : MonoBehaviour
{
    public enum eResolutionMode
    {
        eNative,
        eAutoAdjust
    }

    [Serializable]
    public class KGFDataScreen
    {
        public eResolutionMode itsResolutionMode3D = eResolutionMode.eAutoAdjust;

        public eResolutionMode itsResolutionMode2D = eResolutionMode.eAutoAdjust;

        [HideInInspector]
        public Resolution itsResolution3D;

        [HideInInspector]
        public Resolution itsResolution2D;

        [HideInInspector]
        public Resolution itsResolutionDisplay;

        [HideInInspector]
        public float itsAspect3D = 1f;

        [HideInInspector]
        public float itsAspect2D = 1f;

        [HideInInspector]
        public float itsAspectDisplay = 1f;

        [HideInInspector]
        public float itsScaleFactor3D = 1f;

        [HideInInspector]
        public float itsScaleFactor2D = 1f;

        public int itsMinWidth = 480;

        public int itsMinHeight = 320;
    }

    private const string itsSettingsSection = "screen";

    private const string itsSettingsNameWidth = "resolution.width";

    private const string itsSettingsNameHeight = "resolution.height";

    private const string itsSettingsNameRefreshRate = "refreshrate";

    private const string itsSettingsNameIsFulscreen = "fullscreen";

    private static KGFScreen itsInstance;

    private static bool itsAlreadyChecked;

    private INIFile itsIniFile;

    private RenderTexture itsRenderTexture;

    private Camera itsCamera;

    public KGFDataScreen itsDataModuleScreen = new KGFDataScreen();

    protected void Awake()
    {
        if (itsInstance == null)
        {
            itsInstance = this;
            itsInstance.Init();
        }
        else if (itsInstance != this)
        {
            Debug.Log("there is more than one KFGDebug instance in this scene. please ensure there is always exactly one instance in this scene");
            UnityEngine.Object.Destroy(base.gameObject);
        }
    }

    private INIFile GetIniFile()
    {
        //IL_0030: Unknown result type (might be due to invalid IL or missing references)
        //IL_003a: Expected O, but got Unknown
        if (itsIniFile == null)
        {
            string path = KGFUtility.ConvertPathToPlatformSpecific(Application.dataPath);
            path = Path.Combine(path, "..");
            path = Path.Combine(path, "settings.ini");
            itsIniFile = (INIFile)(object)new INIFile(path);
        }
        return itsIniFile;
    }

    public static KGFScreen GetInstance()
    {
        return itsInstance;
    }

    private static void SetResolution3D(int theWidth, int theHeight)
    {
        SetResolution3D(theWidth, theHeight, 60);
    }

    public static Resolution GetResolution3D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return default(Resolution);
        }
        return itsInstance.itsDataModuleScreen.itsResolution3D;
    }

    public static Resolution GetResolution2D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return default(Resolution);
        }
        return itsInstance.itsDataModuleScreen.itsResolution2D;
    }

    public static Resolution GetResolutionDisplay()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return default(Resolution);
        }
        return itsInstance.itsDataModuleScreen.itsResolutionDisplay;
    }

    public static eResolutionMode GetResolutionMode3D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return eResolutionMode.eNative;
        }
        return itsInstance.itsDataModuleScreen.itsResolutionMode3D;
    }

    public static eResolutionMode GetResolutionMode2D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return eResolutionMode.eNative;
        }
        return itsInstance.itsDataModuleScreen.itsResolutionMode2D;
    }

    public static float GetAspect3D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return 1f;
        }
        return itsInstance.itsDataModuleScreen.itsAspect3D;
    }

    public static float GetAspect2D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return 1f;
        }
        return itsInstance.itsDataModuleScreen.itsAspect2D;
    }

    public static float GetScaleFactor3D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return 1f;
        }
        return itsInstance.itsDataModuleScreen.itsScaleFactor3D;
    }

    public static float GetScaleFactor2D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return 1f;
        }
        return itsInstance.itsDataModuleScreen.itsScaleFactor2D;
    }

    public static Vector3 GetConvertedEventCurrentMousePosition(Vector2 theEventCurrentMousePosition)
    {
        Vector3 vector = Input.mousePosition * GetScaleFactor3D();
        Vector3 mousePosition = Input.mousePosition;
        float num = vector.x - mousePosition.x;
        float num2 = vector.y - mousePosition.y;
        num /= GetScaleFactor3D();
        num2 /= GetScaleFactor3D();
        Vector2 v = new Vector2(theEventCurrentMousePosition.x + num, theEventCurrentMousePosition.y - num2);
        return v;
    }

    public static Vector3 GetMousePositionDisplay()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return Vector3.zero;
        }
        Vector3 mousePosition = Input.mousePosition;
        float x = mousePosition.x * GetScaleFactor3D();
        float num = Screen.height;
        Vector3 mousePosition2 = Input.mousePosition;
        float y = num - mousePosition2.y * GetScaleFactor3D();
        Vector3 mousePosition3 = Input.mousePosition;
        Vector3 result = new Vector3(x, y, mousePosition3.z);
        return result;
    }

    public static Vector3 GetMousePosition2D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return Vector3.zero;
        }
        if (GetResolutionMode3D() == GetResolutionMode2D())
        {
            return Input.mousePosition;
        }
        if (GetResolutionMode2D() == eResolutionMode.eNative && GetResolutionMode3D() == eResolutionMode.eAutoAdjust)
        {
            return Input.mousePosition * GetScaleFactor3D();
        }
        if (GetResolutionMode2D() == eResolutionMode.eAutoAdjust && GetResolutionMode3D() == eResolutionMode.eNative)
        {
            return Input.mousePosition / GetScaleFactor2D();
        }
        return Input.mousePosition;
    }

    public static Vector3 GetMousePositio3D()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return Vector3.zero;
        }
        return Input.mousePosition;
    }

    public static Vector2 DisplayToScreen(Vector2 theDisplayPosition)
    {
        return theDisplayPosition / GetScaleFactor3D();
    }

    public static Vector2 DisplayToScreen2D(Vector2 theDisplayPosition)
    {
        return theDisplayPosition / GetScaleFactor2D();
    }

    public static Vector2 DisplayToScreenNormalized(Vector2 theDisplayPosition)
    {
        Vector2 result = new Vector2(0f, 0f);
        Vector2 vector = DisplayToScreen(theDisplayPosition);
        result.x = vector.x / (float)GetResolution3D().width;
        result.y = vector.y / (float)GetResolution3D().height;
        return result;
    }

    public static Rect DisplayToScreen(Rect theDisplayRect)
    {
        Rect result = new Rect(0f, 0f, 1f, 1f);
        result.x = theDisplayRect.x / GetScaleFactor3D();
        result.y = theDisplayRect.y / GetScaleFactor3D();
        result.width = theDisplayRect.width / GetScaleFactor3D();
        result.height = theDisplayRect.height / GetScaleFactor3D();
        return result;
    }

    public static Rect DisplayToScreenNormalized(Rect theDisplayRect)
    {
        Rect result = new Rect(0f, 0f, 1f, 1f);
        Rect rect = DisplayToScreen(theDisplayRect);
        result.x = rect.x / (float)GetResolution3D().width;
        result.y = rect.y / (float)GetResolution3D().height;
        result.width = rect.width / (float)GetResolution3D().width;
        result.height = rect.height / (float)GetResolution3D().height;
        return result;
    }

    public static Rect NormalizedTo2DScreen(Rect theDisplayRect)
    {
        Rect result = new Rect(0f, 0f, 1f, 1f);
        result.x = (float)GetResolution2D().width * theDisplayRect.x;
        result.y = (float)GetResolution2D().height * theDisplayRect.y;
        result.width = (float)GetResolution2D().width * theDisplayRect.width;
        result.height = (float)GetResolution2D().height * theDisplayRect.height;
        return result;
    }

    public static RenderTexture GetRenderTexture()
    {
        CheckInstance();
        if (itsInstance == null)
        {
            return null;
        }
        itsInstance.CreateCamera();
        return itsInstance.itsRenderTexture;
    }

    public static void BlitToScreen()
    {
        CheckInstance();
        if (!(itsInstance == null) && itsInstance.itsRenderTexture != null)
        {
            Graphics.Blit((Texture)itsInstance.itsRenderTexture, (RenderTexture)null);
        }
    }

    private void Update()
    {
    }

    private void CorrectMousePosition()
    {
        Rect windowRect = KGFUtility.GetWindowRect();
        if (windowRect.width == 0f || windowRect.height == 0f)
        {
            return;
        }
        Rect rect = new Rect(windowRect.x, windowRect.y + windowRect.height - (float)GetResolution2D().height, GetResolution2D().width, GetResolution2D().height);
        if (rect.width != 0f && rect.height != 0f)
        {
            bool flag = false;
            KGFUtility.GetCursorPos(out KGFUtility.Point _point);
            if ((float)_point.X > rect.xMax)
            {
                _point.X = (int)rect.xMax;
                flag = true;
            }
            if ((float)_point.Y < rect.yMin)
            {
                _point.Y = (int)rect.yMin;
                flag = true;
            }
            if ((float)_point.Y > rect.yMax)
            {
                _point.Y = (int)rect.yMax;
                flag = true;
            }
            if (flag)
            {
                KGFUtility.SetCursorPos(_point.X, _point.Y);
            }
        }
    }

    private void CreateCamera()
    {
        if (!(itsCamera != null))
        {
            base.gameObject.AddComponent<Camera>();
            itsCamera = base.gameObject.GetComponent<Camera>();
            itsCamera.clearFlags = CameraClearFlags.Color;
            itsCamera.backgroundColor = Color.black;
            itsCamera.cullingMask = 0;
            itsCamera.orthographic = true;
            itsCamera.orthographicSize = 1f;
            itsCamera.depth = 100f;
            itsCamera.farClipPlane = 1f;
            itsCamera.nearClipPlane = 0.5f;
        }
    }

    private static void SetResolution3D(int theWidth, int theHeight, int theRefreshRate)
    {
        CheckInstance();
        if (!(itsInstance == null))
        {
            itsInstance.itsDataModuleScreen.itsResolution3D.width = theWidth;
            itsInstance.itsDataModuleScreen.itsResolution3D.height = theHeight;
            itsInstance.itsDataModuleScreen.itsResolution3D.refreshRate = theRefreshRate;
            itsInstance.itsDataModuleScreen.itsAspect3D = ReadAspect(theWidth, theHeight);
            itsInstance.itsDataModuleScreen.itsScaleFactor3D = (float)GetResolutionDisplay().width / (float)theWidth;
            Debug.Log("KGFScreen: set resolution 3D to: " + theWidth + "/" + theHeight + "/" + theRefreshRate);
            itsInstance.CreateRenderTexture();
        }
    }

    private static void SetResolution2D(int theWidth, int theHeight)
    {
        CheckInstance();
        if (!(itsInstance == null))
        {
            itsInstance.itsDataModuleScreen.itsResolution2D.width = theWidth;
            itsInstance.itsDataModuleScreen.itsResolution2D.height = theHeight;
            itsInstance.itsDataModuleScreen.itsResolution2D.refreshRate = 0;
            itsInstance.itsDataModuleScreen.itsAspect2D = ReadAspect(theWidth, theHeight);
            itsInstance.itsDataModuleScreen.itsScaleFactor2D = ((float)GetResolutionDisplay().width + 1f) / (float)theWidth;
            Debug.Log("KGFScreen: set resolution 2D to: " + theWidth + "/" + theHeight);
        }
    }

    private static void UpdateMouseRect()
    {
        Rect windowRect = KGFUtility.GetWindowRect();
        KGFUtility.SetMouseRect(new Rect(windowRect.x, windowRect.y + windowRect.height - (float)GetResolution2D().height, GetResolution2D().width, GetResolution2D().height));
        MonoBehaviour.print("new rect:" + windowRect);
    }

    private static void CheckInstance()
    {
        if (itsInstance == null)
        {
            UnityEngine.Object @object = UnityEngine.Object.FindObjectOfType(typeof(KGFScreen));
            if (@object != null)
            {
                itsInstance = (@object as KGFScreen);
                itsInstance.Init();
            }
            else if (!itsAlreadyChecked)
            {
                Debug.LogError("KGFScreen is not running. Make sure that there is an instance of the KGFScreen prefab in the current scene.");
                itsAlreadyChecked = true;
            }
        }
    }

    private void Init()
    {
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, fullscreen: false);
        StartCoroutine(SetResolutionDelayed());
    }

    private IEnumerator SetResolutionDelayed()
    {
        yield return new WaitForSeconds(1f);
        ReadResolutionDisplay();
        Debug.Log("display resolution set to: " + GetResolutionDisplay().width + "/" + GetResolutionDisplay().height);
        float anAspect = ReadAspect(GetResolutionDisplay().width, GetResolutionDisplay().height);
        int aHeight = itsDataModuleScreen.itsMinHeight;
        int aWidth = (int)((float)aHeight * anAspect);
        if (aWidth < itsDataModuleScreen.itsMinWidth)
        {
            aWidth = itsDataModuleScreen.itsMinWidth;
            aHeight = (int)((float)itsDataModuleScreen.itsMinWidth / anAspect);
        }
        switch (GetResolutionMode3D())
        {
            case eResolutionMode.eNative:
                SetResolution3D(GetResolutionDisplay().width, GetResolutionDisplay().height);
                break;
            case eResolutionMode.eAutoAdjust:
                SetResolution3D(aWidth, aHeight);
                break;
        }
        if (itsDataModuleScreen.itsResolutionMode2D == eResolutionMode.eNative)
        {
            SetResolution2D(GetResolutionDisplay().width, GetResolutionDisplay().height);
        }
        else if (itsDataModuleScreen.itsResolutionMode2D == eResolutionMode.eAutoAdjust)
        {
            SetResolution2D(aWidth, aHeight);
        }
    }

    private void ReadResolutionDisplay()
    {
        itsInstance.itsDataModuleScreen.itsResolutionDisplay = default(Resolution);
        itsInstance.itsDataModuleScreen.itsResolutionDisplay.width = Screen.width;
        itsInstance.itsDataModuleScreen.itsResolutionDisplay.height = Screen.height;
        itsInstance.itsDataModuleScreen.itsResolutionDisplay.refreshRate = 60;
        itsInstance.itsDataModuleScreen.itsAspectDisplay = ReadAspect(Screen.width, Screen.height);
    }

    private static float ReadAspect(int theWidth, int theHeight)
    {
        return (float)theWidth / (float)theHeight;
    }

    private void CreateRenderTexture()
    {
        if (itsRenderTexture == null)
        {
            itsRenderTexture = new RenderTexture(GetResolution3D().width, GetResolution3D().height, 16, RenderTextureFormat.ARGB32);
        }
        else if (itsRenderTexture.width != GetResolution3D().width)
        {
            itsRenderTexture.Release();
            itsRenderTexture = new RenderTexture(GetResolution3D().width, GetResolution3D().height, 16, RenderTextureFormat.ARGB32);
        }
        itsRenderTexture.isPowerOfTwo = true;
        itsRenderTexture.name = "KGFScreenRenderTexture";
        itsRenderTexture.Create();
    }

    private void OnPostRender()
    {
        BlitToScreen();
    }
}
