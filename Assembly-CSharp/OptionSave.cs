using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OptionSave : IThoth
{
    private int lastVersion;

    public bool fullScreen;

    public Resolution resolution;

    public bool vsync;

    public int textureQuality;

    public int shadowsQuality;

    public int shadowCascades;

    public bool graphicsDof;

    public bool graphicsSsao;

    public int graphicsSmaa;

    public float graphicsBrightness;

    public bool graphicsBloom;

    public float graphicsGuiScale;

    public float masterVolume;

    public float fxVolume;

    public float musicVolume;

    public float voiceVolume;

    public float ambientVolume;

    public int language;

    public bool gamepadEnabled;

    public bool cameraXInverted;

    public bool cameraYInverted;

    public bool leftHandedMouse;

    public bool leftHandedController;

    public float mouseSensitivity;

    public float joystickSensitivity;

    public bool tacticalViewHelpersEnabled;

    public bool wagonBeaconsEnabled;

    public bool autoExitTacticalEnabled;

    public bool displayFullUI;

    public bool fastForwarded;

    public bool skipTuto;

    public List<string> keyboardMappingData;

    public List<string> joystickMappingData;

    public List<string> mouseMappingData;

    public OptionSave()
    {
        bool useLowSettings = Pandora.useLowSettings;
        fullScreen = true;
        resolution = Screen.resolutions[Screen.resolutions.Length - 1];
        vsync = (QualitySettings.vSyncCount != 0 && !useLowSettings);
        textureQuality = ((!useLowSettings) ? 3 : 0);
        shadowsQuality = ((!useLowSettings) ? 4 : 0);
        shadowCascades = ((!useLowSettings) ? 2 : 0);
        graphicsDof = !useLowSettings;
        graphicsSsao = !useLowSettings;
        graphicsSmaa = ((!useLowSettings) ? 4 : 0);
        graphicsBrightness = 0.5f;
        graphicsBloom = !useLowSettings;
        graphicsGuiScale = 1f;
        masterVolume = 1f;
        fxVolume = 1f;
        musicVolume = 0.45f;
        voiceVolume = 1f;
        ambientVolume = 0.75f;
        language = 0;
        gamepadEnabled = true;
        cameraXInverted = false;
        cameraYInverted = false;
        leftHandedMouse = false;
        leftHandedController = false;
        mouseSensitivity = 0.25f;
        joystickSensitivity = 0.25f;
        tacticalViewHelpersEnabled = true;
        wagonBeaconsEnabled = true;
        autoExitTacticalEnabled = true;
        displayFullUI = true;
        fastForwarded = false;
        skipTuto = false;
    }

    int IThoth.GetVersion()
    {
        return 56;
    }

    void IThoth.Write(BinaryWriter writer)
    {
        Thoth.Write(writer, ((IThoth)this).GetVersion());
        int cRC = GetCRC(read: false);
        Thoth.Write(writer, cRC);
        Thoth.Write(writer, fullScreen);
        Thoth.Write(writer, resolution.height);
        Thoth.Write(writer, resolution.width);
        Thoth.Write(writer, masterVolume);
        Thoth.Write(writer, fxVolume);
        Thoth.Write(writer, musicVolume);
        Thoth.Write(writer, voiceVolume);
        Thoth.Write(writer, ambientVolume);
        Thoth.Write(writer, gamepadEnabled);
        Thoth.Write(writer, cameraXInverted);
        Thoth.Write(writer, cameraYInverted);
        Thoth.Write(writer, leftHandedMouse);
        Thoth.Write(writer, leftHandedController);
        Thoth.Write(writer, tacticalViewHelpersEnabled);
        Thoth.Write(writer, wagonBeaconsEnabled);
        Thoth.Write(writer, autoExitTacticalEnabled);
        SaveInputMaps(writer, keyboardMappingData);
        SaveInputMaps(writer, joystickMappingData);
        SaveInputMaps(writer, mouseMappingData);
        Thoth.Write(writer, mouseSensitivity);
        Thoth.Write(writer, joystickSensitivity);
        Thoth.Write(writer, language);
        Thoth.Write(writer, vsync);
        Thoth.Write(writer, textureQuality);
        Thoth.Write(writer, shadowsQuality);
        Thoth.Write(writer, shadowCascades);
        Thoth.Write(writer, graphicsDof);
        Thoth.Write(writer, graphicsSsao);
        Thoth.Write(writer, graphicsSmaa);
        Thoth.Write(writer, graphicsBrightness);
        Thoth.Write(writer, graphicsBloom);
        Thoth.Write(writer, graphicsGuiScale);
        Thoth.Write(writer, displayFullUI);
        Thoth.Write(writer, fastForwarded);
        Thoth.Write(writer, skipTuto);
    }

    void IThoth.Read(BinaryReader reader)
    {
        int i = 0;
        Thoth.Read(reader, out int i2);
        lastVersion = i2;
        if (i2 > 50)
        {
            Thoth.Read(reader, out i);
        }
        Thoth.Read(reader, out fullScreen);
        Thoth.Read(reader, out int i3);
        resolution.height = i3;
        Thoth.Read(reader, out i3);
        resolution.width = i3;
        if (i2 > 1)
        {
            Thoth.Read(reader, out masterVolume);
            Thoth.Read(reader, out fxVolume);
            Thoth.Read(reader, out musicVolume);
            Thoth.Read(reader, out voiceVolume);
            if (i2 > 44)
            {
                Thoth.Read(reader, out ambientVolume);
            }
            if (i2 > 2)
            {
                Thoth.Read(reader, out gamepadEnabled);
                Thoth.Read(reader, out cameraXInverted);
                Thoth.Read(reader, out cameraYInverted);
                if (i2 > 5)
                {
                    Thoth.Read(reader, out leftHandedMouse);
                    Thoth.Read(reader, out leftHandedController);
                    if (i2 > 35)
                    {
                        Thoth.Read(reader, out tacticalViewHelpersEnabled);
                    }
                    if (i2 > 38)
                    {
                        Thoth.Read(reader, out wagonBeaconsEnabled);
                    }
                    if (i2 > 40)
                    {
                        Thoth.Read(reader, out autoExitTacticalEnabled);
                    }
                    if (i2 > 6)
                    {
                        keyboardMappingData = LoadInputMaps(reader);
                        joystickMappingData = LoadInputMaps(reader);
                        mouseMappingData = LoadInputMaps(reader);
                        if (i2 > 7)
                        {
                            Thoth.Read(reader, out mouseSensitivity);
                            Thoth.Read(reader, out joystickSensitivity);
                            if (i2 > 18)
                            {
                                Thoth.Read(reader, out language);
                                if (i2 > 34)
                                {
                                    Thoth.Read(reader, out vsync);
                                    Thoth.Read(reader, out textureQuality);
                                    Thoth.Read(reader, out shadowsQuality);
                                    if (i2 > 42)
                                    {
                                        Thoth.Read(reader, out shadowCascades);
                                    }
                                    Thoth.Read(reader, out graphicsDof);
                                    if (i2 < 43)
                                    {
                                        Thoth.Read(reader, out int i4);
                                        graphicsSsao = (i4 > 0);
                                    }
                                    else
                                    {
                                        Thoth.Read(reader, out graphicsSsao);
                                    }
                                    Thoth.Read(reader, out graphicsSmaa);
                                    if (i2 < 43)
                                    {
                                        Thoth.Read(reader, out int i5);
                                        graphicsBrightness = (float)i5 / 100f;
                                        Thoth.Read(reader, out int i6);
                                        Thoth.Read(reader, out i6);
                                    }
                                    else
                                    {
                                        Thoth.Read(reader, out graphicsBrightness);
                                    }
                                    if (i2 > 42)
                                    {
                                        Thoth.Read(reader, out graphicsBloom);
                                    }
                                    if (i2 > 55)
                                    {
                                        Thoth.Read(reader, out graphicsGuiScale);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        if (i2 > 51)
        {
            Thoth.Read(reader, out displayFullUI);
        }
        if (i2 > 53)
        {
            Thoth.Read(reader, out fastForwarded);
        }
        if (i2 > 54)
        {
            Thoth.Read(reader, out skipTuto);
        }
    }

    public int GetCRC(bool read)
    {
        return CalculateCRC(read);
    }

    private int CalculateCRC(bool read)
    {
        int num = (!read) ? ((IThoth)this).GetVersion() : lastVersion;
        int num2 = 0;
        num2 += (fullScreen ? 1 : 0);
        num2 += resolution.height;
        num2 += resolution.width;
        num2 += (vsync ? 1 : 0);
        num2 += textureQuality;
        num2 += shadowsQuality;
        num2 += shadowCascades;
        num2 += (graphicsDof ? 1 : 0);
        num2 += (graphicsSsao ? 1 : 0);
        num2 += graphicsSmaa;
        num2 += (int)(graphicsBrightness * 10f);
        num2 += (graphicsBloom ? 1 : 0);
        num2 += (int)(graphicsGuiScale * 10f);
        num2 += (int)(masterVolume * 10f);
        num2 += (int)(fxVolume * 10f);
        num2 += (int)(musicVolume * 10f);
        num2 += (int)(voiceVolume * 10f);
        num2 += (int)(ambientVolume * 10f);
        num2 += language;
        num2 += (gamepadEnabled ? 1 : 0);
        num2 += (cameraXInverted ? 1 : 0);
        num2 += (cameraYInverted ? 1 : 0);
        num2 += (leftHandedMouse ? 1 : 0);
        num2 += (leftHandedController ? 1 : 0);
        num2 += (int)(mouseSensitivity * 10f);
        num2 += (int)(joystickSensitivity * 10f);
        num2 += (tacticalViewHelpersEnabled ? 1 : 0);
        num2 += (wagonBeaconsEnabled ? 1 : 0);
        num2 += (autoExitTacticalEnabled ? 1 : 0);
        if (num > 51)
        {
            num2 += (displayFullUI ? 1 : 0);
        }
        if (num > 53)
        {
            num2 += (fastForwarded ? 1 : 0);
        }
        if (num > 54)
        {
            num2 += (skipTuto ? 1 : 0);
        }
        return num2;
    }

    private List<string> LoadInputMaps(BinaryReader reader)
    {
        Thoth.Read(reader, out int i);
        List<string> list = new List<string>();
        for (int j = 0; j < i; j++)
        {
            Thoth.Read(reader, out string s);
            list.Add(s);
        }
        return list;
    }

    private void SaveInputMaps(BinaryWriter writer, List<string> mappingData)
    {
        if (mappingData != null)
        {
            Thoth.Write(writer, mappingData.Count);
            for (int i = 0; i < mappingData.Count; i++)
            {
                Thoth.Write(writer, mappingData[i]);
            }
        }
        else
        {
            Thoth.Write(writer, 0);
        }
    }
}
