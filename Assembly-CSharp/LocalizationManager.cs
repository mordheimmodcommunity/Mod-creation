using Pathfinding.Serialization.JsonFx;
using Rewired;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class LocalizationManager : PandoraSingleton<LocalizationManager>
{
    public SupportedLanguage defaultLanguage;

    public TextAsset defaultFile;

    private Dictionary<uint, string> language;

    private string[] emptyArray = new string[0];

    public SupportedLanguage CurrentLanguageId
    {
        get;
        private set;
    }

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        if ((bool)defaultFile)
        {
            CurrentLanguageId = defaultLanguage;
            ParseFile(defaultFile);
        }
    }

    public void SetLanguage(SupportedLanguage languageId, bool force = false)
    {
        if (CurrentLanguageId != languageId || force)
        {
            CurrentLanguageId = languageId;
            string path = string.Empty;
            switch (languageId)
            {
                case SupportedLanguage.enUS:
                    path = "loc/loc_en";
                    break;
                case SupportedLanguage.frFR:
                    path = "loc/loc_fr";
                    break;
                case SupportedLanguage.deDE:
                    path = "loc/loc_de";
                    break;
                case SupportedLanguage.esES:
                    path = "loc/loc_es";
                    break;
                case SupportedLanguage.itIT:
                    path = "loc/loc_it";
                    break;
                case SupportedLanguage.plPL:
                    path = "loc/loc_pl";
                    break;
                case SupportedLanguage.ruRU:
                    path = "loc/loc_ru";
                    break;
            }
            ParseFile(Resources.Load(path) as TextAsset);
        }
    }

    public bool HasStringId(string key)
    {
        uint key2 = FNV1a.ComputeHash(key);
        return language.ContainsKey(key2);
    }

    public string BuildStringAndLocalize(string str1, string str2, string str3 = null, string str4 = null)
    {
        StringBuilder stringBuilder = PandoraUtils.StringBuilder;
        if (str1 != null)
        {
            stringBuilder.Append(str1);
        }
        if (str2 != null)
        {
            stringBuilder.Append(str2);
        }
        if (str3 != null)
        {
            stringBuilder.Append(str3);
        }
        if (str4 != null)
        {
            stringBuilder.Append(str4);
        }
        return GetStringById(stringBuilder, emptyArray);
    }

    public string GetStringById(StringBuilder key, params string[] parameters)
    {
        return GetStringById2(key, parameters);
    }

    public string GetStringById(StringBuilder key)
    {
        return GetStringById2(key, emptyArray);
    }

    private string GetStringById2(StringBuilder key, string[] parameters)
    {
        //Discarded unreachable code: IL_0098
        uint key2 = FNV1a.ComputeHash(key);
        if (language.ContainsKey(key2))
        {
            try
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i][0] == '#')
                    {
                        parameters[i] = GetStringById(parameters[i].Replace("#", string.Empty));
                    }
                }
                if (parameters.Length > 0)
                {
                    StringBuilder stringBuilder = PandoraUtils.StringBuilder;
                    stringBuilder.AppendFormat(language[key2], parameters);
                    return stringBuilder.ToString();
                }
                return language[key2];
            }
            catch
            {
            }
        }
        else if (key.Length > 0 && key[0] == '#')
        {
            return Regex.Replace(key.ToString(), "(?<![=])#(\\w+)", ConvertMatchToLocalization);
        }
        return "++" + key;
    }

    public string GetStringById(string key, params string[] parameters)
    {
        return GetStringById2(key, parameters);
    }

    public string GetStringById(string key)
    {
        return GetStringById2(key, emptyArray);
    }

    private string GetStringById2(string key, string[] parameters)
    {
        //Discarded unreachable code: IL_0098
        uint key2 = FNV1a.ComputeHash(key);
        if (language.ContainsKey(key2))
        {
            try
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i][0] == '#')
                    {
                        parameters[i] = GetStringById(parameters[i].Replace("#", string.Empty));
                    }
                }
                if (parameters.Length > 0)
                {
                    StringBuilder stringBuilder = PandoraUtils.StringBuilder;
                    stringBuilder.AppendFormat(language[key2], parameters);
                    return stringBuilder.ToString();
                }
                return language[key2];
            }
            catch
            {
            }
        }
        else if (key.StartsWith("#", StringComparison.OrdinalIgnoreCase))
        {
            return Regex.Replace(key, "(?<![=])#(\\w+)", ConvertMatchToLocalization);
        }
        return "++" + key;
    }

    private string ConvertMatchToLocalization(Match match)
    {
        return GetStringById(match.Value.Replace("#", string.Empty));
    }

    public void ParseFile(TextAsset file)
    {
        string text = file.text;
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary = JsonReader.Deserialize<Dictionary<string, string>>(text);
        language = new Dictionary<uint, string>();
        foreach (string key in dictionary.Keys)
        {
            language[Convert.ToUInt32(key)] = dictionary[key];
        }
    }

    public string ReplaceAllActionsWithButtonName(string inputStr)
    {
        //IL_015d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0169: Unknown result type (might be due to invalid IL or missing references)
        //IL_01e6: Unknown result type (might be due to invalid IL or missing references)
        //IL_0216: Unknown result type (might be due to invalid IL or missing references)
        //IL_021c: Invalid comparison between Unknown and I4
        string text = inputStr;
        foreach (InputAction item in ReInput.get_mapping().ActionsInCategory("game_input"))
        {
            string[] array = new string[3]
            {
                "[" + item.get_name() + "]",
                "[" + item.get_name() + "-]",
                "[" + item.get_name() + "+]"
            };
            List<ActionElementMap> list = null;
            if (PandoraSingleton<PandoraInput>.Instance.lastInputMode == PandoraInput.InputMode.JOYSTICK)
            {
                list = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.ElementMapsWithAction((ControllerType)2, item.get_name(), true).ToDynList();
            }
            else
            {
                list = PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.ButtonMapsWithAction((ControllerType)0, item.get_name(), true).ToDynList();
                list.AddRange(PandoraSingleton<PandoraInput>.Instance.player.controllers.maps.ButtonMapsWithAction((ControllerType)1, item.get_name(), true).ToDynList());
            }
            for (int i = 0; i < array.Length; i++)
            {
                string text2 = string.Empty;
                List<string> list2 = new List<string>();
                for (int j = 0; j < list.Count; j++)
                {
                    ActionElementMap val = list[j];
                    string controllerKeyString = GetControllerKeyString(val.get_elementIdentifierName());
                    if (!list2.Contains(controllerKeyString))
                    {
                        list2.Add(controllerKeyString);
                        if ((int)val.get_elementType() == 0 && (int)val.get_axisRange() == 0)
                        {
                            switch (i)
                            {
                                case 0:
                                    text2 += controllerKeyString;
                                    break;
                                case 1:
                                    text2 += GetControllerKeyString(val.get_elementIdentifierName() + "_-");
                                    break;
                                case 2:
                                    text2 += GetControllerKeyString(val.get_elementIdentifierName() + "_+");
                                    break;
                            }
                        }
                        else if ((int)val.get_axisContribution() == 0 && (i == 2 || i == 0))
                        {
                            text2 = text2 + controllerKeyString + " / ";
                        }
                        else if ((int)val.get_axisContribution() == 1 && i == 1)
                        {
                            text2 = text2 + controllerKeyString + " / ";
                        }
                    }
                }
                text = text.Replace(array[i], text2.TrimEnd(' ', '/'));
            }
        }
        return text;
    }

    public string GetControllerKeyString(string elementIdentifierName)
    {
        elementIdentifierName = "key_" + elementIdentifierName.ToLowerInvariant().Replace(" ", "_");
        if (elementIdentifierName.Equals("key_mouse_horizontal") || elementIdentifierName.Equals("key_mouse_vertical"))
        {
            elementIdentifierName = "key_mouse_move";
        }
        return PandoraSingleton<LocalizationManager>.Instance.GetStringById(elementIdentifierName);
    }
}
